using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

namespace MpvNet.Windows
{
    public class FileListD3D : D3DRenderControl
    {
        private readonly List<Item> _items = new();
        private int _rowHeight = 40;
        private int _scrollY = 0;
        private int _hoverIndex = -1;
        private int _selectedIndex = -1;
        private bool _dragging = false;
        private int _lastMouseY;

        private float[] _col = { 0.5f, 0.15f, 0.15f, 0.20f };

        // 画刷 / 文本格式
        private ID2D1SolidColorBrush? _bHeader, _bRow, _bAlt, _bHover, _bSel, _bText;
        private IDWriteTextFormat? _fmtHeader, _fmtRow;
        private bool _rtReady = false;

        public string CurrentPath { get; private set; } = string.Empty;

        public event EventHandler<string>? FileOpened;
        public event EventHandler<string>? DirectoryChanged;
        public event EventHandler<FileListD3DScrollEventArgs>? ViewportOffsetChanged;
        public event EventHandler<FileListD3DHoverEventArgs>? HoverChanged;

        public FileListD3D() : base()
        {
            // 输入事件
            MouseWheel += (_, e) => Scroll(e.Delta > 0 ? -120 : 120);

            MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _dragging = true;
                    _lastMouseY = e.Y;
                }
            };

            MouseMove += (_, e) =>
            {
                if (_dragging)
                {
                    int dy = e.Y - _lastMouseY;
                    _lastMouseY = e.Y;
                    Scroll(-dy);
                }
                else
                {
                    int idx = HitTestIndex(e.Location);
                    if (idx != _hoverIndex)
                    {
                        _hoverIndex = idx;
                        HoverChanged?.Invoke(this, new FileListD3DHoverEventArgs(idx));
                    }
                }
            };

            MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _dragging = false;
                    int idx = HitTestIndex(e.Location);
                    if (idx >= 0 && idx < _items.Count)
                    {
                        _selectedIndex = idx;
                        var it = _items[idx];
                        if (it.IsDir)
                        {
                            NavigateTo(it.FullPath, raiseEvent: true);
                        }
                        else
                        {
                            FileOpened?.Invoke(this, it.FullPath);
                        }
                    }
                }
            };
        }

        private void EnsureRtResources()
        {
            if (_rtReady) return;

            _bHeader = _d2dContext!.CreateSolidColorBrush(new Color4(0, 0.48f, 0.80f, 0.75f));
            _bRow = _d2dContext.CreateSolidColorBrush(new Color4(0, 0, 0, 0.35f));
            _bAlt = _d2dContext.CreateSolidColorBrush(new Color4(0, 0, 0, 0.25f));
            _bHover = _d2dContext.CreateSolidColorBrush(new Color4(0.0f, 0.47f, 0.84f, 0.35f));
            _bSel = _d2dContext.CreateSolidColorBrush(new Color4(0.0f, 0.47f, 0.84f, 0.55f));
            _bText = _d2dContext.CreateSolidColorBrush(new Color4(1, 1, 1, 1));

            _fmtHeader = _dwriteFactory!.CreateTextFormat("Segoe UI Semibold", 10.0f);
            _fmtHeader.TextAlignment = TextAlignment.Leading;
            _fmtHeader.ParagraphAlignment = ParagraphAlignment.Center;

            _fmtRow = _dwriteFactory.CreateTextFormat("Segoe UI", 10.0f);
            _fmtRow.TextAlignment = TextAlignment.Leading;
            _fmtRow.ParagraphAlignment = ParagraphAlignment.Center;

            _rtReady = true;
        }

        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = Math.Max(24, value); }
        }

        public int ScrollOffset
        {
            get => _scrollY;
            set
            {
                int maxScroll = Math.Max(0, _items.Count * _rowHeight - Math.Max(0, Height - _rowHeight));
                int clamped = Math.Clamp(value, 0, maxScroll);
                if (clamped != _scrollY)
                {
                    _scrollY = clamped;
                    ViewportOffsetChanged?.Invoke(this, new FileListD3DScrollEventArgs(_scrollY));
                }
            }
        }

        public void NavigateTo(string path, bool raiseEvent)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            CurrentPath = path;

            _items.Clear();

            var diRoot = new DirectoryInfo(path);
            if (diRoot.Parent != null)
            {
                _items.Add(new Item
                {
                    Name = "..",
                    FullPath = diRoot.Parent.FullName,
                    IsDir = true,
                    Size = -1,
                    Type = "上一级目录",
                    Modified = DateTime.Now
                });
            }

            try
            {
                foreach (var d in Directory.EnumerateDirectories(path))
                {
                    var di = new DirectoryInfo(d);
                    _items.Add(new Item
                    {
                        Name = di.Name,
                        FullPath = d,
                        IsDir = true,
                        Size = -1,
                        Type = "文件夹",
                        Modified = di.LastWriteTime
                    });
                }
            }
            catch { }

            try
            {
                foreach (var f in Directory.EnumerateFiles(path))
                {
                    var fi = new FileInfo(f);
                    _items.Add(new Item
                    {
                        Name = fi.Name,
                        FullPath = f,
                        IsDir = false,
                        Size = fi.Length,
                        Type = string.IsNullOrEmpty(fi.Extension) ? "文件" : fi.Extension.ToUpperInvariant(),
                        Modified = fi.LastWriteTime
                    });
                }
            }
            catch { }

            _scrollY = 0;
            if (raiseEvent) DirectoryChanged?.Invoke(this, CurrentPath);
        }

        protected override void OnRender()
        {
            EnsureRtResources();

            _context!.OMSetRenderTargets(_rtv);
            _context.ClearRenderTargetView(_rtv!, new Color4(0, 0, 0, 1));

            _d2dContext!.BeginDraw();
            _d2dContext.Clear(new Color4(0, 0, 0, 0));

            int w = Math.Max(1, Width);
            int h = Math.Max(1, Height);

            // Header
            int headerH = _rowHeight;
            _d2dContext.FillRectangle(new RectangleF(0, 0, w, headerH), _bHeader!);

            int x1 = (int)(w * _col[0]);
            int x2 = x1 + (int)(w * _col[1]);
            int x3 = x2 + (int)(w * _col[2]);

            DrawText("名称", 8, 0, (int)(w * _col[0]) - 16, headerH, _fmtHeader!);
            DrawText("大小", x1 + 6, 0, (int)(w * _col[1]) - 12, headerH, _fmtHeader!);
            DrawText("类型", x2 + 6, 0, (int)(w * _col[2]) - 12, headerH, _fmtHeader!);
            DrawText("修改日期", x3 + 6, 0, (int)(w * _col[3]) - 12, headerH, _fmtHeader!);

            // rows
            int viewportH = h - headerH;
            if (viewportH > 0)
            {
                int first = Math.Max(0, _scrollY / _rowHeight);
                int last = Math.Min(_items.Count - 1, (_scrollY + viewportH) / _rowHeight);

                for (int i = first; i <= last; i++)
                {
                    int rowY = headerH + i * _rowHeight - _scrollY;
                    var rowRect = new RectangleF(0, rowY, w, _rowHeight);

                    _d2dContext.FillRectangle(rowRect, (i & 1) == 1 ? _bAlt! : _bRow!);
                    if (i == _hoverIndex) _d2dContext.FillRectangle(rowRect, _bHover!);
                    if (i == _selectedIndex) _d2dContext.FillRectangle(rowRect, _bSel!);

                    var it = _items[i];

                    DrawText(it.Name, 8, rowY, (int)(w * _col[0]) - 16, _rowHeight, _fmtRow!);
                    DrawText(it.Size < 0 ? "" : FormatSize(it.Size), x1 + 6, rowY, (int)(w * _col[1]) - 12, _rowHeight, _fmtRow!);
                    DrawText(it.Type, x2 + 6, rowY, (int)(w * _col[2]) - 12, _rowHeight, _fmtRow!);
                    DrawText(it.Modified.ToString("yyyy-MM-dd HH:mm"), x3 + 6, rowY, (int)(w * _col[3]) - 12, _rowHeight, _fmtRow!);
                }
            }

            _d2dContext.EndDraw();
        }

        private void DrawText(string text, float x, float y, float w, float h, IDWriteTextFormat fmt)
        {
            using var layout = _dwriteFactory!.CreateTextLayout(text ?? "", fmt, w, h);
            _d2dContext!.DrawTextLayout(new System.Numerics.Vector2(x, y), layout, _bText!);
        }

        private int HitTestIndex(Point client)
        {
            if (client.Y < _rowHeight) return -1;
            int contentY = client.Y - _rowHeight + _scrollY;
            int idx = contentY / _rowHeight;
            return (idx >= 0 && idx < _items.Count) ? idx : -1;
        }

        private void Scroll(int deltaPixels)
        {
            int contentH = _items.Count * _rowHeight;
            int viewportH = Math.Max(0, Height - _rowHeight);
            int maxScroll = Math.Max(0, contentH - viewportH);

            int newVal = Math.Clamp(_scrollY + deltaPixels, 0, maxScroll);
            if (newVal != _scrollY)
            {
                _scrollY = newVal;
                ViewportOffsetChanged?.Invoke(this, new FileListD3DScrollEventArgs(_scrollY));
            }
        }

        private static string FormatSize(long size)
        {
            const double KB = 1024.0, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            if (size >= TB) return $"{size / TB:0.##} TB";
            if (size >= GB) return $"{size / GB:0.##} GB";
            if (size >= MB) return $"{size / MB:0.##} MB";
            if (size >= KB) return $"{size / KB:0.##} KB";
            return $"{size} B";
        }

        private class Item
        {
            public string Name = "";
            public string FullPath = "";
            public bool IsDir;
            public long Size;
            public string Type = "";
            public DateTime Modified;
        }
    }

    // ✅ 避免与现有 FileList.cs 的事件类型重名
    public sealed class FileListD3DScrollEventArgs : EventArgs
    {
        public int OffsetY { get; }
        public FileListD3DEventArgsKind Kind => FileListD3DEventArgsKind.Scroll;
        public FileListD3DScrollEventArgs(int offsetY) => OffsetY = offsetY;
    }

    public sealed class FileListD3DHoverEventArgs : EventArgs
    {
        public int Index { get; }
        public FileListD3DEventArgsKind Kind => FileListD3DEventArgsKind.Hover;
        public FileListD3DHoverEventArgs(int index) => Index = index;
    }

    public enum FileListD3DEventArgsKind { Scroll, Hover }
}
