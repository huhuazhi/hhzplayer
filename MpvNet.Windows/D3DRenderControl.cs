using System;
using System.Drawing;
using System.Windows.Forms;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

// 方便完全限定，避免名字冲突
using DXGI = Vortice.DXGI;
using D2D1 = Vortice.Direct2D1;
using DWrite = Vortice.DirectWrite;
using static Vortice.Direct3D11.D3D11;

namespace MpvNet.Windows
{
    /// <summary>
    /// 通用 D3D11 + DXGI + D2D1 + DWrite 渲染宿主控件（WinForms）
    /// - 60FPS 定时器驱动 Render()
    /// - Resize 自动重建 SwapChain 目标
    /// - 暴露 D2D/DWrite 资源给子类（重写 OnRender）
    /// </summary>
    public class D3DRenderControl : Control
    {
        // DXGI / D3D
        protected IDXGIFactory2 _factory;
        protected ID3D11Device _device;
        protected ID3D11DeviceContext _context;
        protected IDXGISwapChain1 _swapChain;
        protected ID3D11Texture2D _backBuffer;
        protected ID3D11RenderTargetView _rtv;

        // D2D / DWrite
        protected ID2D1Factory1 _d2dFactory;
        protected IDWriteFactory _dwriteFactory;
        protected ID2D1Device _d2dDevice;
        protected ID2D1DeviceContext _d2dContext;
        protected ID2D1Bitmap1 _d2dTarget;

        private readonly Timer _renderTimer;

        public D3DRenderControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.Opaque |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            DoubleBuffered = false;
            BackColor = System.Drawing.Color.Black;

            _renderTimer = new Timer { Interval = 1000 / 60 }; // 60 FPS
            _renderTimer.Tick += (_, __) =>
            {
                try { Render(); } catch { /*避免异常卡死消息泵*/ }
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            var flags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif

            // 创建设备
            D3D11CreateDevice(null, DriverType.Hardware, flags,
                new[] { Vortice.Direct3D.FeatureLevel.Level_11_0 },
                out _device, out _context);

            // 获取 DXGI 工厂
            using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
            using var adapter = dxgiDevice.GetAdapter();
            using var factory = adapter.GetParent<IDXGIFactory2>();

            var desc = new SwapChainDescription1
            {
                Width = (uint)Width,
                Height = (uint)Height,
                Format = Format.B8G8R8A8_UNorm,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = new SampleDescription(1, 0),
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore
            };

            _swapChain = factory.CreateSwapChainForHwnd(_device, Handle, desc);
        }


