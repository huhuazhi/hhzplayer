using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MpvNet.Windows
{
    public class FileList : Control
    {
        // ===================== 基本数据 =====================
        private readonly List<FileItem> _items = new();

        private int _rowHeight = 40;
        private readonly string[] _columns = { "名称", "大小", "类型", "修改日期" };
        private float[] _colWidths = { 0.50f, 0.15f, 0.15f, 0.20f };

        private int _hoverIndex = -1;
        private int _selectedIndex = -1;

        // 图标尺寸
        private int _iconSize = 32;

        // ⭐ 图标缓存（全局）
        private static readonly object s_iconLock = new();
        private static readonly Dictionary<string, Bitmap> s_iconByExt = new(StringComparer.OrdinalIgnoreCase);
        private static Bitmap s_folderIcon;
        private static Bitmap s_defaultFileIcon;

        // 滚动
        private int _scrollOffsetY = 0;
        private int _maxScroll = 0;
        private bool _dragging = false;
        private int _lastMouseY;

        // 字体
        private readonly Font _headerFont = new("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _itemFont = new("Segoe UI", 10, FontStyle.Regular);
        private readonly Font _pathFont = new("Segoe UI", 10, FontStyle.Regular);

        // 颜色
        private readonly Color _bgColor = Color.FromArgb(70, 20, 40, 70);
        private readonly Color _altRowColor = Color.FromArgb(40, 50, 50, 80);
        private readonly Color _hoverColor = Color.FromArgb(80, 0, 120, 215);
        private readonly Color _selectedColor = Color.FromArgb(120, 0, 120, 215);
        private readonly Color _headerBg = Color.FromArgb(100, 0, 122, 204);

        private readonly SolidBrush _bgBrush;
        private readonly SolidBrush _altRowBrush;
        private readonly SolidBrush _hoverBrush;
        private readonly SolidBrush _selectedBrush;
        private readonly SolidBrush _headerBrush;
        private readonly Pen _borderPen;

        // ⭐ 路径栏（面包屑）
        private List<(string Segment, string FullPath, Rectangle Bounds)> _pathSegments = new();
        private int _pathBarHeight = 50;

        // 事件
        public event EventHandler<string> FileOpened;
        public event EventHandler<string> DirectoryChanged;
        public event EventHandler<HoverChangedEventArgs> HoverChanged;
        public event EventHandler<ViewportOffsetChangedEventArgs> ViewportOffsetChanged;

        public string CurrentPath { get; private set; } = string.Empty;

        public FileList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            UpdateStyles();

            _bgBrush = new SolidBrush(_bgColor);
            _altRowBrush = new SolidBrush(_altRowColor);
            _hoverBrush = new SolidBrush(_hoverColor);
            _selectedBrush = new SolidBrush(_selectedColor);
            _headerBrush = new SolidBrush(_headerBg);
            _borderPen = new Pen(Color.FromArgb(150, 0, 120, 215), 2);

            EnsureGlobalIcons();

            TabStop = true;
            MouseEnter += (_, __) => Focus();
        }

        // ===================== 属性 =====================
        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = Math.Max(value, _iconSize + 8); Invalidate(); }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = Math.Max(16, Math.Min(64, value));
                if (_rowHeight < _iconSize + 8) _rowHeight = _iconSize + 8;
                Invalidate();
            }
        }

        public float[] ColumnWidths
        {
            get => _colWidths;
            set { if (value != null && value.Length == 4) { _colWidths = value; Invalidate(); } }
        }

        public int ScrollOffset
        {
            get => _scrollOffsetY;
            set
            {
                int clamped = Math.Max(0, Math.Min(_maxScroll, value));
                if (clamped != _scrollOffsetY)
                {
                    _scrollOffsetY = clamped;
                    ViewportOffsetChanged?.Invoke(this, new ViewportOffsetChangedEventArgs(_scrollOffsetY));
                }
                Invalidate();
            }
        }

        // ===================== 导航 =====================
        public void NavigateTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            PrewarmIconsForDirectory(path);
            LoadFilesCore(path);
            CurrentPath = path;
            _scrollOffsetY = 0;
            BuildPathSegments(path);
            DirectoryChanged?.Invoke(this, CurrentPath);
            Invalidate();
        }

        public void LoadFiles(string path) => NavigateTo(path);

        private void PrewarmIconsForDirectory(string path)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var f in Directory.EnumerateFiles(path))
                {
                    string ext = Path.GetExtension(f);
                    if (string.IsNullOrEmpty(ext)) ext = ".";
                    set.Add(ext);
                }
            }
            catch { /* 忽略 */ }

            foreach (var ext in set) EnsureIconCached(ext);
        }

        private void LoadFilesCore(string path)
        {
            _items.Clear();

            var diRoot = new DirectoryInfo(path);

            // “..” 返回上一级
            if (diRoot.Parent != null)
            {
                _items.Add(new FileItem
                {
                    Name = "..",
                    FullPath = diRoot.Parent.FullName,
                    IsDir = true,
                    Size = -1,
                    Type = "上一级目录",
                    Modified = DateTime.Now,
                    Icon = s_folderIcon
                });
            }

            // 目录
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var di = new DirectoryInfo(dir);
                    _items.Add(new FileItem
                    {
                        Name = di.Name,
                        FullPath = dir,
                        IsDir = true,
                        Size = -1,
                        Type = "文件夹",
                        Modified = di.LastWriteTime,
                        Icon = s_folderIcon
                    });
                }
            }
            catch { /* 忽略 */ }

            // 文件
            try
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var fi = new FileInfo(file);
                    string ext = fi.Extension;
                    if (string.IsNullOrEmpty(ext)) ext = ".";
                    var icon = GetIconFromCache(ext) ?? s_defaultFileIcon;

                    _items.Add(new FileItem
                    {
                        Name = fi.Name,
                        FullPath = file,
                        IsDir = false,
                        Size = fi.Length,
                        Type = string.IsNullOrEmpty(fi.Extension) ? "文件" : fi.Extension.ToUpperInvariant(),
                        Modified = fi.LastWriteTime,
                        Icon = icon
                    });
                }
            }
            catch { /* 忽略 */ }
        }

        // ===================== 绘制 =====================
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // 背景
            g.FillRectangle(_bgBrush, ClientRectangle);

            int y = 0;

            // ⭐ 路径栏（面包屑）
            DrawPathBar(g, ref y);

            // 表头
            int headerH = _rowHeight;
            g.FillRectangle(_headerBrush, new Rectangle(0, y, Width, headerH));
            int colX = 0;
            for (int i = 0; i < _columns.Length; i++)
            {
                int cw = (int)(Width * _colWidths[i]);
                TextRenderer.DrawText(g, _columns[i], _headerFont,
                    new Rectangle(colX + 8, y, cw - 16, headerH),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                colX += cw;
            }
            y += headerH;

            // 可见行裁剪
            int viewportH = Math.Max(0, Height - y);
            int first = Math.Max(0, _scrollOffsetY / _rowHeight);
            int last = Math.Min(_items.Count - 1, (_scrollOffsetY + viewportH) / _rowHeight);

            int colW0 = (int)(Width * _colWidths[0]);
            int colW1 = (int)(Width * _colWidths[1]);
            int colW2 = (int)(Width * _colWidths[2]);
            int colW3 = (int)(Width * _colWidths[3]);

            for (int i = first; i <= last; i++)
            {
                var item = _items[i];
                int drawY = y + i * _rowHeight - _scrollOffsetY;
                var rowRect = new Rectangle(0, drawY, Width, _rowHeight);

                if ((i & 1) == 1) g.FillRectangle(_altRowBrush, rowRect);
                if (i == _hoverIndex) g.FillRectangle(_hoverBrush, rowRect);
                //if (i == _selectedIndex) g.FillRectangle(_selectedBrush, rowRect);

                int x0 = 0, x1 = x0 + colW0, x2 = x1 + colW1, x3 = x2 + colW2;

                // 图标 + 名称
                if (item.Icon != null)
                {
                    int iconX = x0 + 8;
                    int iconY = drawY + (_rowHeight - _iconSize) / 2;
                    g.DrawImage(item.Icon, new Rectangle(iconX, iconY, _iconSize, _iconSize));
                }

                int nameLeft = x0 + 8 + _iconSize + 8;
                TextRenderer.DrawText(g, item.Name, _itemFont,
                    new Rectangle(nameLeft, drawY, colW0 - (nameLeft - x0) - 8, _rowHeight),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // 大小
                string sizeText = item.Size < 0 ? "" : FormatSize(item.Size);
                TextRenderer.DrawText(g, sizeText, _itemFont,
                    new Rectangle(x1 + 6, drawY, colW1 - 12, _rowHeight),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // 类型
                TextRenderer.DrawText(g, item.Type, _itemFont,
                    new Rectangle(x2 + 6, drawY, colW2 - 12, _rowHeight),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // 修改日期
                TextRenderer.DrawText(g, item.Modified.ToString("yyyy-MM-dd HH:mm"), _itemFont,
                    new Rectangle(x3 + 6, drawY, colW3 - 12, _rowHeight),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // 滚动范围
            int itemsHeight = _items.Count * _rowHeight;
            _maxScroll = Math.Max(0, itemsHeight - viewportH);

            // 外框
            g.DrawRectangle(_borderPen, 1, 1, Width - 2, Height - 2);
        }

        private void DrawPathBar(Graphics g, ref int y)
        {
            _pathSegments.Clear();
            if (string.IsNullOrEmpty(CurrentPath))
            {
                y += _pathBarHeight;
                return;
            }

            // 分割路径并绘制：分隔符显示为 ">"
            var parts = CurrentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            bool isUnc = CurrentPath.StartsWith(@"\\");
            bool hasDrive = parts.Length > 0 && parts[0].EndsWith(":");

            int x = 8;
            for (int i = 0; i < parts.Length; i++)
            {
                string segLabel = parts[i];
                string fullPath;

                if (isUnc)
                {
                    // UNC: \\server\share\dir\subdir
                    // 将点击“server”与“share”都指向 \\server\share 根，之后逐级追加
                    string root = @"\\" + parts[0] + (parts.Length >= 2 ? "\\" + parts[1] : "");
                    if (i <= 1)
                        fullPath = root;
                    else
                        fullPath = root + "\\" + string.Join("\\", parts, 2, i - 1 + 1);
                }
                else if (hasDrive)
                {
                    // 本地盘: C:\Users\Name\Videos
                    string root = parts[0] + "\\";
                    if (i == 0)
                        fullPath = root;
                    else
                        fullPath = root + string.Join("\\", parts, 1, i);
                }
                else
                {
                    // 其它情况（相对路径等）
                    fullPath = string.Join("\\", parts, 0, i + 1);
                }

                Size textSize = TextRenderer.MeasureText(segLabel, _pathFont);
                var rect = new Rectangle(x, y + 5, textSize.Width, _pathBarHeight - 10);

                TextRenderer.DrawText(g, segLabel, _pathFont, rect, Color.LightBlue,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                _pathSegments.Add((segLabel, fullPath, rect));

                x += rect.Width + 10;
                if (i < parts.Length - 1)
                {
                    TextRenderer.DrawText(g, ">", _itemFont,
                        new Rectangle(x, y, 20, _pathBarHeight),
                        Color.White,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                    x += 40;
                }
            }

            y += _pathBarHeight;
        }

        // ===================== 交互 =====================
        private int HitTestIndex(int mouseY)
        {
            // 内容区域从路径栏 + 表头之后开始
            int headerTop = _pathBarHeight + _rowHeight;
            int contentY = mouseY - headerTop + _scrollOffsetY;
            if (contentY < 0) return -1;
            int idx = contentY / _rowHeight;
            return (idx >= 0 && idx < _items.Count) ? idx : -1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollOffset += -e.Delta; // 120/格
        }

        private Point _mouseDownPos;
        private int _mouseDownIndex = -1;
        private const int ClickMoveTolerance = 5; // 允许抖动像素

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // 先命中路径栏
            foreach (var seg in _pathSegments)
            {
                if (seg.Bounds.Contains(e.Location))
                {
                    NavigateTo(seg.FullPath);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _lastMouseY = e.Y;

                _mouseDownPos = e.Location;
                _mouseDownIndex = HitTestIndex(e.Y);
                Cursor = Cursors.Hand;
            }
        }

        private void OpenItem(int index)
        {
            if (index < 0 || index >= _items.Count) return;

            var item = _items[index];
            if (item.IsDir)
            {
                NavigateTo(item.FullPath);
                DirectoryChanged?.Invoke(this, item.FullPath);
            }
            else
            {
                FileOpened?.Invoke(this, item.FullPath);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 路径栏手型提示
            bool overBreadcrumb = false;
            foreach (var seg in _pathSegments)
            {
                if (seg.Bounds.Contains(e.Location))
                {
                    overBreadcrumb = true;
                    break;
                }
            }
            if (overBreadcrumb)
            {
                Cursor = Cursors.Hand;
                return;
            }
            else if (!_dragging)
            {
                Cursor = Cursors.Default;
            }

            if (_dragging)
            {
                int dy = e.Y - _lastMouseY;
                _lastMouseY = e.Y;
                ScrollOffset -= dy;
            }
            else
            {
                int idx = HitTestIndex(e.Y);
                if (idx != _hoverIndex) SetHotIndex(idx);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;
                Cursor = Cursors.Default;

                int upIndex = HitTestIndex(e.Y);
                bool isClick =
                    _mouseDownIndex >= 0 &&
                    upIndex == _mouseDownIndex &&
                    Math.Abs(e.X - _mouseDownPos.X) <= ClickMoveTolerance &&
                    Math.Abs(e.Y - _mouseDownPos.Y) <= ClickMoveTolerance;

                if (isClick)
                    OpenItem(upIndex);
            }

            _mouseDownIndex = -1;
        }

        public void SetHotIndex(int index, bool raiseEvent = true)
        {
            _hoverIndex = index;
            if (raiseEvent) HoverChanged?.Invoke(this, new HoverChangedEventArgs(index));
            Invalidate();
        }

        // ===================== 工具 =====================
        private static string FormatSize(long size)
        {
            const double KB = 1024.0, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            if (size >= TB) return $"{size / TB:0.##} TB";
            if (size >= GB) return $"{size / GB:0.##} GB";
            if (size >= MB) return $"{size / MB:0.##} MB";
            if (size >= KB) return $"{size / KB:0.##} KB";
            return $"{size} B";
        }

        // ⭐ 全局初始化“文件夹”“默认文件”图标
        private static void EnsureGlobalIcons()
        {
            if (s_folderIcon != null && s_defaultFileIcon != null) return;
            lock (s_iconLock)
            {
                if (s_folderIcon == null)
                    s_folderIcon = GetShellIconBitmap("dummy", FILE_ATTRIBUTE_DIRECTORY);

                if (s_defaultFileIcon == null)
                    s_defaultFileIcon = GetShellIconBitmap("dummy.", FILE_ATTRIBUTE_NORMAL);
            }
        }

        // 从缓存取扩展名图标
        private static Bitmap GetIconFromCache(string ext)
        {
            lock (s_iconLock)
            {
                return s_iconByExt.TryGetValue(ext, out var bmp) ? bmp : null;
            }
        }

        // 确保扩展名图标在缓存
        private static void EnsureIconCached(string ext)
        {
            if (string.IsNullOrEmpty(ext)) ext = ".";
            lock (s_iconLock)
            {
                if (s_iconByExt.ContainsKey(ext)) return;
            }

            Bitmap bmp = GetShellIconBitmap("dummy" + ext, FILE_ATTRIBUTE_NORMAL) ?? s_defaultFileIcon;
            lock (s_iconLock)
            {
                if (!s_iconByExt.ContainsKey(ext))
                    s_iconByExt[ext] = bmp;
            }
        }

        // 取系统图标（32x32）→ Bitmap
        private static Bitmap GetShellIconBitmap(string pathOrExt, uint attrs)
        {
            SHFILEINFO sh = new();
            uint flags = SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES;
            IntPtr h = SHGetFileInfo(pathOrExt, attrs, ref sh, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
            if (sh.hIcon != IntPtr.Zero)
            {
                try
                {
                    using var ico = Icon.FromHandle(sh.hIcon);
                    return new Bitmap(ico.ToBitmap());
                }
                finally
                {
                    DestroyIcon(sh.hIcon);
                }
            }
            return null;
        }

        // ===================== Win32 =====================
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000; // 32x32
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        // ===================== 数据结构 =====================
        private class FileItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsDir { get; set; }
            public long Size { get; set; }
            public string Type { get; set; }
            public DateTime Modified { get; set; }
            public Image Icon { get; set; }
        }

        // ===================== 面包屑辅助 =====================
        private void BuildPathSegments(string path)
        {
            _pathSegments.Clear();
            // 实际的绘制在 DrawPathBar 中完成，这里只清空以保证状态一致
        }
    }

    public class HoverChangedEventArgs : EventArgs
    {
        public int Index { get; }
        public HoverChangedEventArgs(int index) => Index = index;
    }

    public class ViewportOffsetChangedEventArgs : EventArgs
    {
        public int OffsetY { get; }
        public ViewportOffsetChangedEventArgs(int offsetY) => OffsetY = offsetY;
    }
}
