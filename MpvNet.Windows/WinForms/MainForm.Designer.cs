
namespace MpvNet.Windows.WinForms;

partial class MainForm
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
    void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        CursorTimer = new System.Windows.Forms.Timer(components);
        ProgressTimer = new System.Windows.Forms.Timer(components);
        progressBarLeft = new System.Windows.Forms.ProgressBar();
        btnBackLeft = new System.Windows.Forms.PictureBox();
        btn3DLeft = new System.Windows.Forms.Button();
        btn3DSubtitleModeLeft = new System.Windows.Forms.Button();
        btnBackRight = new System.Windows.Forms.PictureBox();
        progressBarRight = new System.Windows.Forms.ProgressBar();
        btn3DSubtitleModeRight = new System.Windows.Forms.Button();
        btn3DRight = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).BeginInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).BeginInit();
        SuspendLayout();
        // 
        // CursorTimer
        // 
        CursorTimer.Enabled = true;
        CursorTimer.Interval = 500;
        CursorTimer.Tick += CursorTimer_Tick;
        // 
        // progressBarLeft
        // 
        progressBarLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        progressBarLeft.ForeColor = System.Drawing.Color.ForestGreen;
        progressBarLeft.Location = new System.Drawing.Point(302, 1184);
        progressBarLeft.Name = "progressBarLeft";
        progressBarLeft.Size = new System.Drawing.Size(1981, 49);
        progressBarLeft.TabIndex = 8;
        progressBarLeft.Visible = false;
        // 
        // btnBackLeft
        // 
        btnBackLeft.BackColor = System.Drawing.Color.Transparent;
        btnBackLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        btnBackLeft.Image = (System.Drawing.Image)resources.GetObject("btnBackLeft.Image");
        btnBackLeft.Location = new System.Drawing.Point(40, 40);
        btnBackLeft.Name = "btnBackLeft";
        btnBackLeft.Size = new System.Drawing.Size(118, 104);
        btnBackLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        btnBackLeft.TabIndex = 7;
        btnBackLeft.TabStop = false;
        btnBackLeft.Visible = false;
        // 
        // btn3DLeft
        // 
        btn3DLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DLeft.BackColor = System.Drawing.Color.Black;
        btn3DLeft.ForeColor = System.Drawing.Color.White;
        btn3DLeft.Location = new System.Drawing.Point(40, 1163);
        btn3DLeft.Name = "btn3DLeft";
        btn3DLeft.Size = new System.Drawing.Size(238, 82);
        btn3DLeft.TabIndex = 6;
        btn3DLeft.Text = "3D";
        btn3DLeft.UseVisualStyleBackColor = false;
        btn3DLeft.Visible = false;
        // 
        // btn3DSubtitleModeLeft
        // 
        btn3DSubtitleModeLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeLeft.BackColor = System.Drawing.Color.Black;
        btn3DSubtitleModeLeft.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeLeft.Location = new System.Drawing.Point(40, 1063);
        btn3DSubtitleModeLeft.Name = "btn3DSubtitleModeLeft";
        btn3DSubtitleModeLeft.Size = new System.Drawing.Size(238, 82);
        btn3DSubtitleModeLeft.TabIndex = 9;
        btn3DSubtitleModeLeft.Text = "3D字幕模式:自动";
        btn3DSubtitleModeLeft.UseVisualStyleBackColor = false;
        btn3DSubtitleModeLeft.Visible = false;
        btn3DSubtitleModeLeft.Click += btnSubtitle_Click;
        // 
        // btnBackRight
        // 
        btnBackRight.BackColor = System.Drawing.Color.Transparent;
        btnBackRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        btnBackRight.Image = (System.Drawing.Image)resources.GetObject("btnBackRight.Image");
        btnBackRight.Location = new System.Drawing.Point(40, 40);
        btnBackRight.Name = "btnBackRight";
        btnBackRight.Size = new System.Drawing.Size(118, 104);
        btnBackRight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        btnBackRight.TabIndex = 10;
        btnBackRight.TabStop = false;
        btnBackRight.Visible = false;
        // 
        // progressBarRight
        // 
        progressBarRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        progressBarRight.Location = new System.Drawing.Point(302, 1184);
        progressBarRight.Name = "progressBarRight";
        progressBarRight.Size = new System.Drawing.Size(1981, 49);
        progressBarRight.TabIndex = 11;
        progressBarRight.Visible = false;
        // 
        // btn3DSubtitleModeRight
        // 
        btn3DSubtitleModeRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeRight.BackColor = System.Drawing.Color.Black;
        btn3DSubtitleModeRight.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeRight.Location = new System.Drawing.Point(40, 1063);
        btn3DSubtitleModeRight.Name = "btn3DSubtitleModeRight";
        btn3DSubtitleModeRight.Size = new System.Drawing.Size(238, 82);
        btn3DSubtitleModeRight.TabIndex = 12;
        btn3DSubtitleModeRight.Text = "3D字幕模式:自动";
        btn3DSubtitleModeRight.UseVisualStyleBackColor = false;
        btn3DSubtitleModeRight.Visible = false;
        btn3DSubtitleModeRight.Click += btnSubtitle_Click;
        // 
        // btn3DRight
        // 
        btn3DRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DRight.BackColor = System.Drawing.Color.Black;
        btn3DRight.ForeColor = System.Drawing.Color.White;
        btn3DRight.Location = new System.Drawing.Point(40, 1163);
        btn3DRight.Name = "btn3DRight";
        btn3DRight.Size = new System.Drawing.Size(238, 82);
        btn3DRight.TabIndex = 13;
        btn3DRight.Text = "3D";
        btn3DRight.UseVisualStyleBackColor = false;
        btn3DRight.Visible = false;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(2333, 1284);
        Controls.Add(btn3DLeft);
        Controls.Add(btn3DRight);
        Controls.Add(btn3DSubtitleModeLeft);
        Controls.Add(btn3DSubtitleModeRight);
        Controls.Add(progressBarLeft);
        Controls.Add(progressBarRight);
        Controls.Add(btnBackRight);
        Controls.Add(btnBackLeft);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        Name = "MainForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        FormClosing += MainForm_FormClosing;
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).EndInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Timer CursorTimer;
    private System.Windows.Forms.Timer ProgressTimer;
    private System.Windows.Forms.ProgressBar progressBarLeft;
    private System.Windows.Forms.PictureBox btnBackLeft;
    private System.Windows.Forms.Button btn3DLeft;
    public System.Windows.Forms.Button btn3DSubtitleModeLeft;
    private System.Windows.Forms.PictureBox btnBackRight;
    private System.Windows.Forms.ProgressBar progressBarRight;
    public System.Windows.Forms.Button btn3DSubtitleModeRight;
    private System.Windows.Forms.Button btn3DRight;
}