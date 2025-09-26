using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MpvNet.Windows
{
    /// <summary>
    /// 透明背景自绘磁盘列表：上部圆角标题栏 + 两排文字 + 进度条 + 系统图标
    /// 不显示滚动条，按住鼠标拖动。标题栏固定并遮挡上滚内容。
    /// 暴露滚动/选中/悬浮同步接口与事件。
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
            set { _headerHeight = Math.Max(20, value); Invalidate(); ClampScroll(); }
        }
        public Color HeaderBackColor { get; set; } = Color.FromArgb(64, 80, 140, 255); // 半透明蓝
        public Padding HeaderPadding { get; set; } = new Padding(12, 6, 12, 6);
        public int HeaderCornerRadius { get; set; } = 10;

        // —— 行高/行距 —— 
        private int _rowHeight = 68;
        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = Math.Max(24, value); RecalcContent(); Invalidate(); }
        }
        private int _rowSpacing = 20;
        public int RowSpacing
        {
            get => _rowSpacing;
            set { _rowSpacing = Math.Max(0, value); RecalcContent(); Invalidate(); }
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

        // —— 真透明 —— 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent == null || _paintingBack) return;
            try
            {
                _paintingBack = true;
                Rectangle rectInParent = new Rectangle(Left, Top, Width, Height);
                var g = e.Graphics;
                var state = g.Save();
                g.TranslateTransform(-rectInParent.X, -rectInParent.Y);
                using var pe = new PaintEventArgs(g, rectInParent);
                this.InvokePaintBackground(Parent, pe);
                this.InvokePaint(Parent, pe);
                g.Restore(state);
            }
            finally { _paintingBack = false; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int viewportTop = ShowHeader ? HeaderHeight : 0;

            // 标题栏（上部圆角）
            if (ShowHeader)
            {
                var headerRect = new Rectangle(0, 0, ClientSize.Width, HeaderHeight);
                using (var path = CreateTopRoundedRect(headerRect, HeaderCornerRadius))
                using (var b = new SolidBrush(HeaderBackColor))
                    g.FillPath(b, path);

                using var fTitle = MakeFont("Segoe UI Semibold", 10f, FontStyle.Regular, "Segoe UI", FontStyle.Bold);
                using var brTitle = new SolidBrush(Color.White);
                var textRect = new Rectangle(HeaderPadding.Left, HeaderPadding.Top,
                                             ClientSize.Width - HeaderPadding.Horizontal,
                                             HeaderHeight - HeaderPadding.Vertical);
                var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(HeaderText, fTitle, brTitle, textRect, sf);
            }

            // 仅在标题栏以下绘制列表（被遮挡效果）
            var clipRect = new Rectangle(0, viewportTop, ClientSize.Width, Math.Max(0, ClientSize.Height - viewportTop));
            Region oldClip = g.Clip;
            g.SetClip(clipRect, CombineMode.Replace);

            int y = viewportTop + _scrollY;

            using var f1 = MakeFont("Segoe UI Semibold", 12f, FontStyle.Regular, "Segoe UI", FontStyle.Bold);
            using var f2 = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            using var br1 = new SolidBrush(Color.White);
            using var br2 = new SolidBrush(Color.FromArgb(215, 220, 230));
            using var barBg = new SolidBrush(Color.FromArgb(80, 110, 120, 140));
            using var barFg = new SolidBrush(Color.FromArgb(200, 120, 180, 255));
            using var hover = new SolidBrush(Color.FromArgb(22, 120, 170, 255));
            using var sel = new SolidBrush(Color.FromArgb(40, 120, 170, 255));

            int contentWidth = ClientSize.Width - 1;

            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                var rowRect = new Rectangle(0, y, contentWidth, RowHeight);

                if (rowRect.Bottom >= viewportTop && rowRect.Top <= ClientSize.Height)
                {
                    Rectangle overlay = rowRect;
                    overlay.Y = Math.Max(viewportTop, overlay.Y - 2);
                    overlay.Height = Math.Min(ClientSize.Height - overlay.Y, overlay.Height + 4);
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

            g.Clip = oldClip;
        }

        // —— 输入/滚动 —— 
        private void DiskList_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _dragging = true;
            _dragStartY = e.Y;
            _dragStartScrollY = _scrollY;
            Capture = true;

            if (ShowHeader && e.Y < HeaderHeight) return;

            int idx = HitTest(e.Location);
            if (idx >= 0 && idx < _items.Count)
            {
                SetSelectedIndex(idx, ensureVisible: false, raiseEvent: true);
                DiskSelected?.Invoke(this, _items[idx].Root);
            }
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
            if (idx >= 0 && idx < _items.Count && (y - topOfRow) < RowHeight) return idx;
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
        }

        private void RecalcContent()
        {
            _contentHeight = _items.Count == 0 ? 0 : _items.Count * (RowHeight + RowSpacing) - RowSpacing;
            ClampScroll();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ClampScroll();
            Invalidate();
        }

        private void ClampScroll()
        {
            int viewportTop = ShowHeader ? HeaderHeight : 0;
            int viewportHeight = Math.Max(0, ClientSize.Height - viewportTop);
            int min = Math.Min(0, viewportHeight - _contentHeight);
            _scrollY = (_contentHeight <= viewportHeight) ? 0 : Math.Max(min, Math.Min(0, _scrollY));
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
            it.Usage01 = (total > 0) ? (float)used / total : 0f;
            it.Line2 = (total > 0) ? $"{Fmt(free)} 可用, 共 {Fmt(total)}" : "不可用";
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
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;

        private Icon? GetDriveIcon(string driveRoot)
        {
            if (string.IsNullOrEmpty(driveRoot)) return null;
            if (_iconCache.TryGetValue(driveRoot, out var cached)) return cached;

            SHFILEINFO shinfo;
            SHGetFileInfo(driveRoot, 0, out shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_ICON | SHGFI_LARGEICON);
            if (shinfo.hIcon != IntPtr.Zero)
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

        // —— 上部圆角路径 —— 
        private static GraphicsPath CreateTopRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); path.CloseFigure(); return path; }
            int r = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height));
            int d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddLine(rect.X + r, rect.Y, rect.Right - r, rect.Y);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddLine(rect.Right, rect.Y + r, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + r);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kv in _iconCache) kv.Value?.Dispose();
                _iconCache.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
