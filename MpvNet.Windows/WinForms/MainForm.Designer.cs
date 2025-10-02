
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
        btn3DLeft = new System.Windows.Forms.Label();
        btn3DSubtitleModeLeft = new System.Windows.Forms.Label();
        btnBackRight = new System.Windows.Forms.PictureBox();
        progressBarRight = new System.Windows.Forms.ProgressBar();
        btn3DSubtitleModeRight = new System.Windows.Forms.Label();
        btn3DRight = new System.Windows.Forms.Label();
        btnAudioTrackLeft = new System.Windows.Forms.Label();
        btnSubtitleTrackLeft = new System.Windows.Forms.Label();
        btnVideoTrackLeft = new System.Windows.Forms.Label();
        btnFullScreenLeft = new System.Windows.Forms.Label();
        btnPlayLeft = new System.Windows.Forms.Label();
        btnPlayRight = new System.Windows.Forms.Label();
        btnVideoTrackRight = new System.Windows.Forms.Label();
        btnSubtitleTrackRight = new System.Windows.Forms.Label();
        btnAudioTrackRight = new System.Windows.Forms.Label();
        btnRenderRight = new System.Windows.Forms.Label();
        btnRenderLeft = new System.Windows.Forms.Label();
        lblDurationLeft = new System.Windows.Forms.Label();
        lblDurationRight = new System.Windows.Forms.Label();
        lblStatusLeft = new System.Windows.Forms.Label();
        lblStatusRight = new System.Windows.Forms.Label();
        lblVolumeLeft = new System.Windows.Forms.Label();
        lblVolumeRight = new System.Windows.Forms.Label();
        lblToastRight = new System.Windows.Forms.Label();
        lblToastLeft = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).BeginInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).BeginInit();
        SuspendLayout();
        // 
        // CursorTimer
        // 
        CursorTimer.Interval = 500;
        CursorTimer.Tick += CursorTimer_Tick;
        // 
        // progressBarLeft
        // 
        progressBarLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        progressBarLeft.ForeColor = System.Drawing.Color.DeepSkyBlue;
        progressBarLeft.Location = new System.Drawing.Point(40, 920);
        progressBarLeft.Name = "progressBarLeft";
        progressBarLeft.Size = new System.Drawing.Size(1830, 45);
        progressBarLeft.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
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
        btn3DLeft.BackColor = System.Drawing.Color.Transparent;
        btn3DLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DLeft.CausesValidation = false;
        btn3DLeft.ForeColor = System.Drawing.Color.White;
        btn3DLeft.Location = new System.Drawing.Point(40, 976);
        btn3DLeft.Name = "btn3DLeft";
        btn3DLeft.Size = new System.Drawing.Size(104, 82);
        btn3DLeft.TabIndex = 6;
        btn3DLeft.Text = "3D";
        btn3DLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // btn3DSubtitleModeLeft
        // 
        btn3DSubtitleModeLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeLeft.BackColor = System.Drawing.Color.Transparent;
        btn3DSubtitleModeLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DSubtitleModeLeft.CausesValidation = false;
        btn3DSubtitleModeLeft.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeLeft.Location = new System.Drawing.Point(150, 976);
        btn3DSubtitleModeLeft.Name = "btn3DSubtitleModeLeft";
        btn3DSubtitleModeLeft.Size = new System.Drawing.Size(238, 82);
        btn3DSubtitleModeLeft.TabIndex = 9;
        btn3DSubtitleModeLeft.Text = "3D字幕模式:自动";
        btn3DSubtitleModeLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btn3DSubtitleModeLeft.Visible = false;
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
        progressBarRight.Location = new System.Drawing.Point(40, 920);
        progressBarRight.Name = "progressBarRight";
        progressBarRight.Size = new System.Drawing.Size(1830, 45);
        progressBarRight.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
        progressBarRight.TabIndex = 11;
        progressBarRight.Visible = false;
        // 
        // btn3DSubtitleModeRight
        // 
        btn3DSubtitleModeRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeRight.BackColor = System.Drawing.Color.Transparent;
        btn3DSubtitleModeRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DSubtitleModeRight.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeRight.Location = new System.Drawing.Point(150, 976);
        btn3DSubtitleModeRight.Name = "btn3DSubtitleModeRight";
        btn3DSubtitleModeRight.Size = new System.Drawing.Size(238, 82);
        btn3DSubtitleModeRight.TabIndex = 12;
        btn3DSubtitleModeRight.Text = "3D字幕模式:自动";
        btn3DSubtitleModeRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btn3DSubtitleModeRight.Visible = false;
        btn3DSubtitleModeRight.Click += btnSubtitle_Click;
        // 
        // btn3DRight
        // 
        btn3DRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DRight.BackColor = System.Drawing.Color.Transparent;
        btn3DRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DRight.ForeColor = System.Drawing.Color.White;
        btn3DRight.Location = new System.Drawing.Point(40, 976);
        btn3DRight.Name = "btn3DRight";
        btn3DRight.Size = new System.Drawing.Size(104, 82);
        btn3DRight.TabIndex = 13;
        btn3DRight.Text = "3D";
        btn3DRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // btnAudioTrackLeft
        // 
        btnAudioTrackLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnAudioTrackLeft.BackColor = System.Drawing.Color.Transparent;
        btnAudioTrackLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnAudioTrackLeft.CausesValidation = false;
        btnAudioTrackLeft.ForeColor = System.Drawing.Color.White;
        btnAudioTrackLeft.Location = new System.Drawing.Point(638, 976);
        btnAudioTrackLeft.Name = "btnAudioTrackLeft";
        btnAudioTrackLeft.Size = new System.Drawing.Size(238, 82);
        btnAudioTrackLeft.TabIndex = 14;
        btnAudioTrackLeft.Text = "音轨";
        btnAudioTrackLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnAudioTrackLeft.Visible = false;
        // 
        // btnSubtitleTrackLeft
        // 
        btnSubtitleTrackLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnSubtitleTrackLeft.BackColor = System.Drawing.Color.Transparent;
        btnSubtitleTrackLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnSubtitleTrackLeft.CausesValidation = false;
        btnSubtitleTrackLeft.ForeColor = System.Drawing.Color.White;
        btnSubtitleTrackLeft.Location = new System.Drawing.Point(882, 976);
        btnSubtitleTrackLeft.Name = "btnSubtitleTrackLeft";
        btnSubtitleTrackLeft.Size = new System.Drawing.Size(238, 82);
        btnSubtitleTrackLeft.TabIndex = 15;
        btnSubtitleTrackLeft.Text = "字幕";
        btnSubtitleTrackLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnSubtitleTrackLeft.Visible = false;
        // 
        // btnVideoTrackLeft
        // 
        btnVideoTrackLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnVideoTrackLeft.BackColor = System.Drawing.Color.Transparent;
        btnVideoTrackLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnVideoTrackLeft.CausesValidation = false;
        btnVideoTrackLeft.ForeColor = System.Drawing.Color.White;
        btnVideoTrackLeft.Location = new System.Drawing.Point(394, 976);
        btnVideoTrackLeft.Name = "btnVideoTrackLeft";
        btnVideoTrackLeft.Size = new System.Drawing.Size(238, 82);
        btnVideoTrackLeft.TabIndex = 16;
        btnVideoTrackLeft.Text = "视频";
        btnVideoTrackLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnVideoTrackLeft.Visible = false;
        // 
        // btnFullScreenLeft
        // 
        btnFullScreenLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnFullScreenLeft.BackColor = System.Drawing.Color.Transparent;
        btnFullScreenLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnFullScreenLeft.CausesValidation = false;
        btnFullScreenLeft.ForeColor = System.Drawing.Color.White;
        btnFullScreenLeft.Location = new System.Drawing.Point(1766, 976);
        btnFullScreenLeft.Name = "btnFullScreenLeft";
        btnFullScreenLeft.Size = new System.Drawing.Size(104, 82);
        btnFullScreenLeft.TabIndex = 17;
        btnFullScreenLeft.Text = "全屏";
        btnFullScreenLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // btnPlayLeft
        // 
        btnPlayLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnPlayLeft.BackColor = System.Drawing.Color.Transparent;
        btnPlayLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnPlayLeft.CausesValidation = false;
        btnPlayLeft.ForeColor = System.Drawing.Color.White;
        btnPlayLeft.Location = new System.Drawing.Point(1656, 976);
        btnPlayLeft.Name = "btnPlayLeft";
        btnPlayLeft.Size = new System.Drawing.Size(104, 82);
        btnPlayLeft.TabIndex = 18;
        btnPlayLeft.Text = "播放";
        btnPlayLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnPlayLeft.Visible = false;
        // 
        // btnPlayRight
        // 
        btnPlayRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnPlayRight.BackColor = System.Drawing.Color.Transparent;
        btnPlayRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnPlayRight.CausesValidation = false;
        btnPlayRight.ForeColor = System.Drawing.Color.White;
        btnPlayRight.Location = new System.Drawing.Point(1656, 976);
        btnPlayRight.Name = "btnPlayRight";
        btnPlayRight.Size = new System.Drawing.Size(104, 82);
        btnPlayRight.TabIndex = 23;
        btnPlayRight.Text = "播放";
        btnPlayRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnPlayRight.Visible = false;
        // 
        // btnVideoTrackRight
        // 
        btnVideoTrackRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnVideoTrackRight.BackColor = System.Drawing.Color.Transparent;
        btnVideoTrackRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnVideoTrackRight.CausesValidation = false;
        btnVideoTrackRight.ForeColor = System.Drawing.Color.White;
        btnVideoTrackRight.Location = new System.Drawing.Point(394, 976);
        btnVideoTrackRight.Name = "btnVideoTrackRight";
        btnVideoTrackRight.Size = new System.Drawing.Size(238, 82);
        btnVideoTrackRight.TabIndex = 21;
        btnVideoTrackRight.Text = "视频";
        btnVideoTrackRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnVideoTrackRight.Visible = false;
        // 
        // btnSubtitleTrackRight
        // 
        btnSubtitleTrackRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnSubtitleTrackRight.BackColor = System.Drawing.Color.Transparent;
        btnSubtitleTrackRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnSubtitleTrackRight.CausesValidation = false;
        btnSubtitleTrackRight.ForeColor = System.Drawing.Color.White;
        btnSubtitleTrackRight.Location = new System.Drawing.Point(882, 976);
        btnSubtitleTrackRight.Name = "btnSubtitleTrackRight";
        btnSubtitleTrackRight.Size = new System.Drawing.Size(238, 82);
        btnSubtitleTrackRight.TabIndex = 20;
        btnSubtitleTrackRight.Text = "字幕";
        btnSubtitleTrackRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnSubtitleTrackRight.Visible = false;
        // 
        // btnAudioTrackRight
        // 
        btnAudioTrackRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnAudioTrackRight.BackColor = System.Drawing.Color.Transparent;
        btnAudioTrackRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnAudioTrackRight.CausesValidation = false;
        btnAudioTrackRight.ForeColor = System.Drawing.Color.White;
        btnAudioTrackRight.Location = new System.Drawing.Point(638, 976);
        btnAudioTrackRight.Name = "btnAudioTrackRight";
        btnAudioTrackRight.Size = new System.Drawing.Size(238, 82);
        btnAudioTrackRight.TabIndex = 19;
        btnAudioTrackRight.Text = "音轨";
        btnAudioTrackRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnAudioTrackRight.Visible = false;
        // 
        // btnRenderRight
        // 
        btnRenderRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnRenderRight.BackColor = System.Drawing.Color.Transparent;
        btnRenderRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnRenderRight.CausesValidation = false;
        btnRenderRight.ForeColor = System.Drawing.Color.White;
        btnRenderRight.Location = new System.Drawing.Point(1126, 976);
        btnRenderRight.Name = "btnRenderRight";
        btnRenderRight.Size = new System.Drawing.Size(238, 82);
        btnRenderRight.TabIndex = 25;
        btnRenderRight.Text = "2D渲染器";
        btnRenderRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnRenderRight.Visible = false;
        // 
        // btnRenderLeft
        // 
        btnRenderLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnRenderLeft.BackColor = System.Drawing.Color.Transparent;
        btnRenderLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnRenderLeft.CausesValidation = false;
        btnRenderLeft.ForeColor = System.Drawing.Color.White;
        btnRenderLeft.Location = new System.Drawing.Point(1126, 976);
        btnRenderLeft.Name = "btnRenderLeft";
        btnRenderLeft.Size = new System.Drawing.Size(238, 82);
        btnRenderLeft.TabIndex = 24;
        btnRenderLeft.Text = "2D渲染器";
        btnRenderLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnRenderLeft.Visible = false;
        // 
        // lblDurationLeft
        // 
        lblDurationLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        lblDurationLeft.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblDurationLeft.ForeColor = System.Drawing.Color.White;
        lblDurationLeft.Location = new System.Drawing.Point(41, 851);
        lblDurationLeft.Name = "lblDurationLeft";
        lblDurationLeft.Size = new System.Drawing.Size(382, 66);
        lblDurationLeft.TabIndex = 28;
        lblDurationLeft.Text = "00:00:00 / 00:00:00";
        lblDurationLeft.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblDurationLeft.Visible = false;
        // 
        // lblDurationRight
        // 
        lblDurationRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        lblDurationRight.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblDurationRight.ForeColor = System.Drawing.Color.White;
        lblDurationRight.Location = new System.Drawing.Point(41, 851);
        lblDurationRight.Name = "lblDurationRight";
        lblDurationRight.Size = new System.Drawing.Size(382, 66);
        lblDurationRight.TabIndex = 29;
        lblDurationRight.Text = "00:00:00 / 00:00:00";
        lblDurationRight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblDurationRight.Visible = false;
        // 
        // lblStatusLeft
        // 
        lblStatusLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatusLeft.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblStatusLeft.ForeColor = System.Drawing.Color.White;
        lblStatusLeft.Location = new System.Drawing.Point(429, 851);
        lblStatusLeft.Name = "lblStatusLeft";
        lblStatusLeft.Size = new System.Drawing.Size(1215, 66);
        lblStatusLeft.TabIndex = 30;
        lblStatusLeft.Text = "正在加载...";
        lblStatusLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblStatusLeft.Visible = false;
        // 
        // lblStatusRight
        // 
        lblStatusRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatusRight.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblStatusRight.ForeColor = System.Drawing.Color.White;
        lblStatusRight.Location = new System.Drawing.Point(429, 851);
        lblStatusRight.Name = "lblStatusRight";
        lblStatusRight.Size = new System.Drawing.Size(1215, 66);
        lblStatusRight.TabIndex = 31;
        lblStatusRight.Text = "正在加载...";
        lblStatusRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblStatusRight.Visible = false;
        // 
        // lblVolumeLeft
        // 
        lblVolumeLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        lblVolumeLeft.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblVolumeLeft.ForeColor = System.Drawing.Color.White;
        lblVolumeLeft.Location = new System.Drawing.Point(1629, 851);
        lblVolumeLeft.Name = "lblVolumeLeft";
        lblVolumeLeft.Size = new System.Drawing.Size(242, 66);
        lblVolumeLeft.TabIndex = 32;
        lblVolumeLeft.Text = "音量:100%";
        lblVolumeLeft.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblVolumeLeft.Visible = false;
        // 
        // lblVolumeRight
        // 
        lblVolumeRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        lblVolumeRight.Font = new System.Drawing.Font("Segoe UI", 16.125F);
        lblVolumeRight.ForeColor = System.Drawing.Color.White;
        lblVolumeRight.Location = new System.Drawing.Point(1629, 851);
        lblVolumeRight.Name = "lblVolumeRight";
        lblVolumeRight.Size = new System.Drawing.Size(242, 66);
        lblVolumeRight.TabIndex = 33;
        lblVolumeRight.Text = "音量:100%";
        lblVolumeRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblVolumeRight.Visible = false;
        // 
        // lblToastRight
        // 
        lblToastRight.BackColor = System.Drawing.Color.Transparent;
        lblToastRight.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        lblToastRight.ForeColor = System.Drawing.Color.White;
        lblToastRight.Location = new System.Drawing.Point(0, 0);
        lblToastRight.Name = "lblToastRight";
        lblToastRight.Size = new System.Drawing.Size(1920, 73);
        lblToastRight.TabIndex = 34;
        lblToastRight.Text = "Toast";
        lblToastRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblToastRight.Visible = false;
        // 
        // lblToastLeft
        // 
        lblToastLeft.BackColor = System.Drawing.Color.Transparent;
        lblToastLeft.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        lblToastLeft.ForeColor = System.Drawing.Color.White;
        lblToastLeft.Location = new System.Drawing.Point(0, 0);
        lblToastLeft.Name = "lblToastLeft";
        lblToastLeft.Size = new System.Drawing.Size(1920, 73);
        lblToastLeft.TabIndex = 35;
        lblToastLeft.Text = "Toast";
        lblToastLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblToastLeft.Visible = false;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(1920, 1080);
        Controls.Add(lblToastLeft);
        Controls.Add(lblToastRight);
        Controls.Add(lblVolumeRight);
        Controls.Add(lblVolumeLeft);
        Controls.Add(lblStatusRight);
        Controls.Add(lblStatusLeft);
        Controls.Add(lblDurationRight);
        Controls.Add(lblDurationLeft);
        Controls.Add(btnFullScreenLeft);
        Controls.Add(btnRenderRight);
        Controls.Add(btnRenderLeft);
        Controls.Add(btnPlayRight);
        Controls.Add(btnVideoTrackRight);
        Controls.Add(btnSubtitleTrackRight);
        Controls.Add(btnAudioTrackRight);
        Controls.Add(btnPlayLeft);
        Controls.Add(btnVideoTrackLeft);
        Controls.Add(btnSubtitleTrackLeft);
        Controls.Add(btnAudioTrackLeft);
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
        KeyDown += MainForm_KeyDown;
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).EndInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Timer CursorTimer;
    private System.Windows.Forms.Timer ProgressTimer;
    private System.Windows.Forms.ProgressBar progressBarLeft;
    private System.Windows.Forms.PictureBox btnBackLeft;
    private System.Windows.Forms.Label btn3DLeft;
    public System.Windows.Forms.Label btn3DSubtitleModeLeft;
    private System.Windows.Forms.PictureBox btnBackRight;
    private System.Windows.Forms.ProgressBar progressBarRight;
    public System.Windows.Forms.Label btn3DSubtitleModeRight;
    private System.Windows.Forms.Label btn3DRight;
    public System.Windows.Forms.Label btnAudioTrackLeft;
    public System.Windows.Forms.Label btnSubtitleTrackLeft;
    public System.Windows.Forms.Label btnVideoTrackLeft;
    private System.Windows.Forms.Label btnFullScreenLeft;
    private System.Windows.Forms.Label btnPlayLeft;
    private System.Windows.Forms.Label btnPlayRight;
    public System.Windows.Forms.Label btnVideoTrackRight;
    public System.Windows.Forms.Label btnSubtitleTrackRight;
    public System.Windows.Forms.Label btnAudioTrackRight;
    public System.Windows.Forms.Label btnRenderRight;
    public System.Windows.Forms.Label btnRenderLeft;
    private System.Windows.Forms.Label lblDurationLeft;
    private System.Windows.Forms.Label lblDurationRight;
    private System.Windows.Forms.Label lblStatusLeft;
    private System.Windows.Forms.Label lblStatusRight;
    private System.Windows.Forms.Label lblVolumeLeft;
    private System.Windows.Forms.Label lblVolumeRight;
    private System.Windows.Forms.Label lblToastRight;
    private System.Windows.Forms.Label lblToastLeft;
}