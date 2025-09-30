using FFmpeg.AutoGen;
using MpvNet.Windows.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MpvNet.Windows
{
    public partial class HHZMainPage_SC : UserControl
    {
        private PictureBox _logoPicLeft;
        private PictureBox _logoPicRight;
        private Label _hintLabelLeft;
        private Label _hintLabelRight;

        // 单控件：内含磁盘列表 + 文件列表（单套/双套）
        private DiskFileList _diskFileList;

        private Bitmap bg;
        public int vWidth;
        public int vHeight;

        public delegate void FileOpenedEventHandler(HHZMainPage_SC sender, string path);
        public event FileOpenedEventHandler FileOpened;

        public event EventHandler<string[]>? FileDropped;

        public HHZMainPage_SC()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            BackColor = Color.Black;
            Dock = DockStyle.Fill;

            // 背景图
            string bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background1.jpg");
            if (File.Exists(bgPath)) bg = new Bitmap(bgPath);

            // UI
            _logoPicLeft = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 128,
                Height = 128,
                Image = LoadMyLogo(),
                BackColor = Color.Transparent,
                Visible = true
            };
            _logoPicRight = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 128,
                Height = 128,
                Image = LoadMyLogo(),
                BackColor = Color.Transparent,
                Visible = false
            };
            _hintLabelLeft = new Label
            {
                AutoSize = true,
                Text = "欢迎使用 HHZPlayer 3D播放器",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12f, FontStyle.Regular)
            };
            _hintLabelRight = new Label
            {
                AutoSize = true,
                Text = "欢迎使用 HHZPlayer 3D播放器",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12f, FontStyle.Regular)
            };

            Controls.Add(_logoPicLeft);
            Controls.Add(_logoPicRight);
            Controls.Add(_hintLabelLeft);
            Controls.Add(_hintLabelRight);

            // 单控件（磁盘 + 文件）
            _diskFileList = new DiskFileList
            {
                Dock = DockStyle.Fill,
                Enable3DMode = App.Settings.Enable3DMode // 外部也可随时切换
            };
            _diskFileList.FileOpened += (_, path) => FileOpened?.Invoke(this, path);
            Controls.Add(_diskFileList);

            // 布局
            Resize += (_, __) => UpdateLogoPosition();
            UpdateLogoPosition();

            // 拖入
            AllowDrop = true;
            DragEnter += HHZMainPage_DragEnter;
            DragDrop += HHZMainPage_DragDrop;

            // 示例：可根据需要切换 3D
            //_diskFileList.Enable3DMode = true;
        }

        public void LoadFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return;

            _diskFileList.NavigateTo(folder);

            App.Settings.LastOpenedFolder = folder;
            App.Settings.Save();
        }

        private void HHZMainPage_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = e.Data!.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void HHZMainPage_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                FileDropped?.Invoke(this, files);
            }
        }

        private void UpdateLogoPosition()
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w == 0 || h == 0) return;

            int logoSize = Math.Max(24, Math.Min(64, h / 20));
            _logoPicLeft.Size = new Size(logoSize, logoSize);
            _logoPicRight.Size = new Size(logoSize, logoSize);

            float fontSize = Math.Max(8, h / 120f);
            var font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            _hintLabelLeft.Font = font;
            _hintLabelRight.Font = font;

            int margin = 10, shift = 3, offsetX = 50;

            if (App.Settings.Enable3DMode)
            {
                int halfWidth = w / 2;

                _logoPicLeft.Location = new Point(offsetX + margin + shift, margin);
                _logoPicRight.Location = new Point(halfWidth + offsetX + margin - shift, margin);

                _hintLabelLeft.Location = new Point(_logoPicLeft.Right + 5 + shift,
                    _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2);
                int textOffsetX = _hintLabelLeft.Left - (offsetX + margin);
                _hintLabelRight.Location = new Point(halfWidth + offsetX + margin + textOffsetX - shift,
                    _logoPicRight.Top + (_logoPicRight.Height - _hintLabelRight.Height) / 2);

                _logoPicLeft.Visible = _logoPicRight.Visible = true;
                _hintLabelLeft.Visible = _hintLabelRight.Visible = true;
            }
            else
            {
                _logoPicLeft.Location = new Point(offsetX + margin, margin);
                _hintLabelLeft.Location = new Point(_logoPicLeft.Right + 5,
                    _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2);

                _logoPicLeft.Visible = _hintLabelLeft.Visible = true;
                _logoPicRight.Visible = _hintLabelRight.Visible = false;
            }
        }

        private Image LoadMyLogo()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylogo.png");
            if (File.Exists(path)) return new Bitmap(path);
            var bmp = new Bitmap(1, 1);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.Transparent);
            return bmp;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            if (bg == null)
            {
                g.Clear(Color.Black);
                return;
            }

            int w = ClientSize.Width, h = ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            if (App.Settings.Enable3DMode)
            {
                g.DrawImage(bg, new Rectangle(0, 0, w / 2, h));
                g.DrawImage(bg, new Rectangle(w / 2, 0, w - w / 2, h));
            }
            else
            {
                g.DrawImage(bg, new Rectangle(0, 0, w, h));
            }
        }

        public unsafe static (int Width, int Height) GetVideoSizeFFmpeg(string filePath)
        {
            ffmpeg.avformat_network_init();

            AVFormatContext* pFormatContext = null;
            if (ffmpeg.avformat_open_input(&pFormatContext, filePath, null, null) != 0)
                return (0, 0);

            if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
            {
                ffmpeg.avformat_close_input(&pFormatContext);
                return (0, 0);
            }

            for (int i = 0; i < pFormatContext->nb_streams; i++)
            {
                AVStream* stream = pFormatContext->streams[i];
                AVCodecParameters* codecpar = stream->codecpar;
                if (codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    int w = codecpar->width;
                    int h = codecpar->height;
                    ffmpeg.avformat_close_input(&pFormatContext);
                    return (w, h);
                }
            }

            ffmpeg.avformat_close_input(&pFormatContext);
            return (0, 0);
        }

        // ============================================================
        // ================== 合并控件：磁盘+文件 ======================
        // ============================================================
        private sealed class DiskFileList : Control
        {
            // —— 对外属性 —— 
            public bool Enable3DMode
            {
                get => _enable3DMode;
                set { if (_enable3DMode != value) { _enable3DMode = value; Invalidate(); } }
            }
            private bool _enable3DMode = false;

            // 行高/间距
            public int DiskRowHeight { get => _diskRowH; set { _diskRowH = Math.Max(48, value); RecalcContent(); Invalidate(); } }
            public int DiskRowSpacing { get => _diskRowGap; set { _diskRowGap = Math.Max(0, value); RecalcContent(); Invalidate(); } }
            public int FileRowHeight { get => _fileRowH; set { _fileRowH = Math.Max(24, value); Invalidate(); } }

            public bool ShowNotReady { get; set; } = true;

            // 事件
            public event EventHandler<string>? FileOpened;
            public event EventHandler<string>? DirectoryChanged;

            // 导航
            public void NavigateTo(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                if (!Directory.Exists(path)) return;

                BuildFiles(path, raiseEvent: false);
                _fileScrollY = 0;
                Invalidate();
            }

            // —— 常量（还原“蓝色框”相对位置）——
            private const float FRAME_LEFT = 0.0255f;
            private const float FRAME_TOP = 0.095f;
            private const float FRAME_WIDTH = 0.253f;
            private const float FRAME_HEIGHT = 0.400f;
            private float _fileListBottomGapRatio = 0.04f; // 文件区底边距占父高比例

            // —— 内部状态 —— 
            private readonly List<DriveItem> _drives = new();
            private readonly List<FileItem> _items = new();
            private string _currentPath = string.Empty;

            // 共享选择/悬停（左右同步）
            private int _diskHot = -1, _diskSel = -1;
            private int _fileHot = -1, _fileSel = -1;

            // 滚动（左右同步）
            private int _diskScrollY = 0;
            private int _fileScrollY = 0;

            // 内容高度
            private int _diskContentH = 0;

            // 鼠标拖拽滚动
            private bool _dragging = false;
            private int _dragStartY = 0;
            private int _dragStartScrollY = 0;
            private AreaKind _dragArea = AreaKind.None;

            // 点击判定（避免拖拽误触发）
            private Point _mouseDownPos;
            private int _mouseDownIndex = -1;
            private AreaKind _mouseDownArea = AreaKind.None;
            private const int ClickMoveTolerance = 5;

            // —— 视觉参数 —— 
            private int _diskRowH = 68;
            private int _diskRowGap = 20;
            private int _fileRowH = 40;

            // 颜色刷
            private readonly SolidBrush _listBgBrush = new(Color.FromArgb(70, 50, 50, 50));
            private readonly SolidBrush _hoverBrush = new(Color.FromArgb(30, 120, 170, 255));
            private readonly SolidBrush _selBrush = new(Color.FromArgb(50, 120, 170, 255));
            private readonly Pen _framePen = new(Color.FromArgb(180, 80, 140, 255), 2f);

            private static Font MakeFont(string family, float size, FontStyle style, string fallbackFamily, FontStyle fallbackStyle)
            {
                try { return new Font(family, size, style, GraphicsUnit.Point); }
                catch { return new Font(fallbackFamily, size, fallbackStyle, GraphicsUnit.Point); }
            }

            // 预生成字体
            private readonly Font _titleFont = MakeFont("Segoe UI Semibold", 12f, FontStyle.Bold, "Segoe UI", FontStyle.Bold);
            private readonly Font _subFont = new("Segoe UI", 9.5f, FontStyle.Regular);
            private readonly Font _fileFont = new("Segoe UI", 10f, FontStyle.Regular);
            private readonly Font _headerFont = new("Segoe UI", 10f, FontStyle.Bold);

            // 列定义（文件列表）
            private readonly string[] _fileColumns = { "名称", "大小", "类型", "修改日期" };
            private float[] _fileColWidths = { 0.50f, 0.15f, 0.15f, 0.20f };

            // 布局矩形
            private Rectangle _leftHost, _rightHost;
            private Rectangle _diskRectL, _diskRectR;
            private Rectangle _fileRectL, _fileRectR;

            public DiskFileList()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                UpdateStyles();
                BackColor = Color.Transparent;

                LoadDrives();
                // 初始无选择
                _diskSel = _drives.Count > 0 ? 0 : -1;
                if (_diskSel >= 0) BuildFiles(_drives[_diskSel].Root, raiseEvent: false);

                // 输入
                MouseDown += OnMouseDown;
                MouseMove += OnMouseMove;
                MouseUp += OnMouseUp;
                MouseWheel += OnMouseWheel;
                MouseLeave += (_, __) => { _diskHot = -1; _fileHot = -1; Invalidate(); };

                Resize += (_, __) => { RecalcContent(); Invalidate(); };
            }

            // 真透明背景（复制父背景）
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (Parent == null) return;
                var g = e.Graphics;
                using (var state = new Region(new Rectangle(0, 0, Width, Height)))
                {
                    var rectInParent = new Rectangle(Left, Top, Width, Height);
                    var gs = g.Save();
                    g.TranslateTransform(-rectInParent.X, -rectInParent.Y);
                    using var pe = new PaintEventArgs(g, rectInParent);
                    this.InvokePaintBackground(Parent, pe);
                    this.InvokePaint(Parent, pe);
                    g.Restore(gs);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 布局
                UpdateRects();

                // 两侧/单侧
                if (Enable3DMode)
                {
                    DrawPane(g, _leftHost, _diskRectL, _fileRectL);
                    DrawPane(g, _rightHost, _diskRectR, _fileRectR);
                }
                else
                {
                    DrawPane(g, new Rectangle(0, 0, Width, Height), _diskRectL, _fileRectL);
                }
            }

            private void DrawPane(Graphics g, Rectangle host, Rectangle diskRect, Rectangle fileRect)
            {
                // 框线
                using (var path = RoundedRect(host, 10))
                using (var pen = new Pen(Color.FromArgb(80, Color.Black), 1f))
                    g.DrawPath(pen, path);

                // 磁盘列表背景 + 边框
                using (var bg = new SolidBrush(Color.FromArgb(64, 80, 140, 255)))
                using (var path = RoundedRect(diskRect, 10))
                    g.FillPath(bg, path);
                g.DrawRectangle(_framePen, InflateR(diskRect, -1, -1));

                // 文件列表背景 + 边框
                using (var bg2 = new SolidBrush(Color.FromArgb(50, 30, 30, 30)))
                    g.FillRectangle(bg2, fileRect);
                g.DrawRectangle(_framePen, InflateR(fileRect, -1, -1));

                // 画磁盘列表（只画当前可见行）
                DrawDiskList(g, diskRect);

                // 画文件列表（带表头）
                DrawFileList(g, fileRect);
            }

            private void DrawDiskList(Graphics g, Rectangle rect)
            {
                // 可视裁剪
                Region old = g.Clip;
                using (var clip = new Region(rect))
                {
                    clip.Intersect(g.Clip);
                    g.Clip = clip;

                    // 背景
                    g.FillRectangle(_listBgBrush, rect);

                    // 行范围
                    int y = rect.Y + _diskScrollY;
                    int contentW = rect.Width - 1;

                    // 可见行区间
                    int first = Math.Max(0, (rect.Y - _diskScrollY - rect.Y) / (_diskRowH + _diskRowGap));
                    int last = _drives.Count - 1;

                    using var br1 = new SolidBrush(Color.White);
                    using var br2 = new SolidBrush(Color.FromArgb(215, 220, 230));
                    using var barBg = new SolidBrush(Color.FromArgb(80, 110, 120, 140));
                    using var barFg = new SolidBrush(Color.FromArgb(200, 120, 180, 255));

                    for (int i = 0; i < _drives.Count; i++)
                    {
                        var it = _drives[i];
                        var rowRect = new Rectangle(rect.X, y, contentW, _diskRowH);

                        if (rowRect.Bottom >= rect.Top && rowRect.Top <= rect.Bottom)
                        {
                            // 悬停/选中
                            if (i == _diskSel) g.FillRectangle(_selBrush, rowRect);
                            else if (i == _diskHot) g.FillRectangle(_hoverBrush, rowRect);

                            // 图标
                            int icoSize = 48;
                            var icoRect = new Rectangle(rowRect.X + 12, rowRect.Y + (_diskRowH - icoSize) / 2, icoSize, icoSize);
                            var icon = GetDriveIcon(it.Root);
                            if (icon != null) g.DrawIcon(icon, icoRect);

                            int textX = icoRect.Right + 10;
                            int textW = rowRect.Right - 12 - textX;

                            var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                            float line1Top = rowRect.Y + 6;
                            g.DrawString(it.Line1, _titleFont, br1, new RectangleF(textX, line1Top, textW, _titleFont.Height), fmt);

                            float line2Top = line1Top + _titleFont.Height + 2;
                            g.DrawString(it.Line2, _subFont, br2, new RectangleF(textX, line2Top, textW, _subFont.Height), fmt);

                            int barLeft = textX;
                            int barRight = rowRect.Right - 12;
                            int barY = rowRect.Bottom - 16;
                            g.FillRectangle(barBg, barLeft, barY, barRight - barLeft, 3);
                            g.FillRectangle(barFg, barLeft, barY, (int)((barRight - barLeft) * it.Usage01), 3);
                        }

                        y += _diskRowH + _diskRowGap;
                    }
                }

                g.Clip = old;
            }

            private void DrawFileList(Graphics g, Rectangle rect)
            {
                // 表头
                var headerRect = new Rectangle(rect.X, rect.Y, rect.Width, _fileRowH);
                using (var headBg = new SolidBrush(Color.FromArgb(100, 0, 122, 204)))
                    g.FillRectangle(headBg, headerRect);
                int colX = rect.X;
                int colW0 = (int)(rect.Width * _fileColWidths[0]);
                int colW1 = (int)(rect.Width * _fileColWidths[1]);
                int colW2 = (int)(rect.Width * _fileColWidths[2]);
                int colW3 = (int)(rect.Width * _fileColWidths[3]);
                int[] colW = { colW0, colW1, colW2, colW3 };
                for (int i = 0; i < 4; i++)
                {
                    TextRenderer.DrawText(g, _fileColumns[i], _headerFont,
                        new Rectangle(colX + 8, headerRect.Y, colW[i] - 16, headerRect.Height),
                        Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    colX += colW[i];
                }

                // 可视裁剪（表头以下）
                var listRect = new Rectangle(rect.X, rect.Y + _fileRowH, rect.Width, rect.Height - _fileRowH);
                Region old = g.Clip;
                using (var clip = new Region(listRect))
                {
                    clip.Intersect(g.Clip);
                    g.Clip = clip;

                    g.FillRectangle(_listBgBrush, listRect);

                    // 行范围（只画可见）
                    int itemsHeight = _items.Count * _fileRowH;
                    int viewportH = listRect.Height;
                    int first = Math.Max(0, _fileScrollY / _fileRowH);
                    int last = Math.Min(_items.Count - 1, (_fileScrollY + viewportH) / _fileRowH);

                    for (int i = first; i <= last; i++)
                    {
                        var it = _items[i];
                        int drawY = listRect.Y + i * _fileRowH - _fileScrollY;

                        var rowRect = new Rectangle(listRect.X, drawY, listRect.Width, _fileRowH);
                        if ((i & 1) == 1)
                            using (var alt = new SolidBrush(Color.FromArgb(18, Color.White)))
                                g.FillRectangle(alt, rowRect);

                        if (i == _fileHot) g.FillRectangle(_hoverBrush, rowRect);
                        if (i == _fileSel) g.FillRectangle(_selBrush, rowRect);

                        int x0 = listRect.X;
                        int x1 = x0 + colW0;
                        int x2 = x1 + colW1;
                        int x3 = x2 + colW2;

                        // 图标 + 名称
                        if (it.Icon != null)
                        {
                            int iconSize = 32;
                            var icoRect = new Rectangle(x0 + 8, drawY + (_fileRowH - iconSize) / 2, iconSize, iconSize);
                            g.DrawImage(it.Icon, icoRect);
                        }
                        int nameLeft = x0 + 8 + 32 + 8;
                        TextRenderer.DrawText(g, it.Name, _fileFont,
                            new Rectangle(nameLeft, drawY, colW0 - (nameLeft - x0) - 8, _fileRowH),
                            Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                        // 大小
                        string sizeText = it.Size < 0 ? "" : FormatSize(it.Size);
                        TextRenderer.DrawText(g, sizeText, _fileFont,
                            new Rectangle(x1 + 6, drawY, colW1 - 12, _fileRowH),
                            Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                        // 类型
                        TextRenderer.DrawText(g, it.Type, _fileFont,
                            new Rectangle(x2 + 6, drawY, colW2 - 12, _fileRowH),
                            Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                        // 修改日期
                        TextRenderer.DrawText(g, it.Modified.ToString("yyyy-MM-dd HH:mm"), _fileFont,
                            new Rectangle(x3 + 6, drawY, colW3 - 12, _fileRowH),
                            Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    }
                }
                g.Clip = old;
            }

            // ===================== 输入与命中 =====================
            private enum AreaKind { None, DiskLeft, DiskRight, FileLeft, FileRight }

            private void OnMouseDown(object? sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;

                var area = HitArea(e.Location);
                _dragging = true;
                _dragStartY = e.Y;
                _dragStartScrollY = (area == AreaKind.DiskLeft || area == AreaKind.DiskRight) ? _diskScrollY : _fileScrollY;
                _dragArea = area;

                _mouseDownPos = e.Location;
                _mouseDownArea = area;
                _mouseDownIndex = HitIndex(area, e.Location);
                Capture = true;
            }

            private void OnMouseMove(object? sender, MouseEventArgs e)
            {
                if (_dragging)
                {
                    int dy = e.Y - _dragStartY;
                    if (_dragArea == AreaKind.DiskLeft || _dragArea == AreaKind.DiskRight)
                    {
                        _diskScrollY = _dragStartScrollY + dy;
                        ClampDiskScroll();
                        Invalidate();
                    }
                    else if (_dragArea == AreaKind.FileLeft || _dragArea == AreaKind.FileRight)
                    {
                        _fileScrollY = Math.Max(0, _dragStartScrollY - dy);
                        Invalidate();
                    }
                    return;
                }

                var area = HitArea(e.Location);
                int idx = HitIndex(area, e.Location);

                if (area == AreaKind.DiskLeft || area == AreaKind.DiskRight)
                {
                    if (_diskHot != idx) { _diskHot = idx; Invalidate(); }
                }
                else if (area == AreaKind.FileLeft || area == AreaKind.FileRight)
                {
                    if (_fileHot != idx) { _fileHot = idx; Invalidate(); }
                }
                else
                {
                    if (_diskHot != -1 || _fileHot != -1)
                    {
                        _diskHot = _fileHot = -1; Invalidate();
                    }
                }
            }

            private void OnMouseUp(object? sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;

                _dragging = false;
                Capture = false;

                var upArea = HitArea(e.Location);
                int upIndex = HitIndex(upArea, e.Location);

                bool isClick = (_mouseDownArea == upArea) &&
                               (_mouseDownIndex == upIndex) &&
                               Math.Abs(e.X - _mouseDownPos.X) <= ClickMoveTolerance &&
                               Math.Abs(e.Y - _mouseDownPos.Y) <= ClickMoveTolerance;

                if (isClick && upIndex >= 0)
                {
                    if (upArea == AreaKind.DiskLeft || upArea == AreaKind.DiskRight)
                    {
                        if (upIndex < _drives.Count)
                        {
                            _diskSel = upIndex;
                            var root = _drives[upIndex].Root;
                            BuildFiles(root, raiseEvent: false);
                            _fileScrollY = 0;
                            Invalidate();
                        }
                    }
                    else if (upArea == AreaKind.FileLeft || upArea == AreaKind.FileRight)
                    {
                        if (upIndex < _items.Count)
                        {
                            _fileSel = upIndex;
                            var it = _items[upIndex];
                            if (it.IsDir)
                            {
                                BuildFiles(it.FullPath, raiseEvent: true);
                                _fileScrollY = 0;
                            }
                            else
                            {
                                FileOpened?.Invoke(this, it.FullPath);
                            }
                            Invalidate();
                        }
                    }
                }

                _mouseDownArea = AreaKind.None;
                _mouseDownIndex = -1;
            }

            private void OnMouseWheel(object? sender, MouseEventArgs e)
            {
                var area = HitArea(e.Location);
                int delta = e.Delta; // 120/格

                if (area == AreaKind.DiskLeft || area == AreaKind.DiskRight)
                {
                    // 以行步进
                    int stride = _diskRowH + _diskRowGap;
                    _diskScrollY += Math.Sign(delta) * 3 * stride;
                    ClampDiskScroll();
                    Invalidate();
                }
                else if (area == AreaKind.FileLeft || area == AreaKind.FileRight)
                {
                    _fileScrollY = Math.Max(0, _fileScrollY - delta);
                    Invalidate();
                }
            }

            private AreaKind HitArea(Point pt)
            {
                if (Enable3DMode)
                {
                    if (_diskRectL.Contains(pt)) return AreaKind.DiskLeft;
                    if (_fileRectL.Contains(pt)) return AreaKind.FileLeft;
                    if (_diskRectR.Contains(pt)) return AreaKind.DiskRight;
                    if (_fileRectR.Contains(pt)) return AreaKind.FileRight;
                }
                else
                {
                    if (_diskRectL.Contains(pt)) return AreaKind.DiskLeft;
                    if (_fileRectL.Contains(pt)) return AreaKind.FileLeft;
                }
                return AreaKind.None;
            }

            private int HitIndex(AreaKind area, Point pt)
            {
                if (area == AreaKind.DiskLeft || area == AreaKind.DiskRight)
                {
                    var rect = (area == AreaKind.DiskLeft || !Enable3DMode) ? _diskRectL : _diskRectR;
                    int y = pt.Y - rect.Y - _diskScrollY;
                    if (y < 0) return -1;
                    int stride = _diskRowH + _diskRowGap;
                    int idx = y / stride;
                    if (idx >= 0 && idx < _drives.Count && (y - idx * stride) < _diskRowH) return idx;
                    return -1;
                }
                else if (area == AreaKind.FileLeft || area == AreaKind.FileRight)
                {
                    var rect = (area == AreaKind.FileLeft || !Enable3DMode) ? _fileRectL : _fileRectR;
                    int y = pt.Y - (rect.Y + _fileRowH) + _fileScrollY; // 表头以下
                    if (y < 0) return -1;
                    int idx = y / _fileRowH;
                    return (idx >= 0 && idx < _items.Count) ? idx : -1;
                }
                return -1;
            }

            // ===================== 数据构建 =====================
            private void LoadDrives()
            {
                _drives.Clear();
                foreach (var di in DriveInfo.GetDrives())
                {
                    if (di.DriveType == DriveType.CDRom) continue;
                    if (!ShowNotReady && !SafeIsReady(di)) continue;
                    _drives.Add(BuildDriveItem(di));
                }
                RecalcContent();
            }

            private DriveItem BuildDriveItem(DriveInfo di)
            {
                var it = new DriveItem { Root = di.RootDirectory.FullName };
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
                it.Line2 = (total > 0) ? $"{FormatSize(free)} 可用, 共 {FormatSize(total)}" : "不可用";
                GetDriveIcon(it.Root); // 预热图标
                return it;
            }

            private void BuildFiles(string path, bool raiseEvent)
            {
                _items.Clear();

                try
                {
                    var diRoot = new DirectoryInfo(path);

                    // 顶部两个“..”
                    if (diRoot.Parent != null)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            _items.Add(new FileItem
                            {
                                Name = "..",
                                FullPath = diRoot.Parent.FullName,
                                IsDir = true,
                                Size = -1,
                                Type = "上一级目录",
                                Modified = DateTime.Now,
                                Icon = GetFolderIconBitmap()
                            });
                        }
                    }

                    // 目录
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
                            Icon = GetFolderIconBitmap()
                        });
                    }

                    // 文件（按扩展缓存图标）
                    var extSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var f in Directory.GetFiles(path))
                    {
                        string ext = Path.GetExtension(f);
                        if (string.IsNullOrEmpty(ext)) ext = ".";
                        extSet.Add(ext);
                    }
                    foreach (var ext in extSet) EnsureIconCached(ext);

                    foreach (var file in Directory.GetFiles(path))
                    {
                        var fi = new FileInfo(file);
                        string ext = fi.Extension;
                        if (string.IsNullOrEmpty(ext)) ext = ".";
                        var icon = GetIconFromCache(ext) ?? GetDefaultFileIconBitmap();

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

                    _currentPath = path;
                    if (raiseEvent) DirectoryChanged?.Invoke(this, _currentPath);
                }
                catch { /* 忽略不可读目录 */ }
            }

            // ===================== 布局计算 =====================
            private void UpdateRects()
            {
                int w = ClientSize.Width, h = ClientSize.Height;
                if (w <= 0 || h <= 0)
                {
                    _leftHost = _rightHost = _diskRectL = _diskRectR = _fileRectL = _fileRectR = Rectangle.Empty;
                    return;
                }

                int bottomGap = (int)Math.Round(h * _fileListBottomGapRatio);

                if (Enable3DMode)
                {
                    _leftHost = new Rectangle(0, 0, w / 2, h);
                    _rightHost = new Rectangle(w / 2, 0, w - w / 2, h);

                    _diskRectL = CalcBlueFrame(_leftHost);
                    _diskRectR = CalcBlueFrame(_rightHost);

                    _fileRectL = new Rectangle(
                        _diskRectL.Right + 20,
                        _diskRectL.Top,
                        _leftHost.Right - _diskRectL.Right - 40,
                        h - _diskRectL.Top - bottomGap
                    );
                    _fileRectR = new Rectangle(
                        _diskRectR.Right + 20,
                        _diskRectR.Top,
                        _rightHost.Right - _diskRectR.Right - 40,
                        h - _diskRectR.Top - bottomGap
                    );
                }
                else
                {
                    _leftHost = new Rectangle(0, 0, w, h);
                    _rightHost = Rectangle.Empty;

                    _diskRectL = CalcBlueFrame(_leftHost);
                    _diskRectR = Rectangle.Empty;

                    _fileRectL = new Rectangle(
                        _diskRectL.Right + 20,
                        _diskRectL.Top,
                        _leftHost.Right - _diskRectL.Right - 100,
                        h - _diskRectL.Top - bottomGap
                    );
                    _fileRectR = Rectangle.Empty;
                }
            }

            private Rectangle CalcBlueFrame(Rectangle host)
            {
                int fx = host.X + (int)(host.Width * FRAME_LEFT);
                int fy = host.Y + (int)(host.Height * FRAME_TOP);
                int fw = (int)(host.Width * FRAME_WIDTH);
                int fh = (int)(host.Height * FRAME_HEIGHT);
                const int pad = 10; // 内边距
                return new Rectangle(fx + pad, fy + pad,
                                     Math.Max(10, fw - pad * 2),
                                     Math.Max(10, fh - pad * 2));
            }

            private static Rectangle InflateR(Rectangle r, int dx, int dy)
            {
                r.Inflate(dx, dy); return r;
            }

            private void RecalcContent()
            {
                _diskContentH = _drives.Count == 0 ? 0 : _drives.Count * (_diskRowH + _diskRowGap) - _diskRowGap;
                ClampDiskScroll();
            }

            private void ClampDiskScroll()
            {
                // 磁盘列表区域（左/右）高度可能不同，以左为准（左右同步滚动）
                var rect = Enable3DMode ? _diskRectL : _diskRectL;
                int viewportH = Math.Max(0, rect.Height);
                int min = Math.Min(0, viewportH - _diskContentH);
                _diskScrollY = (_diskContentH <= viewportH) ? 0 : Math.Max(min, Math.Min(0, _diskScrollY));
            }

            // ===================== 工具/缓存 =====================
            private static bool SafeIsReady(DriveInfo d) { try { return d.IsReady; } catch { return false; } }

            private static string FormatSize(long bytes)
            {
                string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
                double v = bytes; int i = 0; while (v >= 1024 && i < units.Length - 1) { v /= 1024; i++; }
                return $"{v:0.##} {units[i]}";
            }

            private static GraphicsPath RoundedRect(Rectangle r, int radius)
            {
                int d = radius * 2;
                var path = new GraphicsPath();
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }

            // —— 驱动器图标缓存（按根路径）——
            private readonly Dictionary<string, Icon> _driveIconCache = new(StringComparer.OrdinalIgnoreCase);
            private Icon? GetDriveIcon(string driveRoot)
            {
                if (string.IsNullOrEmpty(driveRoot)) return null;
                if (_driveIconCache.TryGetValue(driveRoot, out var cached)) return cached;

                SHFILEINFO sh = new();
                // 直接用 driveRoot + 正确的 flags
                IntPtr h = SHGetFileInfo(driveRoot, 0, ref sh, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_ICON | SHGFI_LARGEICON);

                if (sh.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        var icon = (Icon)Icon.FromHandle(sh.hIcon).Clone();
                        _driveIconCache[driveRoot] = icon;
                        return icon;
                    }
                    finally { DestroyIcon(sh.hIcon); }
                }

                _driveIconCache[driveRoot] = SystemIcons.WinLogo;
                return _driveIconCache[driveRoot];
            }

            // —— 文件图标缓存（静态：按扩展）——
            private static readonly object s_iconLock = new();
            private static readonly Dictionary<string, Bitmap> s_iconByExt = new(StringComparer.OrdinalIgnoreCase);
            private static Bitmap? s_folderIcon;
            private static Bitmap? s_defaultFileIcon;

            private static Bitmap GetFolderIconBitmap()
            {
                EnsureGlobalIcons();
                return s_folderIcon!;
            }
            private static Bitmap GetDefaultFileIconBitmap()
            {
                EnsureGlobalIcons();
                return s_defaultFileIcon!;
            }

            private static void EnsureGlobalIcons()
            {
                if (s_folderIcon != null && s_defaultFileIcon != null) return;
                lock (s_iconLock)
                {
                    if (s_folderIcon == null)
                    {
                        var bmp = GetShellIconBitmap("dummy", FILE_ATTRIBUTE_DIRECTORY);
                        s_folderIcon = bmp ?? ColorSquare(new Size(32, 32), Color.SteelBlue);
                    }
                    if (s_defaultFileIcon == null)
                    {
                        var bmp = GetShellIconBitmap("dummy.", FILE_ATTRIBUTE_NORMAL);
                        s_defaultFileIcon = bmp ?? ColorSquare(new Size(32, 32), Color.Gray);
                    }
                }
            }

            private static Bitmap? GetIconFromCache(string ext)
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

                var bmp = GetShellIconBitmap("dummy" + ext, FILE_ATTRIBUTE_NORMAL) ?? GetDefaultFileIconBitmap();

                lock (s_iconLock)
                {
                    if (!s_iconByExt.ContainsKey(ext)) s_iconByExt[ext] = bmp;
                }
            }

            // Shell 取 32×32 Bitmap
            private static Bitmap? GetShellIconBitmap(string pathOrExt, uint attrs)
            {
                SHFILEINFO sh = new();
                uint flags = SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES;
                IntPtr h = SHGetFileInfo(pathOrExt, attrs, ref sh, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
                if (sh.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        using var ico = Icon.FromHandle(sh.hIcon);
                        using var src = ico.ToBitmap();
                        return new Bitmap(src, new Size(32, 32));
                    }
                    finally
                    {
                        DestroyIcon(sh.hIcon);
                    }
                }
                return null;
            }

            private static Bitmap ColorSquare(Size s, Color c)
            {
                var bmp = new Bitmap(s.Width, s.Height, PixelFormat.Format32bppPArgb);
                using var g = Graphics.FromImage(bmp);
                g.Clear(c);
                return bmp;
            }

            // —— P/Invoke —— 
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

            // —— 数据结构 —— 
            private sealed class DriveItem
            {
                public string Root = "";
                public string Line1 = "";
                public string Line2 = "";
                public float Usage01 = 0f;
            }

            private sealed class FileItem
            {
                public string Name = "";
                public string FullPath = "";
                public bool IsDir = false;
                public long Size = 0;
                public string Type = "";
                public DateTime Modified;
                public Image? Icon;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (var kv in _driveIconCache) kv.Value?.Dispose();
                    _driveIconCache.Clear();
                }
                base.Dispose(disposing);
            }
        }
    }
}