        protected override void OnHandleDestroyed(EventArgs e)
        {
            _renderTimer.Stop();
            DisposeDeviceResources();
            base.OnHandleDestroyed(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (DesignMode) return;
            if (_device == null || _swapChain == null) return;
            if (Width <= 0 || Height <= 0) return;

            RecreateSizeDependentResources();
        }

        #region Init / Dispose
        private void InitDeviceResources()
        {
            // 工厂
            _factory = DXGI.DXGI.CreateDXGIFactory2<IDXGIFactory2>(
#if DEBUG
                true
#else
        false
#endif
            );

            // D3D11 设备（要 BGRA 支持给 D2D 用）
            var flags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif

            Vortice.Direct3D.FeatureLevel[] levels =
            {
                Vortice.Direct3D.FeatureLevel.Level_11_1,
                Vortice.Direct3D.FeatureLevel.Level_11_0,
                Vortice.Direct3D.FeatureLevel.Level_10_1,
                Vortice.Direct3D.FeatureLevel.Level_10_0
            };

            var result = D3D11CreateDevice(
                null,              // 默认适配器
                DriverType.Hardware,
                flags,
                levels,
                out _device,
                out _context
            );

            if (result.Failure || _device == null)
            {
                throw new InvalidOperationException(
                    $"D3D11CreateDevice failed: {result}");
            }

            // D2D / DWrite
            _d2dFactory = D2D1.D2D1.D2D1CreateFactory<ID2D1Factory1>();
            _dwriteFactory = DWrite.DWrite.DWriteCreateFactory<IDWriteFactory>();

            using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
            using var adapter = dxgiDevice.GetAdapter();
            using var factory = adapter.GetParent<IDXGIFactory2>();
            _d2dDevice = _d2dFactory.CreateDevice(dxgiDevice);
            _d2dContext = _d2dDevice.CreateDeviceContext(DeviceContextOptions.None);
        }


        private void InitSizeDependentResources()
        {
            if (Width <= 0 || Height <= 0) return;

            var desc = new SwapChainDescription1
            {
                Width = (uint)Width,   // 不要 Math.Max(1,..)，传 0 会触发 InvalidCall
                Height = (uint)Height,
                Format = Format.B8G8R8A8_UNorm,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipDiscard,
                Scaling = Scaling.Stretch,
                AlphaMode = DXGI.AlphaMode.Premultiplied // ⚠️ 别用 Ignore
            };

            // ⚠️ 确保 Handle 已创建
            if (!IsHandleCreated)
                CreateHandle();

            _swapChain = _factory.CreateSwapChainForHwnd(
    _device,           // 这里可以传 D3D11Device
    Handle,
    desc);

            CreateTargets();
        }

        private void RecreateSizeDependentResources()
        {
            if (_d2dContext != null)
                _d2dContext.Target = null;

            _d2dTarget?.Dispose();
            _rtv?.Dispose();
            _backBuffer?.Dispose();

            // 显式传 2 个缓冲，修复 uint/int 转换
            _swapChain.ResizeBuffers(
                2,
                (uint)Math.Max(1, Width),
                (uint)Math.Max(1, Height),
                Format.B8G8R8A8_UNorm,
                SwapChainFlags.None
            );
            CreateTargets();
        }

        private void CreateTargets()
        {
            _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _rtv = _device.CreateRenderTargetView(_backBuffer);

            using var surface = _swapChain.GetBuffer<IDXGISurface>(0);

            // 修复：在 Vortice 3.6.2 中使用 Vortice.DCommon.PixelFormat
            var bmpProps = new BitmapProperties1(
                new Vortice.DCommon.PixelFormat(
                    Format.B8G8R8A8_UNorm,
                    Vortice.DCommon.AlphaMode.Premultiplied // ⚠️ 必须和 SwapChain 匹配
                ),
                96, 96,
                BitmapOptions.Target | BitmapOptions.CannotDraw
            );


            _d2dTarget = _d2dContext.CreateBitmapFromDxgiSurface(surface, bmpProps);
            _d2dContext.Target = _d2dTarget;
            _d2dContext.TextAntialiasMode = Vortice.Direct2D1.TextAntialiasMode.Grayscale;
        }

        private void DisposeDeviceResources()
        {
            try
            {
                _d2dContext?.Dispose();
                _d2dTarget?.Dispose();
                _d2dDevice?.Dispose();
                _d2dFactory?.Dispose();

                _rtv?.Dispose();
                _backBuffer?.Dispose();
                _swapChain?.Dispose();

                _context?.ClearState();
                _context?.Flush();
                _context?.Dispose();
                _device?.Dispose();
                _factory?.Dispose();
                _dwriteFactory?.Dispose();
            }
            finally
            {
                _d2dContext = null;
                _d2dTarget = null;
                _d2dDevice = null;
                _d2dFactory = null;
                _rtv = null;
                _backBuffer = null;
                _swapChain = null;
                _context = null;
                _device = null;
                _factory = null;
                _dwriteFactory = null;
            }
        }
        #endregion

        /// <summary>每帧渲染，子类重写</summary>
        protected virtual void OnRender()
        {
            // 默认清屏；子类画 UI
            _context.OMSetRenderTargets(_rtv);
            _context.ClearRenderTargetView(_rtv, new Vortice.Mathematics.Color4(0, 0, 0, 1));

            _d2dContext.BeginDraw();
            _d2dContext.Clear(new Vortice.Mathematics.Color4(0, 0, 0, 0)); // D2D 顶层
            _d2dContext.EndDraw();
        }

        private void Render()
        {
            if (_device == null || _swapChain == null) return;

            OnRender();
            _swapChain.Present(1, PresentFlags.None); // vsync
        }

        // 禁用 WinForms 的 GDI 绘制（全部走 D3D/D2D）
        protected override void OnPaint(PaintEventArgs e) { }
        protected override void OnPaintBackground(PaintEventArgs pevent) { }
    }
}