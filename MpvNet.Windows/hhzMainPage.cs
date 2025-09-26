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

            if (App.Settings.Enable3DMode)
            {
                // 显示左右两个 LOGO + 各自下方的提示
                _logoPicLeft.Visible = true;
                _logoPicRight.Visible = true;
                _hintLabelLeft.Visible = true;
                _hintLabelRight.Visible = true;

                int halfWidth = this.ClientSize.Width / 2;
                int centerY = (this.ClientSize.Height - _logoPicLeft.Height) / 2 - 50;

                // 左边 LOGO
                _logoPicLeft.Location = new Point((halfWidth - _logoPicLeft.Width) / 2 + 10, centerY);
                _hintLabelLeft.Location = new Point((halfWidth - _hintLabelLeft.Width) / 2 ,_logoPicLeft.Bottom + 50);

                // 右边 LOGO
                _logoPicRight.Location = new Point(halfWidth + (halfWidth - _logoPicRight.Width) / 2 - 10, centerY);
                _hintLabelRight.Location = new Point(halfWidth + (halfWidth - _hintLabelRight.Width) / 2, _logoPicRight.Bottom + 50);
            }
            else
            {
                // 显示一个 LOGO + 一个提示
                _logoPicLeft.Visible = true;
                _hintLabelLeft.Visible = true;

                _logoPicRight.Visible = false;
                _hintLabelRight.Visible = false;

                int centerX = (this.ClientSize.Width - _logoPicLeft.Width) / 2;
                int centerY = (this.ClientSize.Height - _logoPicLeft.Height) / 2 - 30;

                _logoPicLeft.Location = new Point(centerX, centerY);
                _hintLabelLeft.Location = new Point(
                    (this.ClientSize.Width - _hintLabelLeft.Width) / 2,
                    _logoPicLeft.Bottom + 50
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
