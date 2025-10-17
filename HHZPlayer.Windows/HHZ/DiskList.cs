using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace HHZPlayer.Windows.HHZ
{
    /// <summary>
    /// 透明背景自绘磁盘列表：上部圆角标题栏 + 两排文字 + 进度条 + 系统图标
    /// 不显示滚动条，按住鼠标拖动。标题栏固定并遮挡上滚内容。
    /// 暴露滚动/选中/悬浮同步接口与事件。支持整体四角圆角（上/下半径可独立设置）。
    /// </summary>
    public sealed class DiskList : ScrollableControl
    {
        // —— 标题栏 —— 
        public bool ShowHeader { get; set; } = true;
        public string HeaderText { get; set; } = "此电脑";

        private int _headerHeight = 50;
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(20, value); Invalidate(); ClampScroll(); Update(); }
        }

        public Color HeaderBackColor { get; set; } = Color.FromArgb(64, 80, 140, 255); // 半透明蓝
        public Padding HeaderPadding { get; set; } = new Padding(12, 6, 12, 6);

        /// <summary>顶部圆角半径（影响控件 Region 以及标题栏顶部的圆角视觉）</summary>
        public int CornerRadiusTop
        {
            get => _cornerRadiusTop;
            set { _cornerRadiusTop = Math.Max(0, value); UpdateRegion(); Invalidate(); Update(); }
        }
        private int _cornerRadiusTop = 10;

        /// <summary>底部圆角半径（影响控件 Region 的左下/右下角）</summary>
        public int CornerRadiusBottom
        {
            get => _cornerRadiusBottom;
            set { _cornerRadiusBottom = Math.Max(0, value); UpdateRegion(); Invalidate(); Update(); }
        }
        private int _cornerRadiusBottom = 10;

        // —— 行高/行距 —— 
        private int _rowHeight = 68;
        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = Math.Max(24, value); RecalcContent(); Invalidate(); Update(); }
        }

        private int _rowSpacing = 20;
        public int RowSpacing
        {
            get => _rowSpacing;
            set { _rowSpacing = Math.Max(0, value); RecalcContent(); Invalidate(); Update(); }
        }

        public bool ShowNotReady { get; set; } = true;

        // —— 同步接口/事件 —— 
        public sealed class ViewportOffsetChangedEventArgs : EventArgs
        {
            public int OffsetY { get; }
            public ViewportOffsetChangedEventArgs(int y) { OffsetY = y; }
        }
        public event EventHandler<ViewportOffsetChangedEventArgs>? ViewportOffsetChanged;

        public sealed class SelectionChangedEventArgs : EventArgs
        {
            public int Index { get; }
            public string Root { get; }
            public SelectionChangedEventArgs(int i, string r) { Index = i; Root = r; }
        }
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public sealed class HoverChangedEventArgs : EventArgs
        {
            public int Index { get; }
            public HoverChangedEventArgs(int index) { Index = index; }
        }
        public event EventHandler<HoverChangedEventArgs>? HoverChanged;

        public int SelectedIndex => _selectedIndex;
        public int HotIndex => _hotIndex;

        /// <summary>外部可设滚动位移（<=0），触发 ViewportOffsetChanged。</summary>
        public int ScrollOffset
        {
            get => _scrollY;
            set
            {
                int old = _scrollY;
                _scrollY = value;
                ClampScroll();
                if (_scrollY != old)
                {
                    Invalidate();
                    Update();
                    ViewportOffsetChanged?.Invoke(this, new ViewportOffsetChangedEventArgs(_scrollY));
                }
            }
        }

        /// <summary>外部设置选中行；ensureVisible=true 自动滚入可视；raiseEvent=true 触发 SelectionChanged。</summary>
        public void SelectIndex(int index, bool ensureVisible = true, bool raiseEvent = false)
            => SetSelectedIndex(index, ensureVisible, raiseEvent);

        public void EnsureIndexVisible(int index)
        {
            int viewportTop = ShowHeader ? HeaderHeight : 0;
            int viewportHeight = Math.Max(0, ClientSize.Height - viewportTop);
            if (index < 0 || index >= _items.Count || viewportHeight <= 0) return;

            int stride = RowHeight + RowSpacing;
            int rowTopContent = index * stride;
            int rowBottomContent = rowTopContent + RowHeight;

            int viewStartContent = -_scrollY;
            int viewEndContent = viewStartContent + viewportHeight;

            int newScroll = _scrollY;
            if (rowTopContent < viewStartContent)
                newScroll = -rowTopContent;
            else if (rowBottomContent > viewEndContent)
                newScroll = -(rowBottomContent - viewportHeight);

            ScrollOffset = newScroll;
        }

        /// <summary>外部同步悬浮高亮；raiseEvent=true 触发 HoverChanged（用于来源侧）。</summary>
        public void SetHotIndex(int index, bool raiseEvent = false)
        {
            if (_hotIndex != index)
            {
                _hotIndex = index;
                Invalidate();
                Update();
                if (raiseEvent)
                    HoverChanged?.Invoke(this, new HoverChangedEventArgs(_hotIndex));
            }
        }

        // 你已有的“选择盘符”事件
        public event EventHandler<string>? DiskSelected;

        // —— 内部状态 —— 
        private readonly List<Item> _items = new();
        private int _hotIndex = -1;
        private int _selectedIndex = -1;
        private bool _paintingBack = false;

        private readonly Dictionary<string, Icon> _iconCache = new(StringComparer.OrdinalIgnoreCase);

        // 拖拽滚动
        private int _contentHeight = 0;
        private int _scrollY = 0; // <= 0
        private bool _dragging = false;
        private int _dragStartY = 0;
        private int _dragStartScrollY = 0;

        // Loading animation
        private Timer? _animTimer;
        private float _spinAngle = 0f;
        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                if (_animTimer == null)
                {
                    _animTimer = new Timer();
                    _animTimer.Interval = 80;
                    _animTimer.Tick += (s, e) => { _spinAngle += 20f; if (_spinAngle >= 360f) _spinAngle -= 360f; Invalidate(); };
                }
                if (_isLoading) _animTimer.Start(); else _animTimer.Stop();
                Invalidate();
            }
        }

        public DiskList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            DoubleBuffered = true;
            AutoScroll = false; // 不用 WinForms 滚动条
            Cursor = Cursors.Hand;

            MouseMove += DiskList_MouseMove;
            MouseLeave += DiskList_MouseLeave;
            MouseDown += DiskList_MouseDown;
            MouseUp += DiskList_MouseUp;
            MouseWheel += DiskList_MouseWheel;
        }

        // —— 真透明：把父控件在本区域的背景拷贝过来 —— 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent == null || _paintingBack) return;
            try
            {
                _paintingBack = true;

                // 确保背景绘制遵循控件的圆角 Region（如果已设置）
                if (Region != null)
                    e.Graphics.SetClip(Region, CombineMode.Replace);

                Rectangle rectInParent = new Rectangle(Left, Top, Width, Height);
                var g = e.Graphics;
                var state = g.Save();
                g.TranslateTransform(-rectInParent.X, -rectInParent.Y);
                using var pe = new PaintEventArgs(g, rectInParent);
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
                g.Restore(state);
            }
            finally { _paintingBack = false; }
        }

        // 边框宽度（可调），以及对应的内边距（向内缩进避免内容压线）
        private float _borderWidth = 1.5f;
        private int BorderPad => (int)Math.Ceiling(_borderWidth);

        // 计算内矩形（内容真正可绘制区域 = 去掉四周边框的区域）
        private Rectangle GetInnerRect()
        {
            int pad = BorderPad;
            return new Rectangle(pad, pad, Math.Max(0, ClientSize.Width - pad * 2), Math.Max(0, ClientSize.Height - pad * 2));
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 让整体仍遵循圆角 Region
            if (Region != null)
                g.SetClip(Region, CombineMode.Replace);

            // ★ 新：内矩形（扣除边框的内容区）
            Rectangle inner = GetInnerRect();

            int headerTop = inner.Top;
            int headerHeight = ShowHeader ? HeaderHeight : 0;
            int viewportTop = headerTop + headerHeight;      // ★ 列表从内矩形的标题栏下方开始
            int viewportBottom = inner.Bottom;

            // —— 标题栏（上部圆角，限制在 inner 宽度）——
            if (ShowHeader)
            {
                var headerRect = new Rectangle(inner.Left, inner.Top, inner.Width, HeaderHeight);
                using (var path = CreateTopRoundedRect(headerRect, CornerRadiusTop))
                using (var b = new SolidBrush(HeaderBackColor))
                    g.FillPath(b, path);

                using var fTitle = MakeFont("Segoe UI Semibold", 10f, FontStyle.Regular, "Segoe UI", FontStyle.Bold);
                using var brTitle = new SolidBrush(Color.White);
                var textRect = new Rectangle(headerRect.Left + HeaderPadding.Left,
                                             headerRect.Top + HeaderPadding.Top,
                                             headerRect.Width - HeaderPadding.Horizontal,
                                             headerRect.Height - HeaderPadding.Vertical);
                var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(HeaderText, fTitle, brTitle, textRect, sf);

                // 绘制 loading spinner（在 header 右侧）
                if (IsLoading)
                {
                    int size = Math.Min(24, HeaderHeight - 10);
                    var cx = headerRect.Right - HeaderPadding.Right - size / 2 - 4;
                    var cy = headerRect.Top + headerRect.Height / 2;
                    var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);
                    using var pen = new Pen(Color.White, 2f);
                    // 画部分弧以表现旋转
                    g.DrawArc(pen, rect, _spinAngle, 270f);
                }
            }

            // —— 列表内容（裁剪到 inner 的标题栏以下）——
            var clipRect = new Rectangle(inner.Left, viewportTop, inner.Width, Math.Max(0, viewportBottom - viewportTop));
            Region oldClip = g.Clip;
            using (var listClip = new Region(clipRect))
            {
                listClip.Intersect(g.Clip);
                g.Clip = listClip;

                // （你之前加的）深蓝半透明背景，记得也用 clipRect
                using (var listBg = new SolidBrush(Color.FromArgb(70, 50, 50, 50)))
                    g.FillRectangle(listBg, clipRect);

                int y = viewportTop + _scrollY;

                using var f1 = MakeFont("Segoe UI Semibold", 12f, FontStyle.Regular, "Segoe UI", FontStyle.Bold);
                using var f2 = new Font("Segoe UI", 9.5f, FontStyle.Regular);
                using var br1 = new SolidBrush(Color.White);
                using var br2 = new SolidBrush(Color.FromArgb(215, 220, 230));
                using var barBg = new SolidBrush(Color.FromArgb(80, 110, 120, 140));
                using var barFg = new SolidBrush(Color.FromArgb(200, 120, 180, 255));
                using var hover = new SolidBrush(Color.FromArgb(22, 120, 170, 255));
                using var sel = new SolidBrush(Color.FromArgb(40, 120, 170, 255));

                int contentWidth = inner.Width - 1;                 // ★ 宽度用 inner
                int rowLeft = inner.Left;                           // ★ X 从 inner.Left 开始

                for (int i = 0; i < _items.Count; i++)
                {
                    var it = _items[i];
                    var rowRect = new Rectangle(rowLeft, y, contentWidth, RowHeight);

                    if (rowRect.Bottom >= viewportTop && rowRect.Top <= viewportBottom)
                    {
                        Rectangle overlay = rowRect;
                        overlay.Y = Math.Max(viewportTop, overlay.Y - 2);
                        overlay.Height = Math.Min(viewportBottom - overlay.Y, overlay.Height + 4);

                        if (i == _selectedIndex) g.FillRectangle(sel, overlay);
                        else if (i == _hotIndex) g.FillRectangle(hover, overlay);

                        int icoSize = 48;
                        var icoRect = new Rectangle(rowRect.X + 12, rowRect.Y + (RowHeight - icoSize) / 2, icoSize, icoSize);
                        var icon = GetDriveIcon(it.Root);
                        if (icon != null) g.DrawIcon(icon, icoRect);

                        int textX = icoRect.Right + 10;
                        int textW = rowRect.Right - 12 - textX;
                        var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };

                        float line1Top = rowRect.Y + 6;
                        g.DrawString(it.Line1, f1, br1, new RectangleF(textX, line1Top, textW, f1.Height), fmt);

                        float line2Top = line1Top + f1.Height + 2;
                        g.DrawString(it.Line2, f2, br2, new RectangleF(textX, line2Top, textW, f2.Height), fmt);

                        int barLeft = textX;
                        int barRight = rowRect.Right - 12;
                        int barY = rowRect.Bottom - 20;
                        g.FillRectangle(barBg, barLeft, barY, barRight - barLeft, 2);
                        g.FillRectangle(barFg, barLeft, barY, (int)((barRight - barLeft) * it.Usage01), 2);
                    }

                    y += RowHeight + RowSpacing;
                }
            }
            g.Clip = oldClip;

            // —— 最后：画外圈蓝色圆角边框（仍然在控件最外）——
            using (var pen = new Pen(Color.FromArgb(180, 80, 140, 255), _borderWidth))
            {
                pen.Alignment = PenAlignment.Inset;
                var outerRect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

                using (var path = CreateRoundedRectPath(
                    outerRect,
                    CornerRadiusTop, CornerRadiusTop,
                    CornerRadiusBottom, CornerRadiusBottom))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private Point _mouseDownPos;
        private int _mouseDownIndex = -1;
        private const int ClickMoveTolerance = 5;
        // —— 输入/滚动 —— 
        private void DiskList_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _dragging = true;
            _dragStartY = e.Y;
            _dragStartScrollY = _scrollY;
            Capture = true;

            if (ShowHeader && e.Y < HeaderHeight) return;

            _mouseDownPos = e.Location;
            _mouseDownIndex = HitTest(e.Location);
        }

        private void DiskList_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                int delta = e.Y - _dragStartY;
                ScrollOffset = _dragStartScrollY + delta; // 走属性，触发同步事件
                return;
            }

            if (ShowHeader && e.Y < HeaderHeight)
            {
                if (_hotIndex != -1) SetHotIndex(-1, true);
                return;
            }

            int idx = HitTest(e.Location);
            SetHotIndex(idx, true); // 广播 HoverChanged
        }

        private void DiskList_MouseLeave(object? sender, EventArgs e)
        {
            if (!_dragging)
                SetHotIndex(-1, true); // 鼠标离开控件，清空 hover
        }

        private void DiskList_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _dragging = false;
            Capture = false;

            if (_mouseDownIndex >= 0)
            {
                int upIndex = HitTest(e.Location);

                bool isClick =
                    upIndex == _mouseDownIndex &&
                    Math.Abs(e.X - _mouseDownPos.X) <= ClickMoveTolerance &&
                    Math.Abs(e.Y - _mouseDownPos.Y) <= ClickMoveTolerance;

                if (isClick)
                {
                    SetSelectedIndex(upIndex, ensureVisible: false, raiseEvent: true);
                    DiskSelected?.Invoke(this, _items[upIndex].Root);
                }
            }

            _mouseDownIndex = -1;
        }


        private void DiskList_MouseWheel(object? sender, MouseEventArgs e)
        {
            int stride = RowHeight + RowSpacing;
            ScrollOffset = _scrollY + Math.Sign(e.Delta) * 3 * stride; // 走属性，触发同步事件
        }

        private int HitTest(Point ptClient)
        {
            int viewportTop = ShowHeader ? HeaderHeight : 0;
            if (ptClient.Y < viewportTop) return -1;

            int y = ptClient.Y - viewportTop - _scrollY;
            if (y < 0) return -1;

            int stride = RowHeight + RowSpacing;
            int idx = y / stride;
            int topOfRow = idx * stride;
            if (idx >= 0 && idx < _items.Count && y - topOfRow < RowHeight) return idx;
            return -1;
        }

        private void SetSelectedIndex(int index, bool ensureVisible, bool raiseEvent)
        {
            if (index < 0 || index >= _items.Count) return;
            if (_selectedIndex != index)
            {
                _selectedIndex = index;
                if (ensureVisible) EnsureIndexVisible(index);
                Invalidate();
                Update();
                if (raiseEvent)
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(index, _items[index].Root));
            }
        }

        // —— 数据 —— 
        private sealed class Item
        {
            public string Root = "";
            public string Line1 = "";
            public string Line2 = "";
            public float Usage01 = 0f;
        }

        public void Reload()
        {
            _items.Clear();
            foreach (var di in DriveInfo.GetDrives())
            {
                if (di.DriveType == DriveType.CDRom) continue;
                if (!ShowNotReady && !SafeIsReady(di)) continue;

                _items.Add(BuildItem(di));
            }
            RecalcContent();
            Invalidate();
            Update();
        }

        // 异步版：在后台枚举驱动并在 UI 线程更新，避免阻塞主线程（用于网络盘等可能耗时的情况）
        public async Task ReloadAsync()
        {
            var list = await Task.Run(() =>
            {
                var tmp = new List<Item>();
                try
                {
                    foreach (var di in DriveInfo.GetDrives())
                    {
                        if (di.DriveType == DriveType.CDRom) continue;
                        if (!ShowNotReady && !SafeIsReady(di)) continue;

                        var it = new Item { Root = di.RootDirectory.FullName };
                        try
                        {
                            if (di.IsReady)
                            {
                                string vol = di.VolumeLabel; if (string.IsNullOrWhiteSpace(vol)) vol = "本地磁盘";
                                long total = 0, free = 0, used = 0;
                                try { total = di.TotalSize; free = di.TotalFreeSpace; used = total - free; } catch { }
                                it.Line1 = $"{vol} ({it.Root.TrimEnd('\\')})".Trim();
                                it.Usage01 = total > 0 ? (float)used / total : 0f;
                                it.Line2 = total > 0 ? $"{Fmt(free)} 可用, 共 {Fmt(total)}" : "不可用";
                            }
                            else
                            {
                                it.Line1 = $"({it.Root.TrimEnd('\\')})";
                                it.Usage01 = 0f;
                                it.Line2 = "不可用";
                            }
                        }
                        catch { it.Line1 = it.Root; it.Line2 = "不可用"; it.Usage01 = 0f; }

                        tmp.Add(it);
                    }
                }
                catch { }
                return tmp;
            }).ConfigureAwait(false);

            try
            {
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        _items.Clear();
                        _items.AddRange(list);
                        RecalcContent();
                        Invalidate();
                        Update();
                    }));
                }
                else
                {
                    _items.Clear();
                    _items.AddRange(list);
                    RecalcContent();
                    Invalidate();
                    Update();
                }
            }
            catch { }
        }

        private void RecalcContent()
        {
            _contentHeight = _items.Count == 0 ? 0 : _items.Count * (RowHeight + RowSpacing) - RowSpacing;
            ClampScroll();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            // 更新控件圆角区域（包括底部两个角）
            UpdateRegion();
            ClampScroll();
            Invalidate();
            Update();
        }

        private void UpdateRegion()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                Region = null;
                return;
            }

            using (var path = CreateRoundedRectPath(new Rectangle(Point.Empty, ClientSize),
                                                    CornerRadiusTop, CornerRadiusTop,
                                                    CornerRadiusBottom, CornerRadiusBottom))
            {
                // 设置 Region 使控件四角都遵循圆角，命中与绘制统一
                Region = new Region(path);
            }
        }

        private void ClampScroll()
        {
            int viewportTop = ShowHeader ? HeaderHeight : 0;
            int viewportHeight = Math.Max(0, ClientSize.Height - viewportTop);
            int min = Math.Min(0, viewportHeight - _contentHeight);
            _scrollY = _contentHeight <= viewportHeight ? 0 : Math.Max(min, Math.Min(0, _scrollY));
        }

        private static bool SafeIsReady(DriveInfo d) { try { return d.IsReady; } catch { return false; } }

        private static string Fmt(long bytes)
        {
            string[] u = { "B", "KB", "MB", "GB", "TB", "PB" };
            double v = bytes; int i = 0; while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
            return $"{v:0.##} {u[i]}";
        }

        private Item BuildItem(DriveInfo di)
        {
            var it = new Item { Root = di.RootDirectory.FullName };
            string vol = ""; long total = 0, free = 0, used = 0;
            try
            {
                if (di.IsReady)
                {
                    vol = di.VolumeLabel; if (string.IsNullOrWhiteSpace(vol)) vol = "本地磁盘";
                    total = di.TotalSize; free = di.TotalFreeSpace; used = total - free;
                }
            }
            catch { }
            it.Line1 = $"{vol} ({it.Root.TrimEnd('\\')})".Trim();
            it.Usage01 = total > 0 ? (float)used / total : 0f;
            it.Line2 = total > 0 ? $"{Fmt(free)} 可用, 共 {Fmt(total)}" : "不可用";
            GetDriveIcon(it.Root); // 预热
            return it;
        }

        private static Font MakeFont(string family, float size, FontStyle style, string fallbackFamily, FontStyle fallbackStyle)
        {
            try { return new Font(family, size, style, GraphicsUnit.Point); }
            catch { return new Font(fallbackFamily, size, fallbackStyle); }
        }

        // —— Shell 图标 —— 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public nint hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern nint SHGetFileInfo(string pszPath, uint dwFileAttributes,
            out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(nint hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;

        private Icon? GetDriveIcon(string driveRoot)
        {
            if (string.IsNullOrEmpty(driveRoot)) return null;
            if (_iconCache.TryGetValue(driveRoot, out var cached)) return cached;

            SHFILEINFO shinfo;
            SHGetFileInfo(driveRoot, 0, out shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_ICON | SHGFI_LARGEICON);
            if (shinfo.hIcon != nint.Zero)
            {
                try
                {
                    var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                    _iconCache[driveRoot] = icon;
                    return icon;
                }
                finally { DestroyIcon(shinfo.hIcon); }
            }
            _iconCache[driveRoot] = SystemIcons.WinLogo;
            return _iconCache[driveRoot];
        }

        // —— 圆角路径：仅顶部圆角（用于标题栏绘制）—— 
        private static GraphicsPath CreateTopRoundedRect(Rectangle rect, int radiusTop)
        {
            var path = new GraphicsPath();
            int r = Math.Max(0, Math.Min(radiusTop, Math.Min(rect.Width / 2, rect.Height)));
            int d = r * 2;

            if (r == 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            // 左上角
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            // 上边
            path.AddLine(rect.X + r, rect.Y, rect.Right - r, rect.Y);
            // 右上角
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            // 右边到下边直角
            path.AddLine(rect.Right, rect.Y + r, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + r);
            path.CloseFigure();
            return path;
        }

        /// <summary>四角可各自设置半径的圆角路径（用于控件 Region）。</summary>
        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radiusTL, int radiusTR, int radiusBR, int radiusBL)
        {
            var path = new GraphicsPath();

            int rTL = Math.Max(0, Math.Min(radiusTL, Math.Min(rect.Width / 2, rect.Height / 2)));
            int rTR = Math.Max(0, Math.Min(radiusTR, Math.Min(rect.Width / 2, rect.Height / 2)));
            int rBR = Math.Max(0, Math.Min(radiusBR, Math.Min(rect.Width / 2, rect.Height / 2)));
            int rBL = Math.Max(0, Math.Min(radiusBL, Math.Min(rect.Width / 2, rect.Height / 2)));

            // 起点：左上角弧
            if (rTL > 0) path.AddArc(rect.X, rect.Y, rTL * 2, rTL * 2, 180, 90);
            else path.AddLine(rect.X, rect.Y, rect.X, rect.Y);

            // 上边
            path.AddLine(rect.X + rTL, rect.Y, rect.Right - rTR, rect.Y);

            // 右上角弧
            if (rTR > 0) path.AddArc(rect.Right - rTR * 2, rect.Y, rTR * 2, rTR * 2, 270, 90);
            else path.AddLine(rect.Right, rect.Y, rect.Right, rect.Y);

            // 右边
            path.AddLine(rect.Right, rect.Y + rTR, rect.Right, rect.Bottom - rBR);

            // 右下角弧
            if (rBR > 0) path.AddArc(rect.Right - rBR * 2, rect.Bottom - rBR * 2, rBR * 2, rBR * 2, 0, 90);
            else path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            // 下边
            path.AddLine(rect.Right - rBR, rect.Bottom, rect.X + rBL, rect.Bottom);

            // 左下角弧
            if (rBL > 0) path.AddArc(rect.X, rect.Bottom - rBL * 2, rBL * 2, rBL * 2, 90, 90);
            else path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom);

            // 左边
            path.AddLine(rect.X, rect.Bottom - rBL, rect.X, rect.Y + rTL);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kv in _iconCache) kv.Value?.Dispose();
                _iconCache.Clear();
                if (_animTimer != null) { _animTimer.Stop(); _animTimer.Dispose(); _animTimer = null; }
            }
            base.Dispose(disposing);
        }
    }
}
