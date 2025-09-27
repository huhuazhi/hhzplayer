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
        private readonly List<FileItem> _items = new();
        private int _rowHeight = 40;        // 行高（同表头）
        private int _hoverIndex = -1;
        private int _selectedIndex = -1;

        private readonly Font _headerFont = new("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _itemFont = new("Segoe UI", 10, FontStyle.Regular);

        private string[] _columns = { "名称", "大小", "类型", "修改日期" };
        private float[] _colWidths = { 0.50f, 0.15f, 0.15f, 0.20f };

        // 图标
        private int _iconSize = 32;
        private Image _folderIcon;
        private readonly Dictionary<string, Image> _iconCacheByExt = new(StringComparer.OrdinalIgnoreCase);

        // 滚动
        private int _scrollOffsetY = 0;
        private int _maxScroll = 0;
        private bool _dragging = false;
        private int _lastMouseY;

        // 颜色
        private readonly Color _bgColor = Color.FromArgb(70, 20, 40, 70);
        private readonly Color _altRowColor = Color.FromArgb(40, 50, 50, 80);
        private readonly Color _hoverColor = Color.FromArgb(80, 0, 120, 215);
        private readonly Color _selectedColor = Color.FromArgb(120, 0, 120, 215);
        private readonly Color _headerBg = Color.FromArgb(100, 0, 122, 204);
        private readonly Pen _borderPen;

        private readonly StringFormat _sfHeader;
        private readonly StringFormat _sfCell;

        // 事件（给 HHZMainPage 用）
        public event EventHandler<string> FileOpened;
        public event EventHandler<string> DirectoryChanged;
        public event EventHandler<HoverChangedEventArgs> HoverChanged;
        public event EventHandler<ViewportOffsetChangedEventArgs> ViewportOffsetChanged;

        public string CurrentPath { get; private set; } = string.Empty;

        public FileList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.Selectable, true);
            UpdateStyles();

            _borderPen = new Pen(Color.FromArgb(150, 0, 120, 215), 2);
            _folderIcon = GetFolderIcon() ?? SystemIcons.WinLogo.ToBitmap();

            _sfHeader = new StringFormat(StringFormatFlags.NoWrap)
            {
                Trimming = StringTrimming.EllipsisCharacter,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            _sfCell = new StringFormat(StringFormatFlags.NoWrap)
            {
                Trimming = StringTrimming.EllipsisCharacter,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            TabStop = true;
            MouseEnter += (_, __) => Focus();
        }

        #region 公共API
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
                    Invalidate();
                    ViewportOffsetChanged?.Invoke(this, new ViewportOffsetChangedEventArgs(_scrollOffsetY));
                }
            }
        }

        public void NavigateTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!Directory.Exists(path)) return;

            LoadFilesCore(path);
            CurrentPath = path;
            DirectoryChanged?.Invoke(this, CurrentPath);
            Invalidate();
        }

        // 兼容旧调用
        public void LoadFiles(string path) => NavigateTo(path);
        #endregion

        #region 加载（稳健处理异常）
        private void LoadFilesCore(string path)
        {
            _items.Clear();

            DirectoryInfo diRoot;
            try
            {
                diRoot = new DirectoryInfo(path);
            }
            catch
            {
                return;
            }

            // 顶部两个“..”
            if (diRoot.Parent != null)
            {
                // 第一行 ..
                _items.Add(new FileItem
                {
                    Name = "..",
                    FullPath = diRoot.Parent.FullName,
                    IsDir = true,
                    Size = -1,
                    Type = "上一级目录",
                    Modified = DateTime.Now,
                    Icon = _folderIcon
                });
                // 第二行 ..
                _items.Add(new FileItem
                {
                    Name = "..",
                    FullPath = diRoot.Parent.FullName,
                    IsDir = true,
                    Size = -1,
                    Type = "上一级目录",
                    Modified = DateTime.Now,
                    Icon = _folderIcon
                });
            }

            // —— 目录（尽量忽略无权限项）——
            IEnumerable<string> dirs = Array.Empty<string>();
            try
            {
#if NET5_0_OR_GREATER
                var opt = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false, AttributesToSkip = 0 };
                dirs = Directory.EnumerateDirectories(path, "*", opt);
#else
                dirs = Directory.GetDirectories(path);
#endif
            }
            catch { /* 忽略根枚举异常 */ }

            foreach (var dir in dirs)
            {
                try
                {
                    var di = new DirectoryInfo(dir);
                    DateTime mtime;
                    try { mtime = di.LastWriteTime; } catch { mtime = DateTime.MinValue; }

                    _items.Add(new FileItem
                    {
                        Name = di.Name,
                        FullPath = dir,
                        IsDir = true,
                        Size = -1,
                        Type = "文件夹",
                        Modified = mtime,
                        Icon = _folderIcon // 不去取特殊文件夹图标，稳定
                    });
                }
                catch
                {
                    // 单项失败忽略
                }
            }

            // —— 文件（同样逐条防护）——
            IEnumerable<string> files = Array.Empty<string>();
            try
            {
#if NET5_0_OR_GREATER
                var opt = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false, AttributesToSkip = 0 };
                files = Directory.EnumerateFiles(path, "*", opt);
