using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpvNet.Windows
{
    public class FileList : Control
    {
        private readonly List<FileItem> _items = new();

        private int _rowHeight = 40;
        private int _headerHeight = 60;
        private readonly string[] _columns = { "名称", "大小", "类型", "修改日期" };
        private float[] _colWidths = { 0.50f, 0.15f, 0.15f, 0.20f }; // 4 列比例

        private int _hoverIndex = -1;
        private int _selectedIndex = -1; // 预留（目前未使用）

        private int _iconSize = 32;

        private static readonly object s_iconLock = new();
        private static readonly Dictionary<string, Bitmap> s_iconByExt = new(StringComparer.OrdinalIgnoreCase);
        private static Bitmap s_folderIcon;
        private static Bitmap s_defaultFileIcon;

        private int _scrollOffsetY = 0;
        private int _maxScroll = 0;
        private bool _dragging = false;
        private int _lastMouseY;

        private readonly Font _headerFont = new("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _itemFont = new("Segoe UI", 10, FontStyle.Regular);
        private readonly Font _pathFont = new("Segoe UI", 10, FontStyle.Regular);

        private readonly Color _bgColor = Color.FromArgb(70, 20, 40, 70);
        private readonly Color _altRowColor = Color.FromArgb(80, 60, 60, 60);
        private readonly Color _hoverColor = Color.FromArgb(80, 0, 120, 215);
        private readonly Color _selectedColor = Color.FromArgb(120, 0, 120, 215);
        private readonly Color _headerBg = Color.FromArgb(100, 0, 122, 204);

        private readonly SolidBrush _bgBrush;
        private readonly SolidBrush _altRowBrush;
        private readonly SolidBrush _hoverBrush;
        private readonly SolidBrush _selectedBrush;
        private readonly SolidBrush _headerBrush;
        private readonly Pen _borderPen;

        private List<(string Segment, string FullPath, Rectangle Bounds)> _pathSegments = new();
        private int _pathBarHeight = 50;

        public event EventHandler<string[]> FileOpened;
        public event EventHandler<string> DirectoryChanged;
        public event EventHandler<HoverChangedEventArgs> HoverChanged;
        public event EventHandler<ViewportOffsetChangedEventArgs> ViewportOffsetChanged;

        public string CurrentPath { get; private set; } = string.Empty;
        public int HotIndex => _hoverIndex;

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

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

        public int RowHeight
        {
            get => _rowHeight;
            set
            {
                _rowHeight = Math.Max(value, _iconSize + 8);
                InvalidateAllTextBitmaps(); // 行高变化 → 失效文本位图
                Invalidate();
            }
        }

        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(20, value); Invalidate(); }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = Math.Max(16, Math.Min(64, value));
                if (_rowHeight < _iconSize + 8) _rowHeight = _iconSize + 8;
                Invalidate(); // 图标大小变只需重绘
            }
        }

        public float[] ColumnWidths
        {
            get => _colWidths;
            set
            {
                if (value != null && value.Length == 4)
                {
                    _colWidths = value;
                    InvalidateAllTextBitmaps(); // 列宽变化 → 失效文本位图
                    Invalidate();
                }
            }
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
                    Invalidate();
                }
            }
        }

        public void NavigateTo(string path)
        {
            path = NormalizeRootPath(path);
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            PrewarmIconsForDirectory(path);

            // 清理旧位图缓存，避免泄漏
            DisposeAllTextBitmaps();
            _items.Clear();

            LoadFilesCore(path);
            CurrentPath = path;
            _scrollOffsetY = 0;
            BuildPathSegments(path);
            DirectoryChanged?.Invoke(this, CurrentPath);
            Invalidate();
        }

        private static string NormalizeRootPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == ':')
                return path + "\\";
            return path;
        }

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
            catch { }

            foreach (var ext in set) EnsureIconCached(ext);
        }

        private sealed class ExplorerNameComparer : IComparer<FileItem>
        {
            public static readonly ExplorerNameComparer Instance = new();

            public int Compare(FileItem? a, FileItem? b)
            {
                if (ReferenceEquals(a, b)) return 0;
                if (a is null) return -1;
                if (b is null) return 1;

                bool aUp = a.IsDir && a.Name == "..";
                bool bUp = b.IsDir && b.Name == "..";
                if (aUp || bUp) return aUp ? (bUp ? 0 : -1) : 1;

                if (a.IsDir != b.IsDir) return a.IsDir ? -1 : 1;

                return StrCmpLogicalW(a.Name, b.Name);
            }
        }

        private void LoadFilesCore(string path)
        {
            var diRoot = new DirectoryInfo(path);

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
                    Icon = s_folderIcon,
                    IconLoaded = true
                });
            }

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
                        Icon = s_folderIcon,
                        IconLoaded = true
                    });
                }
            }
            catch { }

            try
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var fi = new FileInfo(file);
                    string ext = fi.Extension;
                    if (string.IsNullOrEmpty(ext)) ext = ".";
                    _items.Add(new FileItem
                    {
                        Name = fi.Name,
                        FullPath = file,
                        IsDir = false,
                        Size = fi.Length,
                        Type = string.IsNullOrEmpty(fi.Extension) ? "文件" : fi.Extension.ToUpperInvariant(),
                        Modified = fi.LastWriteTime,
                        Icon = s_defaultFileIcon,
                        IconLoaded = false
                    });
                }
            }
            catch { }

            _items.Sort(ExplorerNameComparer.Instance);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.FillRectangle(_bgBrush, ClientRectangle);

            int y = 0;
            DrawPathBar(g, ref y);

            // 表头
            g.FillRectangle(_headerBrush, new Rectangle(0, y, Width, _headerHeight));
            int colX = 0;
            for (int i = 0; i < _columns.Length; i++)
            {
                int cw = (int)(Width * _colWidths[i]);
                TextRenderer.DrawText(g, _columns[i], _headerFont,
                    new Rectangle(colX + 8, y, cw - 16, _headerHeight),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                colX += cw;
            }
            y += _headerHeight;

            int contentTop = y;
            int viewportH = Math.Max(0, Height - contentTop);
            Rectangle contentRect = new Rectangle(0, contentTop, Width, viewportH);

            var gs = g.Save();
            g.SetClip(contentRect);

            int first = Math.Max(0, _scrollOffsetY / _rowHeight);
            int last = Math.Min(_items.Count - 1, (_scrollOffsetY + viewportH) / _rowHeight);

            int colW0 = (int)(Width * _colWidths[0]);
            int colW1 = (int)(Width * _colWidths[1]);
            int colW2 = (int)(Width * _colWidths[2]);
            int colW3 = (int)(Width * _colWidths[3]);

            for (int i = first; i <= last; i++)
            {
                var item = _items[i];
                int rowTop = contentTop + i * _rowHeight - _scrollOffsetY;
                Rectangle rowRect = new Rectangle(0, rowTop, Width, _rowHeight);
                Rectangle visRow = Rectangle.Intersect(rowRect, contentRect);
                if (visRow.Height <= 0) continue;

                if ((i & 1) == 1) g.FillRectangle(_altRowBrush, visRow);
                if (i == _hoverIndex) g.FillRectangle(_hoverBrush, visRow);
                if (i == _selectedIndex) g.FillRectangle(_selectedBrush, visRow);

                int x0 = 0, x1 = x0 + colW0, x2 = x1 + colW1, x3 = x2 + colW2;

                // 延迟加载“文件”图标（文件夹已用 s_folderIcon）
                if (!item.IconLoaded && !item.IsDir)
                {
                    item.IconLoaded = true;
                    Task.Run(() =>
                    {
                        var icon = GetShellIconBitmap(item.FullPath, FILE_ATTRIBUTE_NORMAL);
                        if (icon != null)
                        {
                            item.Icon = icon;
                            try { BeginInvoke(new Action(Invalidate)); } catch { }
                        }
                    });
                }

                // 图标
                if (item.Icon != null)
                {
                    int iconX = x0 + 8;
                    int iconY = rowTop + (_rowHeight - _iconSize) / 2;
                    g.DrawImage(item.Icon, new Rectangle(iconX, iconY, _iconSize, _iconSize));
                }

                // —— 名称列（缓存位图）——
                int nameLeft = x0 + 8 + _iconSize + 8;
                var nameRect = new Rectangle(nameLeft, visRow.Top, colW0 - (nameLeft - x0) - 8, visRow.Height);
                EnsureNameBmp(item, nameRect.Width, nameRect.Height);
                if (item.NameBmp != null) g.DrawImageUnscaled(item.NameBmp, nameRect.Left, nameRect.Top);

                // —— 大小列（缓存位图）——
                string sizeText = item.Size < 0 ? "" : FormatSize(item.Size);
                var sizeRect = new Rectangle(x1 + 6, visRow.Top, colW1 - 12, visRow.Height);
                EnsureSizeBmp(item, sizeText, sizeRect.Width, sizeRect.Height);
                if (item.SizeBmp != null) g.DrawImageUnscaled(item.SizeBmp, sizeRect.Left, sizeRect.Top);

                // —— 类型列（缓存位图）——
                var typeRect = new Rectangle(x2 + 6, visRow.Top, colW2 - 12, visRow.Height);
                EnsureTypeBmp(item, item.Type, typeRect.Width, typeRect.Height);
                if (item.TypeBmp != null) g.DrawImageUnscaled(item.TypeBmp, typeRect.Left, typeRect.Top);

                // —— 日期列（缓存位图）——
                string dateText = item.Modified == DateTime.MinValue ? "" : item.Modified.ToString("yyyy-MM-dd HH:mm");
                var dateRect = new Rectangle(x3 + 6, visRow.Top, colW3 - 12, visRow.Height);
                EnsureDateBmp(item, dateText, dateRect.Width, dateRect.Height);
                if (item.DateBmp != null) g.DrawImageUnscaled(item.DateBmp, dateRect.Left, dateRect.Top);
            }

            g.Restore(gs);
            int itemsHeight = _items.Count * _rowHeight;
            _maxScroll = Math.Max(0, itemsHeight - viewportH);
            g.DrawRectangle(_borderPen, 1, 1, Width - 2, Height - 2);
        }

        // —— 文本位图缓存：名称 / 大小 / 类型 / 日期 —— 

        private static readonly TextFormatFlags TextFlags =
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPadding;

        private void EnsureNameBmp(FileItem item, int width, int height)
            => EnsureTextBmp(ref item.NameBmp, ref item.NameBmpW, ref item.NameBmpH, item.Name, width, height);

        private void EnsureSizeBmp(FileItem item, string text, int width, int height)
            => EnsureTextBmp(ref item.SizeBmp, ref item.SizeBmpW, ref item.SizeBmpH, text, width, height);

        private void EnsureTypeBmp(FileItem item, string text, int width, int height)
            => EnsureTextBmp(ref item.TypeBmp, ref item.TypeBmpW, ref item.TypeBmpH, text, width, height);

        private void EnsureDateBmp(FileItem item, string text, int width, int height)
            => EnsureTextBmp(ref item.DateBmp, ref item.DateBmpW, ref item.DateBmpH, text, width, height);

        private void EnsureTextBmp(ref Bitmap bmp, ref int bw, ref int bh, string text, int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (bmp != null && bw == width && bh == height)
                return;

            bmp?.Dispose();
            var nb = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(nb))
            {
                g.Clear(Color.Transparent);
                TextRenderer.DrawText(g, text ?? "", _itemFont, new Rectangle(0, 0, width, height), Color.White, TextFlags);
            }
            bmp = nb;
            bw = width;
            bh = height;
        }

        private void InvalidateAllTextBitmaps()
        {
            foreach (var it in _items)
                it.InvalidateTextBitmaps();
        }

        private void DisposeAllTextBitmaps()
        {
            foreach (var it in _items)
                it.DisposeTextBitmaps();
        }

        // —— 面包屑 —— 
        private void DrawPathBar(Graphics g, ref int y)
        {
            _pathSegments.Clear();
            if (string.IsNullOrEmpty(CurrentPath))
            {
                y += _pathBarHeight;
                return;
            }

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
                    string root = @"\\" + parts[0] + (parts.Length >= 2 ? "\\" + parts[1] : "");
                    if (i <= 1)
                        fullPath = root;
                    else
                        fullPath = root + "\\" + string.Join("\\", parts, 2, i - 1);
                }
                else if (hasDrive)
                {
                    string root = parts[0] + "\\";
                    if (i == 0)
                        fullPath = root;
                    else
                        fullPath = root + string.Join("\\", parts, 1, i);
                }
                else
                {
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

        private int HitTestIndex(int mouseY)
        {
            int headerTop = _pathBarHeight + _headerHeight;
            int contentY = mouseY - headerTop + _scrollOffsetY;
            if (contentY < 0) return -1;
            int idx = contentY / _rowHeight;
            return (idx >= 0 && idx < _items.Count) ? idx : -1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollOffset += -e.Delta;
        }

        private Point _mouseDownPos;
        private int _mouseDownIndex = -1;
        private const int ClickMoveTolerance = 5;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // 面包屑点击
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
                FileOpened?.Invoke(this, new[] { item.FullPath });
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 面包屑 hover
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
            if (_hoverIndex == index) return;
            int old = _hoverIndex;
            _hoverIndex = index;

            if (raiseEvent) HoverChanged?.Invoke(this, new HoverChangedEventArgs(index));

            if (old >= 0) Invalidate(GetRowRect(old));
            if (_hoverIndex >= 0) Invalidate(GetRowRect(_hoverIndex));
        }

        private Rectangle GetRowRect(int index)
        {
            int y = _pathBarHeight + _headerHeight + index * _rowHeight - _scrollOffsetY;
            return new Rectangle(0, y, Width, _rowHeight);
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

            Bitmap bmp = GetShellIconBitmap("dummy" + ext, FILE_ATTRIBUTE_NORMAL) ?? s_defaultFileIcon;
            lock (s_iconLock)
            {
                if (!s_iconByExt.ContainsKey(ext))
                    s_iconByExt[ext] = bmp;
            }
        }

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
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        private class FileItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsDir { get; set; }
            public long Size { get; set; }
            public string Type { get; set; }
            public DateTime Modified { get; set; }
            public Image Icon { get; set; }
            public bool IconLoaded { get; set; }

            // 文本位图缓存
            public Bitmap NameBmp; public int NameBmpW; public int NameBmpH;
            public Bitmap SizeBmp; public int SizeBmpW; public int SizeBmpH;
            public Bitmap TypeBmp; public int TypeBmpW; public int TypeBmpH;
            public Bitmap DateBmp; public int DateBmpW; public int DateBmpH;

            public void InvalidateTextBitmaps()
            {
                NameBmp?.Dispose(); NameBmp = null; NameBmpW = NameBmpH = 0;
                SizeBmp?.Dispose(); SizeBmp = null; SizeBmpW = SizeBmpH = 0;
                TypeBmp?.Dispose(); TypeBmp = null; TypeBmpW = TypeBmpH = 0;
                DateBmp?.Dispose(); DateBmp = null; DateBmpW = DateBmpH = 0;
            }

            public void DisposeTextBitmaps() => InvalidateTextBitmaps();
        }

        private void BuildPathSegments(string path) => _pathSegments.Clear();

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            InvalidateAllTextBitmaps(); // 宽高变化需要重建文本位图
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeAllTextBitmaps();
            }
            base.Dispose(disposing);
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
