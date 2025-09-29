using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MpvNet.Windows
{
    public partial class HHZMainPage : UserControl
    {
        private PictureBox _logoPicLeft;
        private PictureBox _logoPicRight;
        private Label _hintLabelLeft;
        private Label _hintLabelRight;

        private DiskList _diskListLeft;
        private DiskList _diskListRight;

        private FileList _fileListLeft;
        private FileList _fileListRight;

        // 防止事件互相递归触发
        private bool _syncingDisk = false;        // 磁盘 列表的联动
        private bool _syncingDiskHover = false;

        private bool _syncingFileDir = false;     // 文件 列表目录联动
        private bool _syncingLeftFileHover = false;   // 文件 列表悬停联动
        private bool _syncingLeftFileScroll = false;  // 文件 列表滚动联动
        private bool _syncingRightFileHover = false;   // 文件 列表悬停联动
        private bool _syncingRightFileScroll = false;  // 文件 列表滚动联动

        // 蓝色框比例
        private const float FRAME_LEFT = 0.0255f;
        private const float FRAME_TOP = 0.095f;
        private const float FRAME_WIDTH = 0.253f;
        private const float FRAME_HEIGHT = 0.400f;

        private Bitmap bg;
        private float _fileListBottomGapRatio = 0.07f;

        public HHZMainPage()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
            this.BackColor = Color.Black;
            this.Dock = DockStyle.Fill;

            // 背景图
            string bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background1.jpg");
            if (File.Exists(bgPath)) bg = new Bitmap(bgPath);

            // UI
            _logoPicLeft = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width = 128, Height = 128, Image = LoadMyLogo(), BackColor = Color.Transparent, Visible = true };
            _logoPicRight = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width = 128, Height = 128, Image = LoadMyLogo(), BackColor = Color.Transparent, Visible = false };
            _hintLabelLeft = new Label { AutoSize = true, Text = "欢迎使用 HHZPlayer 3D播放器", ForeColor = Color.White, BackColor = Color.Transparent, Font = new Font("Segoe UI", 12f, FontStyle.Regular) };
            _hintLabelRight = new Label { AutoSize = true, Text = "欢迎使用 HHZPlayer 3D播放器", ForeColor = Color.White, BackColor = Color.Transparent, Font = new Font("Segoe UI", 12f, FontStyle.Regular) };

            Controls.Add(_logoPicLeft);
            Controls.Add(_logoPicRight);
            Controls.Add(_hintLabelLeft);
            Controls.Add(_hintLabelRight);

            // 磁盘列表
            _diskListLeft = new DiskList { RowHeight = 110, RowSpacing = 5, ShowNotReady = true };
            _diskListRight = new DiskList { RowHeight = 110, RowSpacing = 5, ShowNotReady = true };
            Controls.Add(_diskListLeft);
            Controls.Add(_diskListRight);

            // 磁盘滚动联动
            _diskListLeft.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncingDisk) return;
                try { _syncingDisk = true; _diskListRight.ScrollOffset = e.OffsetY; }
                finally { _syncingDisk = false; }
            };
            _diskListRight.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncingDisk) return;
                try { _syncingDisk = true; _diskListLeft.ScrollOffset = e.OffsetY; }
                finally { _syncingDisk = false; }
            };
            // 磁盘选中联动
            _diskListLeft.SelectionChanged += (_, e) =>
            {
                if (_syncingDisk) return;
                try { _syncingDisk = true; _diskListRight.SelectIndex(e.Index, ensureVisible: true, raiseEvent: false); }
                finally { _syncingDisk = false; }
            };
            _diskListRight.SelectionChanged += (_, e) =>
            {
                if (_syncingDisk) return;
                try { _syncingDisk = true; _diskListLeft.SelectIndex(e.Index, ensureVisible: true, raiseEvent: false); }
                finally { _syncingDisk = false; }
            };
            // 磁盘悬停联动
            _diskListLeft.HoverChanged += (_, e) =>
            {
                if (_syncingDiskHover) return;
                try { _syncingDiskHover = true; _diskListRight.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingDiskHover = false; }
            };
            _diskListRight.HoverChanged += (_, e) =>
            {
                if (_syncingDiskHover) return;
                try { _syncingDiskHover = true; _diskListLeft.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingDiskHover = false; }
            };

            // 文件列表
            _fileListLeft = new FileList();
            _fileListRight = new FileList();
            Controls.Add(_fileListLeft);
            Controls.Add(_fileListRight);

            // ② DirectoryChanged：统一用同一个闸门
            _fileListLeft.DirectoryChanged += (_, path) =>
            {
                if (_syncingFileDir) return;
                try { _syncingFileDir = true; _fileListRight.NavigateTo(path); App.Settings.LastOpenedFolder = path; }
                finally { _syncingFileDir = false; }
            };

            _fileListRight.DirectoryChanged += (_, path) =>
            {
                if (_syncingFileDir) return;
                try { _syncingFileDir = true; _fileListLeft.NavigateTo(path); }
                finally { _syncingFileDir = false; }
            };


            // —— 文件滚动联动（像素级）——
            _fileListLeft.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncingLeftFileScroll) return;
                try { _syncingLeftFileScroll = true; _fileListRight.ScrollOffset = e.OffsetY; }
                finally { _syncingLeftFileScroll = false; }
            };
            _fileListRight.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncingRightFileScroll) return;
                try { _syncingRightFileScroll = true; _fileListLeft.ScrollOffset = e.OffsetY; }
                finally { _syncingRightFileScroll = false; }
            };

            // —— 文件悬停联动 —— 
            _fileListLeft.HoverChanged += (_, e) =>
            {
                if (_syncingLeftFileHover) return;
                try { _syncingLeftFileHover = true; _fileListRight.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingLeftFileHover = false; }
            };
            _fileListRight.HoverChanged += (_, e) =>
            {
                if (_syncingRightFileHover) return;
                try { _syncingRightFileHover = true; _fileListLeft.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingRightFileHover = false; }
            };

            // ③ DiskSelected：两边同时导航时，也用同一个闸门，避免触发对方的 DirectoryChanged 再回调自己
            _diskListLeft.DiskSelected += (_, root) =>
            {
                if (_syncingFileDir) return;
                try { _syncingFileDir = true; _fileListLeft.NavigateTo(root); _fileListRight.NavigateTo(root); }
                finally { _syncingFileDir = false; }
            };

            _diskListRight.DiskSelected += (_, root) =>
            {
                if (_syncingFileDir) return;
                try { _syncingFileDir = true; _fileListLeft.NavigateTo(root); _fileListRight.NavigateTo(root); }
                finally { _syncingFileDir = false; }
            };


            // 打开文件转发
            _fileListLeft.FileOpened += (_, path) => FileOpened?.Invoke(this, path);
            _fileListRight.FileOpened += (_, path) => FileOpened?.Invoke(this, path);

            // 布局
            this.Resize += (_, __) => { UpdateLogoPosition(); UpdateBoundsLayout(); };
            UpdateLogoPosition();
            UpdateBoundsLayout();

            _diskListLeft.Reload();
            _diskListRight.Reload();

            // 拖入
            this.AllowDrop = true;
            this.DragEnter += HHZMainPage_DragEnter;
            this.DragDrop += HHZMainPage_DragDrop;
        }

        private Rectangle CalcBlueFrame(Rectangle host)
        {
            int fx = host.X + (int)(host.Width * FRAME_LEFT);
            int fy = host.Y + (int)(host.Height * FRAME_TOP);
            int fw = (int)(host.Width * FRAME_WIDTH);
            int fh = (int)(host.Height * FRAME_HEIGHT);
            const int pad = 10;
            return new Rectangle(fx + pad, fy + pad, Math.Max(10, fw - pad * 2), Math.Max(10, fh - pad * 2));
        }

        private void UpdateBoundsLayout()
        {
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            int bottomGap = (int)Math.Round(h * _fileListBottomGapRatio);

            if (App.Settings.Enable3DMode)
            {
                var leftHost = new Rectangle(0/*5*/, 0, w / 2, h);
                var rightHost = new Rectangle(w / 2/* - 5*/, 0, w - w / 2, h);

                _diskListLeft.Bounds = CalcBlueFrame(leftHost);
                _diskListRight.Bounds = CalcBlueFrame(rightHost);

                var leftFileRect = new Rectangle(
                    _diskListLeft.Right + 20/* + 5*/,
                    _diskListLeft.Top,
                    leftHost.Right - _diskListLeft.Right - bottomGap,
                    h - _diskListLeft.Top - bottomGap
                );

                var rightFileRect = new Rectangle(
                    _diskListRight.Right + 20/* - 5*/,
                    _diskListRight.Top,
                    rightHost.Right - _diskListRight.Right - bottomGap,
                    h - _diskListRight.Top - bottomGap
                );

                _fileListLeft.Bounds = leftFileRect;
                _fileListRight.Bounds = rightFileRect;
                //_fileListLeft.Bounds = rightFileRect;
                //_fileListRight.Bounds = leftFileRect;


                _logoPicLeft.Visible = _logoPicRight.Visible = true;
                _hintLabelLeft.Visible = _hintLabelRight.Visible = true;

                _diskListLeft.Visible = true;
                _diskListRight.Visible = true;
                _fileListLeft.Visible = true;
                _fileListRight.Visible = true;
            }
            else
            {
                var fullHost = new Rectangle(0, 0, w, h);
                _diskListLeft.Bounds = CalcBlueFrame(fullHost);

                var fileRect = new Rectangle(
                    _diskListLeft.Right + 20,
                    _diskListLeft.Top,
                    fullHost.Right - _diskListLeft.Right - 100,
                    h - _diskListLeft.Top - bottomGap
                );
                _fileListLeft.Bounds = fileRect;

                _logoPicLeft.Visible = _hintLabelLeft.Visible = true;
                _logoPicRight.Visible = _hintLabelRight.Visible = false;

                _diskListLeft.Visible = _fileListLeft.Visible = true;
                _diskListRight.Visible = _fileListRight.Visible = false;
            }

            _diskListLeft.Invalidate();
            _diskListRight.Invalidate();
            _fileListLeft.Invalidate();
            _fileListRight.Invalidate();
        }

        private void UpdateLogoPosition()
        {
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w == 0 || h == 0) return;

            int logoSize = Math.Max(24, Math.Min(64, h / 20));
            _logoPicLeft.Size = new Size(logoSize, logoSize);
            _logoPicRight.Size = new Size(logoSize, logoSize);

            float fontSize = Math.Max(8, h / 120f);
            var font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            _hintLabelLeft.Font = font; _hintLabelRight.Font = font;

            int margin = 10, shift = 10, offsetX = 50;

            if (App.Settings.Enable3DMode)
            {
                int halfWidth = w / 2;

                _logoPicLeft.Location = new Point(offsetX + margin + shift, margin);
                _logoPicRight.Location = new Point(halfWidth + offsetX + margin - shift, margin);

                _hintLabelLeft.Location = new Point(_logoPicLeft.Right + 5, _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2);
                int textOffsetX = _hintLabelLeft.Left - (offsetX + margin);
                _hintLabelRight.Location = new Point(halfWidth + offsetX + margin + textOffsetX,
                                                     _logoPicRight.Top + (_logoPicRight.Height - _hintLabelRight.Height) / 2);
            }
            else
            {
                _logoPicLeft.Location = new Point(offsetX + margin, margin);
                _hintLabelLeft.Location = new Point(_logoPicLeft.Right + 5, _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2);
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

            int w = this.ClientSize.Width, h = this.ClientSize.Height;
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

        // 拖入
        public event EventHandler<string[]>? FileDropped;
        private void HHZMainPage_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data!.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }
        private void HHZMainPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                FileDropped?.Invoke(this, files);
            }
        }

        public event EventHandler<string> FileOpened;
        public void LoadFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return;

            try
            {
                _syncingFileDir = true;

                // 两边同时导航
                _fileListLeft.NavigateTo(folder);
                _fileListRight.NavigateTo(folder);

                // 保存到设置
                App.Settings.LastOpenedFolder = folder;
                App.Settings.Save();
            }
            finally
            {
                _syncingFileDir = false;
            }
        }
    }
}
