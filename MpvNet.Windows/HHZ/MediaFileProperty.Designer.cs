namespace MpvNet.Windows.WinForms
{
    partial class FormMediaProperty
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMediaProperty));
            groupBox1 = new System.Windows.Forms.GroupBox();
            rbVidaspDefault = new System.Windows.Forms.RadioButton();
            btnOK = new System.Windows.Forms.Button();
            tbVidH = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            tbVidW = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            rb4x3 = new System.Windows.Forms.RadioButton();
            rb47x1 = new System.Windows.Forms.RadioButton();
            rb235x1 = new System.Windows.Forms.RadioButton();
            rb32x9 = new System.Windows.Forms.RadioButton();
            rb16x10 = new System.Windows.Forms.RadioButton();
            rb16x9 = new System.Windows.Forms.RadioButton();
            btnCancel = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = System.Drawing.Color.Transparent;
            groupBox1.Controls.Add(rbVidaspDefault);
            groupBox1.Controls.Add(btnOK);
            groupBox1.Controls.Add(tbVidH);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(tbVidW);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(rb4x3);
            groupBox1.Controls.Add(rb47x1);
            groupBox1.Controls.Add(rb235x1);
            groupBox1.Controls.Add(rb32x9);
            groupBox1.Controls.Add(rb16x10);
            groupBox1.Controls.Add(rb16x9);
            groupBox1.ForeColor = System.Drawing.Color.White;
            groupBox1.Location = new System.Drawing.Point(38, 28);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(882, 186);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "强制视频比例";
            // 
            // rbVidaspDefault
            // 
            rbVidaspDefault.AutoSize = true;
            rbVidaspDefault.Location = new System.Drawing.Point(45, 51);
            rbVidaspDefault.Name = "rbVidaspDefault";
            rbVidaspDefault.Size = new System.Drawing.Size(93, 35);
            rbVidaspDefault.TabIndex = 1;
            rbVidaspDefault.Text = "默认";
            rbVidaspDefault.UseVisualStyleBackColor = true;
            rbVidaspDefault.Click += rbVidaspDefault_Click;
            // 
            // btnOK
            // 
            btnOK.BackColor = System.Drawing.Color.White;
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.ForeColor = System.Drawing.Color.Black;
            btnOK.Location = new System.Drawing.Point(458, 104);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(148, 59);
            btnOK.TabIndex = 12;
            btnOK.Text = "应用";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += btnVidApply_Click;
            // 
            // tbVidH
            // 
            tbVidH.Location = new System.Drawing.Point(302, 114);
            tbVidH.Name = "tbVidH";
            tbVidH.Size = new System.Drawing.Size(131, 38);
            tbVidH.TabIndex = 10;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(239, 116);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(57, 31);
            label2.TabIndex = 11;
            label2.Text = "x 高";
            // 
            // tbVidW
            // 
            tbVidW.Location = new System.Drawing.Point(92, 114);
            tbVidW.Name = "tbVidW";
            tbVidW.Size = new System.Drawing.Size(131, 38);
            tbVidW.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(44, 116);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 31);
            label1.TabIndex = 8;
            label1.Text = "宽";
            // 
            // rb4x3
            // 
            rb4x3.AutoSize = true;
            rb4x3.Location = new System.Drawing.Point(764, 51);
            rb4x3.Name = "rb4x3";
            rb4x3.Size = new System.Drawing.Size(79, 35);
            rb4x3.TabIndex = 7;
            rb4x3.Text = "4:3";
            rb4x3.UseVisualStyleBackColor = true;
            rb4x3.Click += rb4x3_Click;
            // 
            // rb47x1
            // 
            rb47x1.AutoSize = true;
            rb47x1.Location = new System.Drawing.Point(643, 51);
            rb47x1.Name = "rb47x1";
            rb47x1.Size = new System.Drawing.Size(99, 35);
            rb47x1.TabIndex = 6;
            rb47x1.Text = "4.7:1";
            rb47x1.UseVisualStyleBackColor = true;
            rb47x1.Click += rb47x1_Click;
            // 
            // rb235x1
            // 
            rb235x1.AutoSize = true;
            rb235x1.Location = new System.Drawing.Point(506, 51);
            rb235x1.Name = "rb235x1";
            rb235x1.Size = new System.Drawing.Size(113, 35);
            rb235x1.TabIndex = 5;
            rb235x1.Text = "2.35:1";
            rb235x1.UseVisualStyleBackColor = true;
            rb235x1.Click += rb235x1_Click;
            // 
            // rb32x9
            // 
            rb32x9.AutoSize = true;
            rb32x9.Location = new System.Drawing.Point(394, 51);
            rb32x9.Name = "rb32x9";
            rb32x9.Size = new System.Drawing.Size(93, 35);
            rb32x9.TabIndex = 4;
            rb32x9.Text = "32:9";
            rb32x9.UseVisualStyleBackColor = true;
            rb32x9.Click += rb32x9_Click;
            // 
            // rb16x10
            // 
            rb16x10.AutoSize = true;
            rb16x10.Location = new System.Drawing.Point(262, 51);
            rb16x10.Name = "rb16x10";
            rb16x10.Size = new System.Drawing.Size(107, 35);
            rb16x10.TabIndex = 3;
            rb16x10.Text = "16:10";
            rb16x10.UseVisualStyleBackColor = true;
            rb16x10.Click += rb16x10_Click;
            // 
            // rb16x9
            // 
            rb16x9.AutoSize = true;
            rb16x9.Location = new System.Drawing.Point(144, 51);
            rb16x9.Name = "rb16x9";
            rb16x9.Size = new System.Drawing.Size(93, 35);
            rb16x9.TabIndex = 2;
            rb16x9.Text = "16:9";
            rb16x9.UseVisualStyleBackColor = true;
            rb16x9.Click += rb16x9_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(772, 246);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(148, 59);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "关闭";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormMediaProperty
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(946, 334);
            Controls.Add(btnCancel);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "FormMediaProperty";
            Text = "媒体文件设置";
            TopMost = true;
            Activated += FormMediaProperty_Activated;
            FormClosing += FormMediaProperty_FormClosing;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb16x10;
        private System.Windows.Forms.RadioButton rb16x9;
        private System.Windows.Forms.TextBox tbVidH;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbVidW;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rb4x3;
        private System.Windows.Forms.RadioButton rb47x1;
        private System.Windows.Forms.RadioButton rb235x1;
        private System.Windows.Forms.RadioButton rb32x9;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RadioButton rbVidaspDefault;
    }
}