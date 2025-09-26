using System;
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

        // 防止事件互相递归触发
        private bool _syncing = false;        // 滚动/选中
        private bool _syncingHover = false;   // 悬浮

        // 蓝色框的相对位置（按你的背景测）
        private const float FRAME_LEFT = 0.0255f;
        private const float FRAME_TOP = 0.095f;
        private const float FRAME_WIDTH = 0.253f;
        private const float FRAME_HEIGHT = 0.400f;

        private Bitmap bg;

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
            string bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.jpg");
            if (File.Exists(bgPath))
                bg = new Bitmap(bgPath);

            // 左右 LOGO/文字
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

            this.Controls.Add(_logoPicLeft);
            this.Controls.Add(_logoPicRight);
            this.Controls.Add(_hintLabelLeft);
            this.Controls.Add(_hintLabelRight);

            // 两个磁盘列表
            _diskListLeft = new DiskList
            {
                RowHeight = 110,
                RowSpacing = 5,
                ShowNotReady = true
            };
            _diskListLeft.DiskSelected += (_, root) => Console.WriteLine("[Left] 选择磁盘：" + root);

            _diskListRight = new DiskList
            {
                RowHeight = 110,
                RowSpacing = 5,
                ShowNotReady = true
            };
            _diskListRight.DiskSelected += (_, root) => Console.WriteLine("[Right] 选择磁盘：" + root);

            this.Controls.Add(_diskListLeft);
            this.Controls.Add(_diskListRight);
            _diskListLeft.BringToFront();
            _diskListRight.BringToFront();

            // —— 双向同步：滚动 —— 
            _diskListLeft.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncing) return;
                try { _syncing = true; _diskListRight.ScrollOffset = e.OffsetY; }
                finally { _syncing = false; }
            };
            _diskListRight.ViewportOffsetChanged += (_, e) =>
            {
                if (_syncing) return;
                try { _syncing = true; _diskListLeft.ScrollOffset = e.OffsetY; }
                finally { _syncing = false; }
            };

            // —— 双向同步：选中 —— 
            _diskListLeft.SelectionChanged += (_, e) =>
            {
                if (_syncing) return;
                try { _syncing = true; _diskListRight.SelectIndex(e.Index, ensureVisible: true, raiseEvent: false); }
                finally { _syncing = false; }
            };
            _diskListRight.SelectionChanged += (_, e) =>
            {
                if (_syncing) return;
                try { _syncing = true; _diskListLeft.SelectIndex(e.Index, ensureVisible: true, raiseEvent: false); }
                finally { _syncing = false; }
            };

            // —— 双向同步：悬浮（hover 高亮）——
            _diskListLeft.HoverChanged += (_, e) =>
            {
                if (_syncingHover) return;
                try { _syncingHover = true; _diskListRight.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingHover = false; }
            };
            _diskListRight.HoverChanged += (_, e) =>
            {
                if (_syncingHover) return;
                try { _syncingHover = true; _diskListLeft.SetHotIndex(e.Index, raiseEvent: false); }
                finally { _syncingHover = false; }
            };

            // 初次布局
            this.Resize += (_, __) =>
            {
                UpdateLogoPosition();
                UpdateDiskListBounds();
            };
            UpdateLogoPosition();
            UpdateDiskListBounds();

            _diskListLeft.Reload();
            _diskListRight.Reload();

            // 拖入
            this.AllowDrop = true;
            this.DragEnter += HHZMainPage_DragEnter;
            this.DragDrop += HHZMainPage_DragDrop;
        }

        // 计算蓝色框在某个宿主矩形内的位置
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

        // 根据模式布置一或两个列表
        private void UpdateDiskListBounds()
        {
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            if (App.Settings.Enable3DMode)
            {
                var leftHost = new Rectangle(0, 0, w / 2, h);
                var rightHost = new Rectangle(w / 2, 0, w - w / 2, h);

                _diskListLeft.Bounds = CalcBlueFrame(leftHost);
                _diskListRight.Bounds = CalcBlueFrame(rightHost);

                _diskListLeft.Visible = true;
                _diskListRight.Visible = true;
            }
            else
            {
                var fullHost = new Rectangle(0, 0, w, h);
                _diskListLeft.Bounds = CalcBlueFrame(fullHost);
                _diskListLeft.Visible = true;

                _diskListRight.Visible = false;
            }

            _diskListLeft.Invalidate();
            _diskListRight.Invalidate();
        }

        private void UpdateLogoPosition()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (w == 0 || h == 0) return;

            int logoSize = Math.Max(24, Math.Min(64, h / 20));
            _logoPicLeft.Size = new Size(logoSize, logoSize);
            _logoPicRight.Size = new Size(logoSize, logoSize);

            float fontSize = Math.Max(8, h / 120f);
            var font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            _hintLabelLeft.Font = font;
            _hintLabelRight.Font = font;

            int margin = 10;
            int shift = 10;
            int offsetX = 50;

            if (App.Settings.Enable3DMode)
            {
                _logoPicLeft.Visible = _logoPicRight.Visible = true;
                _hintLabelLeft.Visible = _hintLabelRight.Visible = true;

                int halfWidth = w / 2;

                _logoPicLeft.Location = new Point(offsetX + margin + shift, margin);
                _logoPicRight.Location = new Point(halfWidth + offsetX + margin - shift, margin);

                _hintLabelLeft.Location = new Point(
                    _logoPicLeft.Right + 5,
                    _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2
                );

                int textOffsetX = _hintLabelLeft.Left - (offsetX + margin);
                _hintLabelRight.Location = new Point(
                    halfWidth + offsetX + margin + textOffsetX,
                    _logoPicRight.Top + (_logoPicRight.Height - _hintLabelRight.Height) / 2
                );
            }
            else
            {
                _logoPicLeft.Visible = true;
                _hintLabelLeft.Visible = true;

                _logoPicRight.Visible = false;
                _hintLabelRight.Visible = false;

                _logoPicLeft.Location = new Point(offsetX + margin, margin);
                _hintLabelLeft.Location = new Point(
                    _logoPicLeft.Right + 5,
                    _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2
                );
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

        // 背景绘制
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

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            if (App.Settings.Enable3DMode)
            {
                Rectangle leftRect = new Rectangle(0, 0, w / 2, h);
                Rectangle rightRect = new Rectangle(w / 2, 0, w - w / 2, h);
                g.DrawImage(bg, leftRect);
                g.DrawImage(bg, rightRect);
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
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void HHZMainPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                FileDropped?.Invoke(this, files);
            }
        }
    }
}
