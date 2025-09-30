using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MpvNet.Windows.WinForms
{
    /// <summary>
    /// 单控件：包含磁盘列表 + 文件列表；支持单屏/3D模式（左右各一套）。
    /// - 左侧：磁盘列表（卡片式，两行文字+进度条+图标）
    /// - 右侧：文件列表（详细信息风格，32x32 图标 + 名称/大小/类型/时间）
    /// - 外部事件：FileOpened
    /// - 外部方法：NavigateTo(string path) 供父窗体调用（两侧同步）
    /// </summary>
    public class DiskFileList : SKControl
    {
        #region 布局常量（沿用你原 HHZMainPage_SC 的蓝框比例）
        private const float FRAME_LEFT = 0.0255f;
        private const float FRAME_TOP = 0.095f;
        private const float FRAME_WIDTH = 0.253f;
        private const float FRAME_HEIGHT = 0.400f;

        private float _fileListBottomGapRatio = 0.07f; // 底部留白比例
        #endregion

        #region 外部可设置
        public bool Enable3DMode
        {
            get => _enable3DMode;
            set { _enable3DMode = value; Invalidate(); }
        }
        private bool _enable3DMode = false;

        public int DiskRowHeight { get => _diskRowHeight; set { _diskRowHeight = Math.Max(64, value); Invalidate(); } }
        public int DiskRowSpacing { get => _diskRowSpacing; set { _diskRowSpacing = Math.Max(0, value); Invalidate(); } }

        public int FileRowHeight { get => _fileRowHeight; set { _fileRowHeight = Math.Max(28, value); Invalidate(); } }

        public event EventHandler<string> FileOpened;
        #endregion

        #region 内部模型/状态

        private enum ViewMode { Disk, File }

        private sealed class PanelState
        {
            // 磁盘区
            public int DiskScroll = 0;
            public int DiskHover = -1;
            public int DiskSelected = -1;

            // 文件区
            public int FileScroll = 0;
            public int FileHover = -1;
            public int FileSelected = -1;

            public ViewMode Mode = ViewMode.Disk;
            public string CurrentPath = string.Empty;

            // 数据
            public readonly List<DriveItem> Drives = new();
            public readonly List<FileItem> Files = new();

            // 运行时布局
            public Rectangle DiskRect; // 设备坐标
            public Rectangle FileRect;

            // 拖拽滚动
            public bool Dragging = false;
            public bool DragInDisk = false;
            public int DragLastY = 0;
            public int DragStartScroll = 0;

            // 点击判定
            public Point MouseDownPos;
            public int MouseDownIndex = -1;
            public bool MouseDownInDisk = false;
        }

        private sealed class DriveItem
        {
            public string Root;
            public string Line1;
            public string Line2;
            public float Usage01;
            public SKBitmap Icon; // 可空
        }

        private sealed class FileItem
        {
            public string Name;
            public string FullPath;
            public bool IsDir;
            public long Size;
            public string Type;
            public DateTime Modified;
            public SKBitmap Icon; // 可空
        }

        private readonly PanelState _left = new();
        private readonly PanelState _right = new();

        // 行高/间距/图标大小
        private int _diskRowHeight = 110;
        private int _diskRowSpacing = 5;

        private int _fileRowHeight = 80;
        private const int DriveIconSize = 48;
        private const int FileIconSize = 32;

        // 点击判定
        private const int ClickMoveTolerance = 5;

        #endregion

        #region 图标缓存（静态共享）
        private static readonly object s_iconLock = new();
        private static readonly Dictionary<string, SKBitmap> s_iconByExt = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, SKBitmap> s_driveIconByRoot = new(StringComparer.OrdinalIgnoreCase);
        private static SKBitmap s_folderIcon;
        private static SKBitmap s_defaultFileIcon;

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
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        #endregion

        public DiskFileList()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;   // 允许透过去
            DoubleBuffered = true;

            // 数据预热
            EnsureGlobalIcons();

            // 初次加载磁盘
            ReloadDisks(_left);
            ReloadDisks(_right);

            // 输入
            MouseMove += OnMouseMove;
            MouseLeave += (_, __) => { _left.DiskHover = _left.FileHover = -1; _right.DiskHover = _right.FileHover = -1; Invalidate(); };
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            // 绘制
            PaintSurface += OnPaintSurface;
            SizeChanged += (_, __) => Invalidate();
        }

        #region 对外导航（兼容 HHZMainPage_SC.LoadFolder 调用）
        public void NavigateTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            // 两侧同步
            NavigateToInternal(_left, path);
            NavigateToInternal(_right, path);
            Invalidate();
        }
        #endregion

        #region 绘制
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(new SKColor(0, 0, 0, 0)); // 透明，由父背景负责

            if (Enable3DMode)
            {
                int half = Width / 2;
                var leftHost = new Rectangle(0, 0, half, Height);
                var rightHost = new Rectangle(half, 0, Width - half, Height);

                LayoutPanel(_left, leftHost);
                LayoutPanel(_right, rightHost);

                DrawPanel(canvas, _left, true);
                DrawPanel(canvas, _right, false);
            }
            else
            {
                var host = new Rectangle(0, 0, Width, Height);
                LayoutPanel(_left, host);
                DrawPanel(canvas, _left, true);
            }
        }

        private void LayoutPanel(PanelState state, Rectangle host)
        {
            // 蓝框区域
            var frame = CalcBlueFrame(host);

            // 磁盘列表占蓝框
            state.DiskRect = frame;

            // 文件列表在 磁盘列表右侧，贴合上边，底边保持比例留白
            int bottomGap = (int)Math.Round(Height * _fileListBottomGapRatio);
            var fileLeft = state.DiskRect.Right + 20;
            var fileTop = state.DiskRect.Top;
            var fileRight = host.Right - 40;
            var fileBottom = Height - bottomGap;

            if (fileRight < fileLeft + 80) fileRight = fileLeft + 80;
            if (fileBottom < fileTop + 60) fileBottom = fileTop + 60;

            state.FileRect = Rectangle.FromLTRB(fileLeft, fileTop, fileRight, fileBottom);

            // 视口变了，滚动夹取
            ClampScrolls(state);
        }

        private void DrawPanel(SKCanvas canvas, PanelState s, bool isLeft)
        {
            // 半透明底（列表区域）
            using var listBg = new SKPaint { Color = new SKColor(30, 30, 30, 140), IsAntialias = true };
            canvas.DrawRect(ToSKRect(s.DiskRect), listBg);
            canvas.DrawRect(ToSKRect(s.FileRect), listBg);

            // 边框
            using var border = new SKPaint { Color = new SKColor(80, 140, 255, 180), IsStroke = true, StrokeWidth = 1.5f, IsAntialias = true };
            canvas.DrawRect(ToSKRect(Inflate(s.DiskRect, -1)), border);
            canvas.DrawRect(ToSKRect(Inflate(s.FileRect, -1)), border);

            DrawDiskList(canvas, s);
            DrawFileList(canvas, s);
        }

        private void DrawDiskList(SKCanvas c, PanelState s)
        {
            var r = s.DiskRect;
            int y = r.Top - s.DiskScroll;

            // —— 字体：微软雅黑（主），配合回退字体 —— 
            using var f1 = MakeFont(14, bold: true);
            using var f2 = MakeFont(12, bold: false);

            using var br1 = new SKPaint
            {
                Color = SKColors.White,
                TextSize = f1.TextSize,
                Typeface = f1.Typeface,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                HintingLevel = SKPaintHinting.Full
            };
            using var br2 = new SKPaint
            {
                Color = new SKColor(215, 220, 230),
                TextSize = f2.TextSize,
                Typeface = f2.Typeface,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                HintingLevel = SKPaintHinting.Full
            };

            using var barBg = new SKPaint { Color = new SKColor(100, 120, 140, 160) };
            using var barFg = new SKPaint { Color = new SKColor(120, 180, 255, 220) };
            using var hover = new SKPaint { Color = new SKColor(0, 120, 215, 60) };
            using var sel = new SKPaint { Color = new SKColor(0, 120, 215, 120) };

            for (int i = 0; i < s.Drives.Count; i++)
            {
                var row = new Rectangle(r.Left, y, r.Width, _diskRowHeight);
                if (row.Bottom >= r.Top && row.Top <= r.Bottom)
                {
                    if (i == s.DiskHover) c.DrawRect(ToSKRect(row), hover);
                    if (i == s.DiskSelected) c.DrawRect(ToSKRect(row), sel);

                    // 图标
                    var icoDest = new SKRect(row.Left + 12, row.Top + (_diskRowHeight - DriveIconSize) / 2f, row.Left + 12 + DriveIconSize, row.Top + (_diskRowHeight + DriveIconSize) / 2f);
                    var it = s.Drives[i];
                    if (it.Icon != null) c.DrawBitmap(it.Icon, icoDest);

                    int textX = (int)icoDest.Right + 10;
                    int textW = row.Right - 12 - textX;

                    var line1Rect = new SKRect(textX, row.Top + 6, textX + textW, row.Top + 6 + 28);
                    var line2Rect = new SKRect(textX, row.Top + 6 + 28, textX + textW, row.Bottom - 24);
                    DrawTextOneLine(c, br1, it.Line1, line1Rect);
                    DrawTextOneLine(c, br2, it.Line2, line2Rect);

                    int barLeft = textX;
                    int barRight = row.Right - 12;
                    int barY = row.Bottom - 14;
                    c.DrawRect(new SKRect(barLeft, barY, barRight, barY + 3), barBg);
                    c.DrawRect(new SKRect(barLeft, barY, barLeft + (barRight - barLeft) * it.Usage01, barY + 3), barFg);
                }
                y += _diskRowHeight + _diskRowSpacing;
            }
        }

        private void DrawFileList(SKCanvas c, PanelState s)
        {
            var r = s.FileRect;

            // 表头
            int headerH = FileRowHeight;
            using var headBg = new SKPaint { Color = new SKColor(0, 122, 204, 150) };
            c.DrawRect(new SKRect(r.Left, r.Top, r.Right, r.Top + headerH), headBg);

            using var headText = MakePaint(17, bold: true, color: SKColors.White);
            headText.SubpixelText = true; headText.LcdRenderText = true; headText.HintingLevel = SKPaintHinting.Full;

            int colW0 = (int)(r.Width * 0.50f);
            int colW1 = (int)(r.Width * 0.15f);
            int colW2 = (int)(r.Width * 0.15f);
            int colW3 = r.Width - colW0 - colW1 - colW2;
            int x0 = r.Left, x1 = x0 + colW0, x2 = x1 + colW1, x3 = x2 + colW2;

            DrawTextVCenter(c, headText, "名称", new SKRect(x0 + 8, r.Top, x0 + colW0 - 8, r.Top + headerH));
            DrawTextVCenter(c, headText, "大小", new SKRect(x1 + 8, r.Top, x1 + colW1 - 8, r.Top + headerH));
            DrawTextVCenter(c, headText, "类型", new SKRect(x2 + 8, r.Top, x2 + colW2 - 8, r.Top + headerH));
            DrawTextVCenter(c, headText, "修改日期", new SKRect(x3 + 8, r.Top, x3 + colW3 - 8, r.Top + headerH));

            // 行
            int viewportTop = r.Top + headerH;
            int viewportH = r.Height - headerH;
            int first = Math.Max(0, s.FileScroll / FileRowHeight);
            int last = Math.Min(s.Files.Count - 1, (s.FileScroll + viewportH) / FileRowHeight);

            using var rowAlt = new SKPaint { Color = new SKColor(255, 255, 255, 12) };
            using var hover = new SKPaint { Color = new SKColor(0, 120, 215, 70) };
            using var sel = new SKPaint { Color = new SKColor(0, 120, 215, 140) };
            using var text = MakePaint(15, bold: false, color: SKColors.White);
            text.SubpixelText = true; text.LcdRenderText = true; text.HintingLevel = SKPaintHinting.Full;

            for (int i = first; i <= last; i++)
            {
                int drawY = viewportTop + i * FileRowHeight - s.FileScroll;
                var rowRect = new SKRect(r.Left, drawY, r.Right, drawY + FileRowHeight);

                if ((i & 1) == 1) c.DrawRect(rowRect, rowAlt);
                if (i == s.FileHover) c.DrawRect(rowRect, hover);
                if (i == s.FileSelected) c.DrawRect(rowRect, sel);

                // 图标 + 名称
                var item = s.Files[i];
                if (item.Icon != null)
                {
                    var dest = new SKRect(x0 + 8, drawY + (FileRowHeight - FileIconSize) / 2f, x0 + 8 + FileIconSize, drawY + (FileRowHeight + FileIconSize) / 2f);
                    c.DrawBitmap(item.Icon, dest);
                }
                float nameLeft = x0 + 8 + FileIconSize + 8;
                DrawTextVCenterEllipsis(c, text, item.Name, new SKRect(nameLeft, drawY, x0 + colW0 - 8, drawY + FileRowHeight));

                // 大小
                var sizeText = item.IsDir ? "" : FormatSize(item.Size);
                DrawTextVCenterEllipsis(c, text, sizeText, new SKRect(x1 + 8, drawY, x1 + colW1 - 8, drawY + FileRowHeight));

                // 类型
                DrawTextVCenterEllipsis(c, text, item.Type, new SKRect(x2 + 8, drawY, x2 + colW2 - 8, drawY + FileRowHeight));

                // 时间
                DrawTextVCenterEllipsis(c, text, item.Modified.ToString("yyyy-MM-dd HH:mm"), new SKRect(x3 + 8, drawY, x3 + colW3 - 8, drawY + FileRowHeight));
            }
        }
        #endregion

        #region 输入与命中
        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            var (p, inDisk) = HitWhichPanelAndArea(e.Location);
            if (p == null) return;

            if (inDisk)
                p.DiskScroll = ClampScroll(p.DiskScroll - Math.Sign(e.Delta) * (_diskRowHeight + _diskRowSpacing), 0, GetDiskMaxScroll(p));
            else
                p.FileScroll = ClampScroll(p.FileScroll - Math.Sign(e.Delta) * FileRowHeight, 0, GetFileMaxScroll(p));

            Invalidate();
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var (p, inDisk) = HitWhichPanelAndArea(e.Location);
            if (p == null) return;

            p.Dragging = true;
            p.DragInDisk = inDisk;
            p.DragLastY = e.Y;
            p.DragStartScroll = inDisk ? p.DiskScroll : p.FileScroll;

            p.MouseDownPos = e.Location;
            p.MouseDownInDisk = inDisk;
            p.MouseDownIndex = inDisk ? HitDiskIndex(p, e.Location) : HitFileIndex(p, e.Location);
            Capture = true;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            var (p, inDisk) = HitWhichPanelAndArea(e.Location);
            if (p == null) return;

            if (p.Dragging && p.DragInDisk == inDisk)
            {
                int dy = e.Y - p.DragLastY;
                p.DragLastY = e.Y;

                if (inDisk)
                {
                    p.DiskScroll = ClampScroll(p.DragStartScroll - (e.Y - p.MouseDownPos.Y), 0, GetDiskMaxScroll(p));
                }
                else
                {
                    p.FileScroll = ClampScroll(p.DragStartScroll - (e.Y - p.MouseDownPos.Y), 0, GetFileMaxScroll(p));
                }
                Invalidate();
                return;
            }

            // Hover
            int idx = inDisk ? HitDiskIndex(p, e.Location) : HitFileIndex(p, e.Location);
            if (inDisk)
            {
                if (idx != p.DiskHover) { p.DiskHover = idx; Invalidate(); }
            }
            else
            {
                if (idx != p.FileHover) { p.FileHover = idx; Invalidate(); }
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var (p, inDisk) = HitWhichPanelAndArea(e.Location);
            if (p == null) { Capture = false; return; }

            bool isClick = p.MouseDownIndex >= 0 &&
                           p.MouseDownInDisk == inDisk &&
                           Math.Abs(e.X - p.MouseDownPos.X) <= ClickMoveTolerance &&
                           Math.Abs(e.Y - p.MouseDownPos.Y) <= ClickMoveTolerance;

            if (isClick)
            {
                if (inDisk)
                {
                    int idx = HitDiskIndex(p, e.Location);
                    if (idx >= 0 && idx < p.Drives.Count)
                    {
                        p.DiskSelected = idx;
                        // 任一侧点磁盘 → 两侧文件列表都导航到同一根
                        var root = p.Drives[idx].Root;
                        NavigateTo(root);
                    }
                }
                else
                {
                    int idx = HitFileIndex(p, e.Location);
                    if (idx >= 0 && idx < p.Files.Count)
                    {
                        p.FileSelected = idx;
                        var it = p.Files[idx];
                        if (it.IsDir)
                        {
                            if (it.Name == "..")
                            {
                                NavigateUp(p);
                            }
                            else
                            {
                                NavigateToInternal(p, it.FullPath);
                            }
                        }
                        else
                        {
                            FileOpened?.Invoke(this, it.FullPath);
                        }
                    }
                }
                Invalidate();
            }

            p.Dragging = false;
            Capture = false;
        }

        private (PanelState? panel, bool inDiskArea) HitWhichPanelAndArea(Point pt)
        {
            if (Enable3DMode)
            {
                int half = Width / 2;
                bool isLeft = pt.X < half;
                var p = isLeft ? _left : _right;
                bool inDisk = p.DiskRect.Contains(pt);
                bool inFile = !inDisk && p.FileRect.Contains(pt);
                if (!inDisk && !inFile) return (null, false);
                return (p, inDisk);
            }
            else
            {
                var p = _left;
                bool inDisk = p.DiskRect.Contains(pt);
                bool inFile = !inDisk && p.FileRect.Contains(pt);
                if (!inDisk && !inFile) return (null, false);
                return (p, inDisk);
            }
        }

        private int HitDiskIndex(PanelState s, Point pt)
        {
            if (!s.DiskRect.Contains(pt)) return -1;
            int y = pt.Y - s.DiskRect.Top + s.DiskScroll;
            int stride = _diskRowHeight + _diskRowSpacing;
            int idx = y / stride;
            int topOfRow = idx * stride;
            if (idx >= 0 && idx < s.Drives.Count && (y - topOfRow) < _diskRowHeight) return idx;
            return -1;
        }

        private int HitFileIndex(PanelState s, Point pt)
        {
            if (!s.FileRect.Contains(pt)) return -1;
            int headerH = FileRowHeight;
            int y = pt.Y - (s.FileRect.Top + headerH) + s.FileScroll;
            if (y < 0) return -1;
            int idx = y / FileRowHeight;
            return (idx >= 0 && idx < s.Files.Count) ? idx : -1;
        }
        #endregion

        #region 数据加载
        private void ReloadDisks(PanelState s)
        {
            s.Mode = ViewMode.Disk;
            s.CurrentPath = "";
            s.Drives.Clear();

            foreach (var di in DriveInfo.GetDrives())
            {
                if (di.DriveType == DriveType.CDRom) continue;
                if (!SafeIsReady(di)) continue;

                long total = 0, free = 0, used = 0;
                string vol = "";
                try
                {
                    vol = di.VolumeLabel; if (string.IsNullOrWhiteSpace(vol)) vol = "本地磁盘";
                    total = di.TotalSize; free = di.TotalFreeSpace; used = total - free;
                }
                catch { }
                string root = di.RootDirectory.FullName;
                float usage01 = (total > 0) ? (float)used / total : 0f;

                var icon = GetDriveIcon(root);

                s.Drives.Add(new DriveItem
                {
                    Root = root,
                    Line1 = $"{vol} ({root.TrimEnd('\\')})",
                    Line2 = (total > 0) ? $"{FormatSize(free)} 可用, 共 {FormatSize(total)}" : "不可用",
                    Usage01 = usage01,
                    Icon = icon
                });
            }

            s.DiskScroll = 0;
            s.DiskHover = s.DiskSelected = -1;
        }

        private void NavigateToInternal(PanelState s, string path)
        {
            DirectoryInfo dir;
            try { dir = new DirectoryInfo(path); }
            catch { return; }
            if (!dir.Exists) return;

            s.Mode = ViewMode.File;
            s.CurrentPath = path;
            s.Files.Clear();

            // 顶部 “..” 返回
            if (dir.Parent != null)
            {
                s.Files.Add(new FileItem
                {
                    Name = "..",
                    FullPath = dir.Parent.FullName,
                    IsDir = true,
                    Size = -1,
                    Type = "上级目录",
                    Modified = DateTime.Now,
                    Icon = s_folderIcon
                });
            }

            // 目录
            try
            {
                foreach (var d in dir.GetDirectories())
                {
                    s.Files.Add(new FileItem
                    {
                        Name = d.Name,
                        FullPath = d.FullName,
                        IsDir = true,
                        Size = -1,
                        Type = "文件夹",
                        Modified = d.LastWriteTime,
                        Icon = s_folderIcon
                    });
                }
            }
            catch { }

            // 文件
            try
            {
                foreach (var f in dir.GetFiles())
                {
                    string ext = f.Extension;
                    if (string.IsNullOrEmpty(ext)) ext = ".";
                    var icon = GetExtIcon(ext) ?? s_defaultFileIcon;

                    s.Files.Add(new FileItem
                    {
                        Name = f.Name,
                        FullPath = f.FullName,
                        IsDir = false,
                        Size = f.Length,
                        Type = string.IsNullOrEmpty(f.Extension) ? "文件" : f.Extension.ToUpperInvariant(),
                        Modified = f.LastWriteTime,
                        Icon = icon
                    });
                }
            }
            catch { }

            s.FileScroll = 0;
            s.FileHover = s.FileSelected = -1;
        }

        private void NavigateUp(PanelState s)
        {
            if (string.IsNullOrEmpty(s.CurrentPath)) { ReloadDisks(s); return; }
            try
            {
                var dir = new DirectoryInfo(s.CurrentPath);
                if (dir.Parent == null) ReloadDisks(s);
                else NavigateToInternal(s, dir.Parent.FullName);
            }
            catch { }
        }

        private static bool SafeIsReady(DriveInfo d) { try { return d.IsReady; } catch { return false; } }
        #endregion

        #region 工具/布局/绘制辅助
        private static Rectangle CalcBlueFrame(Rectangle host)
        {
            int fx = host.X + (int)(host.Width * FRAME_LEFT);
            int fy = host.Y + (int)(host.Height * FRAME_TOP);
            int fw = (int)(host.Width * FRAME_WIDTH);
            int fh = (int)(host.Height * FRAME_HEIGHT);
            const int pad = 10;
            return new Rectangle(fx + pad, fy + pad,
                                 Math.Max(10, fw - pad * 2),
                                 Math.Max(10, fh - pad * 2));
        }

        private static Rectangle Inflate(Rectangle r, int delta)
        {
            r.Inflate(delta, delta);
            return r;
        }

        private void ClampScrolls(PanelState s)
        {
            s.DiskScroll = ClampScroll(s.DiskScroll, 0, GetDiskMaxScroll(s));
            s.FileScroll = ClampScroll(s.FileScroll, 0, GetFileMaxScroll(s));
        }

        private int GetDiskMaxScroll(PanelState s)
        {
            int content = Math.Max(0, s.Drives.Count * (_diskRowHeight + _diskRowSpacing) - _diskRowSpacing);
            int view = Math.Max(0, s.DiskRect.Height);
            return Math.Max(0, content - view);
        }

        private int GetFileMaxScroll(PanelState s)
        {
            int headerH = FileRowHeight;
            int content = s.Files.Count * FileRowHeight;
            int view = Math.Max(0, s.FileRect.Height - headerH);
            return Math.Max(0, content - view);
        }

        private static int ClampScroll(int v, int min, int max) => (v < min) ? min : (v > max ? max : v);

        private static SKRect ToSKRect(Rectangle r) => new SKRect(r.Left, r.Top, r.Right, r.Bottom);

        private static void DrawTextOneLine(SKCanvas c, SKPaint p, string text, SKRect rect)
        {
            if (string.IsNullOrEmpty(text)) return;
            var bounds = new SKRect();
            p.MeasureText(text, ref bounds);
            float x = rect.Left;
            float y = rect.Top + (rect.Height + (bounds.Bottom - bounds.Top)) / 2f - bounds.Bottom;
            c.DrawText(text, x, y, p);
        }

        private static void DrawTextVCenter(SKCanvas c, SKPaint p, string text, SKRect r)
        {
            var bounds = new SKRect();
            p.MeasureText(text ?? "", ref bounds);
            float x = r.Left;
            float y = r.Top + (r.Height + (bounds.Bottom - bounds.Top)) / 2f - bounds.Bottom;
            c.DrawText(text ?? "", x, y, p);
        }

        private static void DrawTextVCenterEllipsis(SKCanvas c, SKPaint p, string text, SKRect r)
        {
            if (string.IsNullOrEmpty(text)) return;
            float width = p.MeasureText(text);
            if (width <= r.Width) { DrawTextVCenter(c, p, text, r); return; }
            string ell = "…";
            float ellW = p.MeasureText(ell);
            int len = text.Length;
            while (len > 0 && p.MeasureText(text.AsSpan(0, len)) + ellW > r.Width) len--;
            string t = (len > 0 ? text[..len] : "") + ell;
            DrawTextVCenter(c, p, t, r);
        }

        // —— 字体工具（微软雅黑优先 + 回退）——
        private static SKTypeface GetTypeface(bool bold)
        {
            // 优先：Microsoft YaHei；回退：Segoe UI / Arial Unicode MS / Noto Sans CJK SC
            var mgr = SKFontManager.Default;
            SKTypeface tf = mgr.MatchFamily("Microsoft YaHei", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            if (tf == null) tf = mgr.MatchFamily("Segoe UI", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            if (tf == null) tf = mgr.MatchFamily("Arial Unicode MS", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            if (tf == null) tf = mgr.MatchFamily("Noto Sans CJK SC", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            if (tf == null) tf = SKTypeface.Default;
            return tf;
        }

        private static SKPaint MakePaint(float size, bool bold, SKColor color)
        {
            return new SKPaint
            {
                Color = color,
                TextSize = size,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                HintingLevel = SKPaintHinting.Full,
                Typeface = GetTypeface(bold)
            };
        }

        private static SKPaint MakeFont(float size, bool bold) => MakePaint(size, bold, SKColors.White);
        #endregion

        #region 图标工具
        private static void EnsureGlobalIcons()
        {
            if (s_folderIcon != null && s_defaultFileIcon != null) return;
            lock (s_iconLock)
            {
                if (s_folderIcon == null)
                    s_folderIcon = GetShellIconBitmapSkia("dummy", FILE_ATTRIBUTE_DIRECTORY, 32, 32) ?? CreateColorSquare(new SKColor(70, 120, 220));
                if (s_defaultFileIcon == null)
                    s_defaultFileIcon = GetShellIconBitmapSkia("dummy.", FILE_ATTRIBUTE_NORMAL, 32, 32) ?? CreateColorSquare(new SKColor(160, 160, 160));
            }
        }

        private static SKBitmap GetDriveIcon(string root)
        {
            lock (s_iconLock)
            {
                if (s_driveIconByRoot.TryGetValue(root, out var bmp)) return bmp;
            }

            var icon = GetShellIconBitmapSkia(root, 0, DriveIconSize, DriveIconSize);
            if (icon == null) icon = CreateColorSquare(new SKColor(120, 120, 200));
            lock (s_iconLock)
            {
                s_driveIconByRoot[root] = icon;
            }
            return icon;
        }

        private static SKBitmap GetExtIcon(string ext)
        {
            if (string.IsNullOrEmpty(ext)) ext = ".";
            lock (s_iconLock)
            {
                if (s_iconByExt.TryGetValue(ext, out var bmp)) return bmp;
            }
            var icon = GetShellIconBitmapSkia("dummy" + ext, FILE_ATTRIBUTE_NORMAL, FileIconSize, FileIconSize) ?? s_defaultFileIcon;
            lock (s_iconLock)
            {
                if (!s_iconByExt.ContainsKey(ext)) s_iconByExt[ext] = icon;
            }
            return icon;
        }

        private static SKBitmap GetShellIconBitmapSkia(string pathOrExt, uint attrs, int w, int h)
        {
            SHFILEINFO sh = new();
            uint flags = SHGFI_ICON | SHGFI_LARGEICON;
            if (attrs != 0) flags |= SHGFI_USEFILEATTRIBUTES;

            SHGetFileInfo(pathOrExt, attrs, ref sh, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
            if (sh.hIcon != IntPtr.Zero)
            {
                try
                {
                    using var ico = Icon.FromHandle(sh.hIcon);
                    using var bmp = ico.ToBitmap();
                    using var scaled = new Bitmap(bmp, new Size(w, h));
                    using var ms = new MemoryStream();
                    scaled.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    return SKBitmap.Decode(ms);
                }
                finally { DestroyIcon(sh.hIcon); }
            }
            return null;
        }

        private static SKBitmap CreateColorSquare(SKColor c)
        {
            var bmp = new SKBitmap(32, 32, true);
            using var cnv = new SKCanvas(bmp);
            cnv.Clear(c);
            return bmp;
        }
        #endregion

        #region 常用工具
        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
            double v = bytes; int i = 0;
            while (v >= 1024 && i < units.Length - 1) { v /= 1024; i++; }
            return $"{v:0.##} {units[i]}";
        }
        #endregion
    }
}
