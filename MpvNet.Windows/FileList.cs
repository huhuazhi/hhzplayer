using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // ⭐关键修改：全局（静态）图标缓存，按扩展名缓存，所有 FileList 共用，避免重复打 Shell
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
        private readonly Font _itemFont   = new("Segoe UI", 10, FontStyle.Regular);

        // 颜色 & 画刷/笔（复用）
        private readonly Color _bgColor       = Color.FromArgb(70, 20, 40, 70);
        private readonly Color _altRowColor   = Color.FromArgb(40, 50, 50, 80);
        private readonly Color _hoverColor    = Color.FromArgb(80, 0, 120, 215);
        private readonly Color _selectedColor = Color.FromArgb(120, 0, 120, 215);
        private readonly Color _headerBg      = Color.FromArgb(100, 0, 122, 204);

        private readonly SolidBrush _bgBrush;
        private readonly SolidBrush _altRowBrush;
        private readonly SolidBrush _hoverBrush;
        private readonly SolidBrush _selectedBrush;
        private readonly SolidBrush _headerBrush;
        private readonly Pen _borderPen;

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

            _bgBrush       = new SolidBrush(_bgColor);
            _altRowBrush   = new SolidBrush(_altRowColor);
            _hoverBrush    = new SolidBrush(_hoverColor);
            _selectedBrush = new SolidBrush(_selectedColor);
            _headerBrush   = new SolidBrush(_headerBg);
            _borderPen     = new Pen(Color.FromArgb(150, 0, 120, 215), 2);

            // ⭐关键修改：初始化全局缓存的“文件夹”与“默认文件”图标（仅一次）
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
            set { _iconSize = Math.Max(16, Math.Min(64, value)); if (_rowHeight < _iconSize + 8) _rowHeight = _iconSize + 8; Invalidate(); }
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

        // ===================== 导航 & 预取缓存 =====================
        public void NavigateTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            // ⭐关键修改：在装载条目之前，先“预热”本目录用到的所有扩展名图标
            PrewarmIconsForDirectory(path);

            LoadFilesCore(path);
            CurrentPath = path;
            _scrollOffsetY = 0;
            DirectoryChanged?.Invoke(this, CurrentPath);
            Invalidate();
        }

        public void LoadFiles(string path) => NavigateTo(path);

        private void PrewarmIconsForDirectory(string path)
        {
            // 收集所有会用到的扩展名（文件夹统一用一个 icon，不需要循环拿）
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var f in Directory.EnumerateFiles(path))
                {
                    string ext = Path.GetExtension(f);
                    if (string.IsNullOrEmpty(ext)) ext = "."; // 统一当作“无扩展名”
                    set.Add(ext);
                }
            }
            catch { /* 忽略访问异常等 */ }

            // ⭐关键修改：一次性确保缓存里有这些扩展名的图标
            foreach (var ext in set)
            {
                EnsureIconCached(ext);
            }
        }

        // ===================== 数据构建 =====================
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
                    // ⭐关键修改：直接引用全局缓存
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
                        Icon = s_folderIcon // ⭐关键修改：统一用一个
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

                    // ⭐关键修改：从全局缓存拿（此时基本已被 Prewarm 缓好），不再调用 Shell
                    var icon = GetIconFromCache(ext) ?? s_defaultFileIcon;

                    _items.Add(new FileItem
                    {
                        Name     = fi.Name,
                        FullPath = file,
                        IsDir    = false,
                        Size     = fi.Length,
                        Type     = string.IsNullOrEmpty(fi.Extension) ? "文件" : fi.Extension.ToUpperInvariant(),
                        Modified = fi.LastWriteTime,
                        Icon     = icon
                    });
                }
            }
            catch { /* 忽略 */ }
        }

        // ===================== 绘制（虚拟化 + TextRenderer） =====================
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            //g.InterpolationMode = InterpolationMode.NearestNeighbor;

            // 背景
            g.FillRectangle(_bgBrush, ClientRectangle);

            int headerH = _rowHeight;

            // 表头
            g.FillRectangle(_headerBrush, new Rectangle(0, 0, Width, headerH));
            int colX = 0;
            for (int i = 0; i < _columns.Length; i++)
            {
                int cw = (int)(Width * _colWidths[i]);
                TextRenderer.DrawText(g, _columns[i], _headerFont,
                    new Rectangle(colX + 8, 0, cw - 16, headerH),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                colX += cw;
            }

            // 可见行裁剪
            int viewportH = Math.Max(0, Height - headerH);
            int first = Math.Max(0, _scrollOffsetY / _rowHeight);
            int last  = Math.Min(_items.Count - 1, (_scrollOffsetY + viewportH) / _rowHeight);

            int colW0 = (int)(Width * _colWidths[0]);
            int colW1 = (int)(Width * _colWidths[1]);
            int colW2 = (int)(Width * _colWidths[2]);
            int colW3 = (int)(Width * _colWidths[3]);

            for (int i = first; i <= last; i++)
            {
                var item = _items[i];
                int drawY = headerH + i * _rowHeight - _scrollOffsetY;
                var rowRect = new Rectangle(0, drawY, Width, _rowHeight);

                if ((i & 1) == 1) g.FillRectangle(_altRowBrush, rowRect);
                if (i == _hoverIndex) g.FillRectangle(_hoverBrush, rowRect);
                //if (i == _selectedIndex) g.FillRectangle(_selectedBrush, rowRect);

                int x0 = 0, x1 = x0 + colW0, x2 = x1 + colW1, x3 = x2 + colW2;

                // 图标 + 名称
                if (item.Icon != null)
                {
                    // ⭐关键修改：缓存里就是 32×32，不需要高质量缩放，直接画
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

        // ===================== 交互 =====================
        private int HitTestIndex(int mouseY)
        {
            int headerH = _rowHeight;
            int contentY = mouseY - headerH + _scrollOffsetY;
            if (contentY < 0) return -1;
            int idx = contentY / _rowHeight;
            return (idx >= 0 && idx < _items.Count) ? idx : -1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollOffset += -e.Delta; // 120/格
        }

// 1) 新增两个字段（在 FileList 成员里）
private Point _mouseDownPos;
private int _mouseDownIndex = -1;
private const int ClickMoveTolerance = 5; // 允许抖动像素
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;              // 如果你有拖拽滚动，这行保留
                _lastMouseY = e.Y;             // 如果你有拖拽滚动，这行保留

                _mouseDownPos = e.Location;
                _mouseDownIndex = HitTestIndex(e.Y);   // 你已有的命中测试函数
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
            if (_dragging)
            {
                int dy = e.Y - _lastMouseY;
                _lastMouseY = e.Y;
                ScrollOffset -= dy;            // 你的滚动逻辑
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
                    OpenItem(upIndex);   // 👉 真点击才打开/进入
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

        // ⭐关键修改：全局初始化“文件夹”“默认文件”图标
        private static void EnsureGlobalIcons()
        {
            if (s_folderIcon != null && s_defaultFileIcon != null) return;
            lock (s_iconLock)
            {
                if (s_folderIcon == null)
                    s_folderIcon = GetShellIconBitmap("dummy", FILE_ATTRIBUTE_DIRECTORY);

                if (s_defaultFileIcon == null)
                {
                    // 用 ".txt" 或 "." 都可以拿到一个通用文件类型图标
                    s_defaultFileIcon = GetShellIconBitmap("dummy.", FILE_ATTRIBUTE_NORMAL);
                }
            }
        }

        // ⭐关键修改：获取（或创建）某扩展名的图标
        private static Bitmap GetIconFromCache(string ext)
        {
            lock (s_iconLock)
            {
                return s_iconByExt.TryGetValue(ext, out var bmp) ? bmp : null;
            }
        }

        private static void EnsureIconCached(string ext)
        {
            if (string.IsNullOrEmpty(ext)) ext = ".";
            lock (s_iconLock)
            {
                if (s_iconByExt.ContainsKey(ext)) return;
            }

            // 取一次 shell 图标并放入缓存
            Bitmap bmp = GetShellIconBitmap("dummy" + ext, FILE_ATTRIBUTE_NORMAL) ?? s_defaultFileIcon;
            lock (s_iconLock)
            {
                if (!s_iconByExt.ContainsKey(ext))
                    s_iconByExt[ext] = bmp;
            }
        }

        // ⭐关键修改：真正的 Shell 取图标（32x32）→ Bitmap，已 DestroyIcon，避免泄漏
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
                    // 复制为 Bitmap（32x32），FromHandle 的 Icon 需要自行 Destroy
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]  public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON              = 0x000000100;
        private const uint SHGFI_LARGEICON         = 0x000000000; // 32x32
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL    = 0x00000080;

        // ===================== 数据结构 =====================
        private class FileItem
        {
            public string   Name { get; set; }
            public string   FullPath { get; set; }
            public bool     IsDir { get; set; }
            public long     Size { get; set; }
            public string   Type { get; set; }
            public DateTime Modified { get; set; }
            public Image    Icon { get; set; }
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
