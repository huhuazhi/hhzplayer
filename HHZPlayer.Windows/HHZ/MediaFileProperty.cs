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

namespace HHZPlayer.Windows.WinForms
{
    public partial class FormMediaProperty : Form
    {
        MainForm hhzForm;
        public FormMediaProperty(MainForm hhzform)
        {
            hhzForm = hhzform;
            InitializeComponent();
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
                MessageBox.Show("宽高值不能为空只能为数字 !", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            hhzSettingsManager.Current.VideoAspestW = tbVidW.Text;
            hhzSettingsManager.Current.VideoAspestH = tbVidH.Text;
            Player.SetPropertyString("video-aspect-override", $"{tbVidW.Text}:{tbVidH.Text}");
        }

        private void FormMediaProperty_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Close();
        }

        private void rb16x9_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "16";
            hhzSettingsManager.Current.VideoAspestH = "9";
            Player.SetPropertyString("video-aspect-override", "16:9");
        }
        private void rb16x10_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "16";
            hhzSettingsManager.Current.VideoAspestH = "10";
            Player.SetPropertyString("video-aspect-override", "16:10");
        }

        private void rb32x9_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "32";
            hhzSettingsManager.Current.VideoAspestH = "9";
            Player.SetPropertyString("video-aspect-override", "32:9");
        }

        private void rb235x1_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "2.35";
            hhzSettingsManager.Current.VideoAspestH = "1";
            Player.SetPropertyString("video-aspect-override", "2.35:1");
        }

        private void rb47x1_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "4.7";
            hhzSettingsManager.Current.VideoAspestH = "1";
            Player.SetPropertyString("video-aspect-override", "4.7:1");
        }

        private void rb4x3_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "4";
            hhzSettingsManager.Current.VideoAspestH = "3";
            Player.SetPropertyString("video-aspect-override", "4:3");
        }

        private void rbVidaspDefault_Click(object sender, EventArgs e)
        {
            hhzSettingsManager.Current.VideoAspestW = "0";
            hhzSettingsManager.Current.VideoAspestH = "0";
            set3DFullHalf();
        }
        void set3DFullHalf()
        {
            if (Player.Duration.TotalMicroseconds > 0 && (hhzSettingsManager.Current.VideoAspestW == "0" && hhzSettingsManager.Current.VideoAspestH == "0"))
            {
                var vw = Player.GetPropertyInt("width");
                var vh = Player.GetPropertyInt("height");
                var scrW = hhzForm.Width;
                var scrH = hhzForm.Height;
                //FullSBS画面比例最小值为2.35 * 2 : 1
                if ((double)vw / vh < 2.35 / 1) // half-SBS
                {
                    if ((scrW / scrH) <= 16.00 / 9)
                    {
                        //One Screen
                        Player.SetPropertyString("video-aspect-override", $"{scrW}:{(scrW / 2) / (vw / 2) * vh}");
                    }
                    else
                    {
                        //Two Screen
                        Player.SetPropertyString("video-aspect-override", $"{scrW * 2}:{(scrW / 2) / (vw / 2) * vh}");
                    }
                }
                else // full-SBS
                {
                    if ((scrW / scrH) <= 16.00 / 9)
                    {
                        //One Screen
                        Player.SetPropertyString("video-aspect-override", $"{scrW}:{scrW / (vw / 2) * vh}");
                    }
                    else
                    {
                        //Two Screen
                        Player.SetPropertyString("video-aspect-override", $"{scrW}:{scrW / vw * vh}");
                        //Player.SetPropertyString("video-aspect-override", $"{Width}:{vh}");
                    }
                }
            }
        }

        private void FormMediaProperty_Activated(object sender, EventArgs e)
        {
            Debug.Print($"{hhzSettingsManager.Current.VideoAspestW}x{hhzSettingsManager.Current.VideoAspestH}");
            switch ($"{hhzSettingsManager.Current.VideoAspestW}x{hhzSettingsManager.Current.VideoAspestH}")
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
                    tbVidW.Text = hhzSettingsManager.Current.VideoAspestW;
                    tbVidH.Text = hhzSettingsManager.Current.VideoAspestH;
                    break;
            }
        }
    }
}
