using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HHZPlayer.Windows.HHZ
{
    public class FileList : Control
    {
        private readonly List<FileItem> _items = new();

        private int _rowHeight = 40;
        private int _headerHeight = 60;
        private readonly string[] _columns = { "名称", "大小", "类型", "修改日期" };
        private float[] _colWidths = { 0.65f, 0.10f, 0.10f, 0.15f };

        private int _hoverIndex = -1;
        private int _selectedIndex = -1;

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

        // —— ToolTip 延迟显示 ——
        private ToolTip _toolTip;
        private Timer _tipTimer;
        private int _tipIndex = -1;
        private Rectangle _tipNameRect = Rectangle.Empty;
        private bool _tipVisible = false;
        private Point _tipMousePoint;
        private int TipDelayTime = 300; //Tip延迟1000ms

        #region Shell 右键菜单支持

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHParseDisplayName(
            string name, IntPtr bindingContext,
            out IntPtr pidl, uint sfgaoIn, out uint psfgaoOut);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHBindToParent(
            IntPtr pidl, ref Guid riid, out IShellFolder ppv, out IntPtr ppidlLast);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E6-0000-0000-C000-000000000046")]
        private interface IShellFolder
        {
            [PreserveSig]
            int ParseDisplayName(IntPtr hwnd, IntPtr pbc,
                [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
                ref uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
            [PreserveSig] int EnumObjects(IntPtr hwnd, int grfFlags, out IntPtr ppenumIDList);
            [PreserveSig] int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
            [PreserveSig] int CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int GetAttributesOf(int cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref uint rgfInOut);
            [PreserveSig]
            int GetUIObjectOf(IntPtr hwndOwner, int cidl,
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
            [PreserveSig] int GetDisplayNameOf(IntPtr pidl, uint uFlags, IntPtr pName);
            [PreserveSig] int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E4-0000-0000-C000-000000000046")]
        private interface IContextMenu
        {
            [PreserveSig] int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            [PreserveSig] void InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);
            [PreserveSig] void GetCommandString(uint idcmd, uint uflags, uint reserved, [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring, int cch);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct CMINVOKECOMMANDINFOEX
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            public IntPtr lpParameters;
            public IntPtr lpDirectory;
            public int nShow;
            public int dwHotKey;
            public IntPtr hIcon;
            public IntPtr lpTitle;
            public IntPtr lpVerbW;
            public IntPtr lpParametersW;
            public IntPtr lpDirectoryW;
            public IntPtr lpTitleW;
            public POINT ptInvoke;
        }

        private void ShowFileContextMenu(string filePath)
        {
            try
            {
                // 1️⃣ 获取文件 PIDL
                uint dummy;
                SHParseDisplayName(filePath, IntPtr.Zero, out var pidl, 0, out dummy);


                // 2️⃣ 获取父目录对象
                Guid iidShellFolder = typeof(IShellFolder).GUID;
                SHBindToParent(pidl, ref iidShellFolder, out var folder, out var childPidl);

                // 3️⃣ 获取 IContextMenu 接口
                Guid iidContextMenu = typeof(IContextMenu).GUID;
                folder.GetUIObjectOf(IntPtr.Zero, 1, new IntPtr[] { childPidl }, ref iidContextMenu, IntPtr.Zero, out var contextMenuPtr);
                var contextMenu = (IContextMenu)Marshal.GetTypedObjectForIUnknown(contextMenuPtr, typeof(IContextMenu));

                // 4️⃣ 创建菜单
                IntPtr hMenu = User32.CreatePopupMenu();
                contextMenu.QueryContextMenu(hMenu, 0, 1, 0x7FFF, 0x0000);

                GetCursorPos(out POINT pt);
                User32.TrackPopupMenu(hMenu, 0, pt.X, pt.Y, 0, Handle, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show("显示右键菜单失败：" + ex.Message);
            }
        }

        private static class User32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr CreatePopupMenu();

            [DllImport("user32.dll")]
            public static extern bool DestroyMenu(IntPtr hMenu);

            [DllImport("user32.dll")]
            public static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hwnd, IntPtr prcRect);
        }

        #endregion


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

            _toolTip = new ToolTip
            {
                InitialDelay = 0,       // 我们用 Timer 自己做 2 秒延迟
                ReshowDelay = 200,
                AutoPopDelay = 5000,
                ShowAlways = true
            };

            _tipTimer = new Timer { Interval = TipDelayTime }; // 2 秒后显示
            _tipTimer.Tick += (s, e) =>
            {
                _tipTimer.Stop();
                if (_tipIndex >= 0 && _tipNameRect.Contains(PointToClient(MousePosition)))
                {
                    var item = _items[_tipIndex];
                    _toolTip.Show(item.Name ?? string.Empty, this, _tipMousePoint + new Size(38, 18), _toolTip.AutoPopDelay);
                    _tipVisible = true;
                }
            };
        }

        public int RowHeight
        {
            get => _rowHeight;
            set
            {
                _rowHeight = Math.Max(value, _iconSize + 8);
                InvalidateAllTextBitmaps();
                Invalidate();
            }
        }

        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(20, value); Invalidate(); Update(); }
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
            set
            {
                if (value != null && value.Length == 4)
                {
                    _colWidths = value;
                    InvalidateAllTextBitmaps();
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
                    Update(); // 同步刷新以提升双页 hover 同步
                }
            }
        }

        public void NavigateTo(string path)
        {
            path = NormalizeRootPath(path);
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            PrewarmIconsForDirectory(path);

            DisposeAllTextBitmaps();
            _items.Clear();

            LoadFilesCore(path);
            CurrentPath = path;
            _scrollOffsetY = 0;
            BuildPathSegments(path);
            DirectoryChanged?.Invoke(this, CurrentPath);
            Invalidate();
            Update(); // 同步刷新
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
                if (aUp || bUp) return aUp ? bUp ? 0 : -1 : 1;

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

                    // ⭐ 跳过隐藏或系统文件夹
                    if ((di.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                        continue;

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

                    // ⭐ 跳过隐藏或系统文件
                    if ((fi.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                        continue;

                    // ⭐ 另外再排除 Thumbs.db、desktop.ini 这种特殊名字
                    string nameLower = fi.Name.ToLowerInvariant();
                    if (nameLower == "thumbs.db" || nameLower == "desktop.ini")
                        continue;

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

                // —— 名称列（基于 rowTop） ——
                int nameLeft = x0 + 8 + _iconSize + 8;
                var nameRect = new Rectangle(nameLeft, rowTop, colW0 - (nameLeft - x0) - 8, _rowHeight);
                EnsureNameBmp(item, nameRect.Width, nameRect.Height);
                if (item.NameBmp != null) g.DrawImageUnscaled(item.NameBmp, nameRect.Left, nameRect.Top);

                // —— 大小列（基于 rowTop） ——
                string sizeText = item.Size < 0 ? "" : FormatSize(item.Size);
                var sizeRect = new Rectangle(x1 + 6, rowTop, colW1 - 12, _rowHeight);
                EnsureSizeBmp(item, sizeText, sizeRect.Width, sizeRect.Height);
                if (item.SizeBmp != null) g.DrawImageUnscaled(item.SizeBmp, sizeRect.Left, sizeRect.Top);

                // —— 类型列（基于 rowTop） ——
                var typeRect = new Rectangle(x2 + 6, rowTop, colW2 - 12, _rowHeight);
                EnsureTypeBmp(item, item.Type, typeRect.Width, typeRect.Height);
                if (item.TypeBmp != null) g.DrawImageUnscaled(item.TypeBmp, typeRect.Left, typeRect.Top);

                // —— 日期列（基于 rowTop） ——
                string dateText = item.Modified == DateTime.MinValue ? "" : item.Modified.ToString("yyyy-MM-dd HH:mm");
                var dateRect = new Rectangle(x3 + 6, rowTop, colW3 - 12, _rowHeight);
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
            int baseY = y; // ⭐固定在顶部，不受滚动影响

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
                var rect = new Rectangle(x, baseY + 5, textSize.Width, _pathBarHeight - 10);

                TextRenderer.DrawText(g, segLabel, _pathFont, rect, Color.LightBlue,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                _pathSegments.Add((segLabel, fullPath, rect));

                x += rect.Width + 10;
                if (i < parts.Length - 1)
                {
                    TextRenderer.DrawText(g, ">", _itemFont,
                        new Rectangle(x, baseY, 20, _pathBarHeight),
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
            return idx >= 0 && idx < _items.Count ? idx : -1;
        }

        // 鼠标是否命中文件图标/名称区域
        private bool HitFileContent(int index, Point pt)
        {
            if (index < 0 || index >= _items.Count) return false;

            int colW0 = (int)(Width * _colWidths[0]);
            int x0 = 0;
            int rowTop = _pathBarHeight + _headerHeight + index * _rowHeight - _scrollOffsetY;

            // 图标矩形
            int iconX = x0 + 8;
            var iconRect = new Rectangle(iconX, rowTop + (_rowHeight - _iconSize) / 2, _iconSize, _iconSize);

            // 名称矩形
            int nameLeft = x0 + 8 + _iconSize + 8;
            var nameRect = new Rectangle(nameLeft, rowTop, colW0 - (nameLeft - x0) - 8, _rowHeight);

            return iconRect.Contains(pt) || nameRect.Contains(pt);
        }

        // 面包屑命中（仅文字）
        private bool HitBreadcrumb(Point pt, out string fullPath)
        {
            foreach (var seg in _pathSegments)
            {
                if (seg.Bounds.Contains(pt))
                {
                    fullPath = seg.FullPath;
                    return true;
                }
            }
            fullPath = null;
            return false;
        }

        // —— 输入 —— 
        private Point _mouseDownPos;
        private int _mouseDownIndex = -1;
        private const int ClickMoveTolerance = 5;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollOffset += -e.Delta;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int headerTop = _pathBarHeight + _headerHeight;

            // —— 正在拖拽：滚动 + 任何提示隐藏 —— 
            if (_dragging)
            {
                int dy = e.Y - _lastMouseY;
                _lastMouseY = e.Y;
                ScrollOffset -= dy;

                if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
                _tipTimer.Stop();
                _tipIndex = -1;
                _tipNameRect = Rectangle.Empty;
                return;
            }

            // —— 面包屑 hover：小手；清除列表 hover 与 tip —— 
            foreach (var seg in _pathSegments)
            {
                if (seg.Bounds.Contains(e.Location))
                {
                    Cursor = Cursors.Hand;

                    if (_hoverIndex != -1) SetHotIndex(-1, true);
                    if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
                    _tipTimer.Stop();
                    _tipIndex = -1;
                    _tipNameRect = Rectangle.Empty;
                    return;
                }
            }

            // —— Header / PathBar 区域：箭头指针；不触发选择，不显示 tip —— 
            if (e.Y < headerTop)
            {
                Cursor = Cursors.Default;

                if (_hoverIndex != -1) SetHotIndex(-1, true);
                if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
                _tipTimer.Stop();
                _tipIndex = -1;
                _tipNameRect = Rectangle.Empty;
                return;
            }

            // —— 列表区域 —— 
            int idx = HitTestIndex(e.Y);
            if (idx != _hoverIndex) SetHotIndex(idx);

            Cursor = Cursors.Default;

            if (idx >= 0 && idx < _items.Count)
            {
                // 计算“名称列/图标”区域，决定鼠标形状与是否启用 tip
                int colW0 = (int)(Width * _colWidths[0]);
                int x0 = 0;
                int rowTop = headerTop + idx * _rowHeight - _scrollOffsetY;

                // 图标区域
                int iconX = x0 + 8;
                Rectangle iconRect = new Rectangle(
                    iconX,
                    rowTop + (_rowHeight - _iconSize) / 2,
                    _iconSize,
                    _iconSize
                );

                // 名称文本区域（与绘制时一致）
                int nameLeft = x0 + 8 + _iconSize + 8;
                Rectangle nameRect = new Rectangle(
                    nameLeft,
                    rowTop,
                    Math.Max(1, colW0 - (nameLeft - x0) - 8),
                    _rowHeight
                );

                bool overIconOrName = iconRect.Contains(e.Location) || nameRect.Contains(e.Location);
                Cursor = overIconOrName ? Cursors.Hand : Cursors.Default;

                // —— 仅当鼠标在“名称列”内，且文本被截断时，启动 2 秒 tip —— 
                if (nameRect.Contains(e.Location))
                {
                    var item = _items[idx];

                    // 估算是否被截断（宽度大于可见宽度）
                    Size textSize = TextRenderer.MeasureText(
                        item.Name ?? string.Empty,
                        _itemFont,
                        new Size(int.MaxValue, _rowHeight),
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding
                    );

                    bool truncated = textSize.Width > nameRect.Width;

                    if (truncated)
                    {
                        // 若切换到新行/新位置，重启 2 秒计时；保持在同一行则不打断计时
                        bool sameTarget = _tipIndex == idx && nameRect == _tipNameRect && _tipVisible;
                        if (!sameTarget)
                        {
                            if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
                            _tipTimer.Stop();

                            _tipIndex = idx;
                            _tipNameRect = nameRect;
                            _tipMousePoint = e.Location; // 记住当前位置，2 秒后在附近显示

                            _tipTimer.Start();
                        }

                        return; // 在名称列且可能会显示 tip，提前返回
                    }
                }

                // 否则离开名称列或未截断：清理 tip
                if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
                _tipTimer.Stop();
                _tipIndex = -1;
                _tipNameRect = Rectangle.Empty;
                return;
            }

            // 命中不到任何行：清理 hover / tip，恢复箭头
            Cursor = Cursors.Default;
            if (_hoverIndex != -1) SetHotIndex(-1, true);
            if (_tipVisible) { _toolTip.Hide(this); _tipVisible = false; }
            _tipTimer.Stop();
            _tipIndex = -1;
            _tipNameRect = Rectangle.Empty;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                // 面包屑
                if (HitBreadcrumb(e.Location, out var path))
                {
                    NavigateTo(path);
                    return;
                }

                _dragging = true;
                _lastMouseY = e.Y;
                _mouseDownPos = e.Location;
                _mouseDownIndex = HitTestIndex(e.Y);
                // 不强行改光标，保持 hover 的语义
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;

                int upIndex = HitTestIndex(e.Y);
                bool isClick =
                    _mouseDownIndex >= 0 &&
                    upIndex == _mouseDownIndex &&
                    Math.Abs(e.X - _mouseDownPos.X) <= ClickMoveTolerance &&
                    Math.Abs(e.Y - _mouseDownPos.Y) <= ClickMoveTolerance;

                if (isClick && HitFileContent(upIndex, e.Location))
                {
                    OpenItem(upIndex);
                }
            }

            _mouseDownIndex = -1;
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
                _toolTip.Hide(this); _tipVisible = false;            
                _tipTimer.Stop();
                _tipIndex = -1;
                _tipNameRect = Rectangle.Empty;
                FileOpened?.Invoke(this, new[] { item.FullPath });
            }
        }

        public void SetHotIndex(int index, bool raiseEvent = true)
        {
            if (_hoverIndex == index) return;
            int old = _hoverIndex;
            _hoverIndex = index;

            if (raiseEvent) HoverChanged?.Invoke(this, new HoverChangedEventArgs(index));

            if (old >= 0) Invalidate(GetRowRect(old));
            if (_hoverIndex >= 0) Invalidate(GetRowRect(_hoverIndex));
            Update(); // 立即刷新，减少两侧不同步感
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
            nint h = SHGetFileInfo(pathOrExt, attrs, ref sh, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
            if (sh.hIcon != nint.Zero)
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
            public nint hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern nint SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(nint hIcon);

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