#else
                files = Directory.GetFiles(path);
#endif
            }
            catch { }

            foreach (var file in files)
            {
                try
                {
                    var fi = new FileInfo(file);
                    string ext = fi.Extension;
                    DateTime mtime;
                    try { mtime = fi.LastWriteTime; } catch { mtime = DateTime.MinValue; }

                    _items.Add(new FileItem
                    {
                        Name = fi.Name,
                        FullPath = file,
                        IsDir = false,
                        Size = SafeGetLength(fi),
                        Type = string.IsNullOrEmpty(ext) ? "文件" : ext.ToUpperInvariant(),
                        Modified = mtime,
                        Icon = GetFileIconByExtension(ext) ?? _folderIcon
                    });
                }
                catch
                {
                    // 忽略该文件
                }
            }

            _scrollOffsetY = 0; // 重置滚动
        }

        private static long SafeGetLength(FileInfo fi)
        {
            try { return fi.Length; } catch { return -1; }
        }
        #endregion

        #region 绘制
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 背景
            using (var bg = new SolidBrush(_bgColor))
                g.FillRectangle(bg, ClientRectangle);

            int headerHeight = _rowHeight;

            // 表头
            using (var headBg = new SolidBrush(_headerBg))
                g.FillRectangle(headBg, new Rectangle(0, 0, Width, headerHeight));

            int colX = 0;
            for (int i = 0; i < _columns.Length; i++)
            {
                int colWidth = (int)(Width * _colWidths[i]);
                var rect = new Rectangle(colX + 8, 0, colWidth - 16, headerHeight);
                g.DrawString(_columns[i], _headerFont, Brushes.White, rect, _sfHeader);
                colX += colWidth;
            }

            // 内容
            var contentClip = new Rectangle(0, headerHeight, Width, Height - headerHeight);
            g.SetClip(contentClip);
            g.TranslateTransform(0, headerHeight - _scrollOffsetY);

            int colW0 = (int)(Width * _colWidths[0]);
            int colW1 = (int)(Width * _colWidths[1]);
            int colW2 = (int)(Width * _colWidths[2]);
            int colW3 = (int)(Width * _colWidths[3]);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                int y = i * _rowHeight;
                var rowRect = new Rectangle(0, y, Width, _rowHeight);

                if ((i & 1) == 1)
                    using (var alt = new SolidBrush(_altRowColor))
                        g.FillRectangle(alt, rowRect);

                if (i == _hoverIndex)
                    using (var hov = new SolidBrush(_hoverColor))
                        g.FillRectangle(hov, rowRect);
                if (i == _selectedIndex)
                    using (var sel = new SolidBrush(_selectedColor))
                        g.FillRectangle(sel, rowRect);

                int x0 = 0, x1 = x0 + colW0, x2 = x1 + colW1, x3 = x2 + colW2;

                if (item.Icon != null)
                {
                    int iconX = x0 + 8;
                    int iconY = y + (_rowHeight - _iconSize) / 2;
                    g.DrawImage(item.Icon, new Rectangle(iconX, iconY, _iconSize, _iconSize));
                }
                int nameLeft = x0 + 8 + _iconSize + 8;
                g.DrawString(item.Name, _itemFont, Brushes.White,
                    new Rectangle(nameLeft, y, colW0 - (nameLeft - x0) - 8, _rowHeight), _sfCell);

                string sizeText = item.Size < 0 ? "" : FormatSize(item.Size);
                g.DrawString(sizeText, _itemFont, Brushes.White,
                    new Rectangle(x1 + 6, y, colW1 - 12, _rowHeight), _sfCell);

                g.DrawString(item.Type, _itemFont, Brushes.White,
                    new Rectangle(x2 + 6, y, colW2 - 12, _rowHeight), _sfCell);

                g.DrawString(item.Modified.ToString("yyyy-MM-dd HH:mm"), _itemFont, Brushes.White,
                    new Rectangle(x3 + 6, y, colW3 - 12, _rowHeight), _sfCell);
            }

            g.ResetTransform();
            g.ResetClip();

            int itemsHeight = _items.Count * _rowHeight;
            int visibleContent = Math.Max(0, Height - headerHeight);
            _maxScroll = Math.Max(0, itemsHeight - visibleContent);

            g.DrawRectangle(_borderPen, 1, 1, Width - 2, Height - 2);
        }
        #endregion

        #region 交互
        private int HitTestIndex(int mouseY)
        {
            int headerHeight = _rowHeight;
            int contentY = mouseY - headerHeight + _scrollOffsetY;
            if (contentY < 0) return -1;
            int index = contentY / _rowHeight;
            return (index >= 0 && index < _items.Count) ? index : -1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollOffset += -e.Delta;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _lastMouseY = e.Y;
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                int dy = e.Y - _lastMouseY;
                ScrollOffset -= dy;
                _lastMouseY = e.Y;
            }
            else
            {
                int idx = HitTestIndex(e.Y);
                if (idx != _hoverIndex)
                {
                    SetHotIndex(idx);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;
                Cursor = Cursors.Default;

                int idx = HitTestIndex(e.Y);
                if (idx >= 0)
                {
                    _selectedIndex = idx;
                    var item = _items[idx];
                    if (item.IsDir)
                        NavigateTo(item.FullPath); // 点击文件夹/“..” 进入
                    else
                        FileOpened?.Invoke(this, item.FullPath);
                }
                Invalidate();
            }
        }
        #endregion

        #region 外部可调用
        public void SetHotIndex(int index, bool raiseEvent = true)
        {
            _hoverIndex = index;
            if (raiseEvent)
                HoverChanged?.Invoke(this, new HoverChangedEventArgs(index));
            Invalidate();
        }
        #endregion

        #region 工具
        private static string FormatSize(long size)
        {
            const double KB = 1024.0, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            if (size >= TB) return $"{size / TB:0.##} TB";
            if (size >= GB) return $"{size / GB:0.##} GB";
            if (size >= MB) return $"{size / MB:0.##} MB";
            if (size >= KB) return $"{size / KB:0.##} KB";
            return $"{size} B";
        }
        #endregion

        #region Shell 图标获取（容错）
        private Image GetFolderIcon()
        {
            try
            {
                SHFILEINFO shinfo = new();
                uint flags = SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES;
                SHGetFileInfo("dummy", FILE_ATTRIBUTE_DIRECTORY, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                if (shinfo.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        using var icon = Icon.FromHandle(shinfo.hIcon);
                        return new Bitmap(icon.ToBitmap());
                    }
                    finally { DestroyIcon(shinfo.hIcon); }
                }
            }
            catch { }
            return null;
        }

        private Image GetFileIconByExtension(string ext)
        {
            if (string.IsNullOrEmpty(ext)) ext = ".";
            if (_iconCacheByExt.TryGetValue(ext, out var cached))
                return cached;

            try
            {
                SHFILEINFO shinfo = new();
                uint flags = SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES;
                SHGetFileInfo("dummy" + ext, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                if (shinfo.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        using var icon = Icon.FromHandle(shinfo.hIcon);
                        var bmp = new Bitmap(icon.ToBitmap());
                        _iconCacheByExt[ext] = bmp;
                        return bmp;
                    }
                    finally { DestroyIcon(shinfo.hIcon); }
                }
            }
            catch { }
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
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x00000000; // 32x32
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        #endregion

        #region 内部类 & 事件参数
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
#endregion