using MyApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpvNet.Windows.WinForms
{
    public partial class FormMediaProperty : Form
    {
        public FormMediaProperty()
        {
            InitializeComponent();
            Debug.Print($"{SettingsManager.Current.VideoAspestW}x{SettingsManager.Current.VideoAspestH}");
            switch ($"{SettingsManager.Current.VideoAspestW}x{SettingsManager.Current.VideoAspestH}")
            {
                case "16x9":
                    rb16x9.Checked = true;
                    break;
                case "16x10":
                    rb16x10.Checked = true;
                    break;
                case "32x9":
                    rb32x9.Checked = true;
                    break;
                case "2.35x1":
                    rb235x1.Checked = true;
                    break;
                case "4.7x1":
                    rb47x1.Checked = true;
                    break;
                case "4x3":
                    rb4x3.Checked = true;
                    break;
                case "0x0":
                    rbVidaspDefault.Checked = true;
                    break;
                default:
                    tbVidW.Text = SettingsManager.Current.VideoAspestW;
                    tbVidH.Text = SettingsManager.Current.VideoAspestH;
                    break;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnVidApply_Click(object sender, EventArgs e)
        {
            int w;
            int h;
            int.TryParse(tbVidW.Text, out w);
            int.TryParse(tbVidH.Text, out h);
            if ((w <= 0 || w > 65535) && (h <= 0 || h > 65535))
            {
                MessageBox.Show("宽高值不能为空或大于65535 !", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Player.SetPropertyString("video-aspect-override", $"{tbVidW.Text}:{tbVidH.Text}");
            SettingsManager.Current.VideoAspestW = tbVidW.Text;
            SettingsManager.Current.VideoAspestH = tbVidH.Text;
        }

        private void FormMediaProperty_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Close();
        }

        private void rbVidaspDefault_CheckedChanged(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "0");
            SettingsManager.Current.VideoAspestW = "0";
            SettingsManager.Current.VideoAspestH = "0";
        }
        private void rb16x9_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "16:9");
            SettingsManager.Current.VideoAspestW = "16";
            SettingsManager.Current.VideoAspestH = "9";

        }
        private void rb16x10_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "16:10");
            SettingsManager.Current.VideoAspestW = "16";
            SettingsManager.Current.VideoAspestH = "10";
        }

        private void rb32x9_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "32:9");
            SettingsManager.Current.VideoAspestW = "32";
            SettingsManager.Current.VideoAspestH = "9";

        }

        private void rb235x1_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "2.35:1");
            SettingsManager.Current.VideoAspestW = "2.35";
            SettingsManager.Current.VideoAspestH = "1";
        }

        private void rb47x1_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "4.7:1");
            SettingsManager.Current.VideoAspestW = "4.7";
            SettingsManager.Current.VideoAspestH = "1";
        }

        private void rb4x3_Click(object sender, EventArgs e)
        {
            Player.SetPropertyString("video-aspect-override", "4:3");
            SettingsManager.Current.VideoAspestW = "4";
            SettingsManager.Current.VideoAspestH = "3";
        }
    }
}
