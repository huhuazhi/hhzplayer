using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpvNet.Windows
{
    public partial class HHZMainPage : UserControl
    {
        private PictureBox _logoPicLeft;
        private PictureBox _logoPicRight;
        private Label _hintLabelLeft;
        private Label _hintLabelRight;
        public HHZMainPage()
        {
            InitializeComponent();
            this.DoubleBuffered = true;     // 开启双缓冲，避免闪烁
            //强制整个 UserControl 双缓冲（推荐）
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.BackColor = Color.Transparent;   // 默认背景黑色
            this.Dock = DockStyle.Fill;     // 默认填充父容器

            // 左眼 LOGO
            _logoPicLeft = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 128,
                Height = 128,
                Image = LoadMyLogo(),
                BackColor = Color.Transparent,
                Visible = true                
            };

            // 右眼 LOGO
            _logoPicRight = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 128,
                Height = 128,
                Image = LoadMyLogo(),
                BackColor = Color.Transparent,
                Visible = false // 默认只显示一个
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

            this.Resize += (_, __) => UpdateLogoPosition();
            UpdateLogoPosition();
        }
        private void UpdateLogoPosition()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (w == 0 || h == 0) return;

            // LOGO 大小：高度的 1/20，最小 24，最大 64
            int logoSize = Math.Max(24, Math.Min(64, h / 20));
            _logoPicLeft.Size = new Size(logoSize, logoSize);
            _logoPicRight.Size = new Size(logoSize, logoSize);

            // 字体大小：高度的 1/120，最小 8
            float fontSize = Math.Max(8, h / 120f);
            var font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            _hintLabelLeft.Font = font;
            _hintLabelRight.Font = font;

            int margin = 10;       // 基础边距
            int shift = 10;        // LOGO 水平错位距离（越大越立体）
            int offsetX = 50;      // ⭐ 整体水平偏移量（你可以随时改）

            if (App.Settings.Enable3DMode)
            {
                _logoPicLeft.Visible = _logoPicRight.Visible = true;
                _hintLabelLeft.Visible = _hintLabelRight.Visible = true;

                int halfWidth = w / 2;

                // 左屏 LOGO（整体往右偏移 offsetX，再右移一点）
                _logoPicLeft.Location = new Point(offsetX + margin + shift, margin);

                // 右屏 LOGO（整体往右偏移 offsetX，再左移一点）
                _logoPicRight.Location = new Point(halfWidth + offsetX + margin - shift, margin);

                // 左边文字
                _hintLabelLeft.Location = new Point(
                    _logoPicLeft.Right + 5,
                    _logoPicLeft.Top + (_logoPicLeft.Height - _hintLabelLeft.Height) / 2
                );

                // 右边文字（保持和左边文字的相对关系）
                int textOffsetX = _hintLabelLeft.Left - (offsetX + margin);
                _hintLabelRight.Location = new Point(
                    halfWidth + offsetX + margin + textOffsetX,
                    _logoPicRight.Top + (_logoPicRight.Height - _hintLabelRight.Height) / 2
                );
            }
            else
            {
                // 普通模式：整体往右偏移 offsetX
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
            // 1) 放到可执行目录下 Assets\mylogo.png
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylogo.png");
            if (File.Exists(path)) return new Bitmap(path);

            // 2) 或者嵌入到资源 Properties.Resources.MyLogo
            // return Properties.Resources.MyLogo;

            // 兜底：一个空白图，防止空引用
            var bmp = new Bitmap(1, 1);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.Transparent);
            return bmp;
        }

        private void hhzMainPage_Paint(object sender, PaintEventArgs e)
        {
            //// 示例：画一个提示文本
            //string text = "My Visualizer Control (空白占位)";
            //using (var brush = new SolidBrush(Color.White))
            //using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            //{
            //    e.Graphics.DrawString(text, this.Font, brush, this.ClientRectangle, sf);
            //}
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            string bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.png");
            if (!File.Exists(bgPath))
            {
                e.Graphics.Clear(Color.Black); // 没有背景就填充黑色
                return;
            }

            using (var bg = new Bitmap(bgPath))
            {
                int w = this.ClientSize.Width;
                int h = this.ClientSize.Height;

                if (App.Settings.Enable3DMode)
                {
                    // 左边一半
                    Rectangle leftRect = new Rectangle(0, 0, w / 2, h);
                    e.Graphics.DrawImage(bg, leftRect);

                    // 右边一半
                    Rectangle rightRect = new Rectangle(w / 2, 0, w / 2, h);
                    e.Graphics.DrawImage(bg, rightRect);
                }
                else
                {
                    // 普通模式：整幅铺满
                    e.Graphics.DrawImage(bg, new Rectangle(0, 0, w, h));
                }
            }
        }

        // 自定义事件：文件拖入
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
                FileDropped?.Invoke(this, files); // 触发事件，把文件传出去
            }
        }
    }
}
