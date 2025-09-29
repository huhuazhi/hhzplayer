
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
        btnStopLeft = new System.Windows.Forms.Label();
        btnPlayLeft = new System.Windows.Forms.Label();
        btnPlayRight = new System.Windows.Forms.Label();
        btnStopRight = new System.Windows.Forms.Label();
        btnVideoTrackRight = new System.Windows.Forms.Label();
        btnSubtitleTrackRight = new System.Windows.Forms.Label();
        btnAudioTrackRight = new System.Windows.Forms.Label();
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
        progressBarLeft.ForeColor = System.Drawing.Color.DeepSkyBlue;
        progressBarLeft.Location = new System.Drawing.Point(40, 1184);
        progressBarLeft.Name = "progressBarLeft";
        progressBarLeft.Size = new System.Drawing.Size(2243, 49);
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
        btn3DLeft.Location = new System.Drawing.Point(40, 1063);
        btn3DLeft.Name = "btn3DLeft";
        btn3DLeft.Size = new System.Drawing.Size(104, 82);
        btn3DLeft.TabIndex = 6;
        btn3DLeft.Text = "3D";
        btn3DLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btn3DLeft.Visible = false;
        // 
        // btn3DSubtitleModeLeft
        // 
        btn3DSubtitleModeLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeLeft.BackColor = System.Drawing.Color.Transparent;
        btn3DSubtitleModeLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DSubtitleModeLeft.CausesValidation = false;
        btn3DSubtitleModeLeft.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeLeft.Location = new System.Drawing.Point(150, 1063);
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
        progressBarRight.Location = new System.Drawing.Point(40, 1184);
        progressBarRight.Name = "progressBarRight";
        progressBarRight.Size = new System.Drawing.Size(2243, 49);
        progressBarRight.TabIndex = 11;
        progressBarRight.Visible = false;
        // 
        // btn3DSubtitleModeRight
        // 
        btn3DSubtitleModeRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btn3DSubtitleModeRight.BackColor = System.Drawing.Color.Transparent;
        btn3DSubtitleModeRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btn3DSubtitleModeRight.ForeColor = System.Drawing.Color.White;
        btn3DSubtitleModeRight.Location = new System.Drawing.Point(150, 1063);
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
        btn3DRight.Location = new System.Drawing.Point(40, 1063);
        btn3DRight.Name = "btn3DRight";
        btn3DRight.Size = new System.Drawing.Size(104, 82);
        btn3DRight.TabIndex = 13;
        btn3DRight.Text = "3D";
        btn3DRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btn3DRight.Visible = false;
        // 
        // btnAudioTrackLeft
        // 
        btnAudioTrackLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnAudioTrackLeft.BackColor = System.Drawing.Color.Transparent;
        btnAudioTrackLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnAudioTrackLeft.CausesValidation = false;
        btnAudioTrackLeft.ForeColor = System.Drawing.Color.White;
        btnAudioTrackLeft.Location = new System.Drawing.Point(638, 1063);
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
        btnSubtitleTrackLeft.Location = new System.Drawing.Point(882, 1063);
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
        btnVideoTrackLeft.Location = new System.Drawing.Point(394, 1063);
        btnVideoTrackLeft.Name = "btnVideoTrackLeft";
        btnVideoTrackLeft.Size = new System.Drawing.Size(238, 82);
        btnVideoTrackLeft.TabIndex = 16;
        btnVideoTrackLeft.Text = "视频";
        btnVideoTrackLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnVideoTrackLeft.Visible = false;
        // 
        // btnStopLeft
        // 
        btnStopLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnStopLeft.BackColor = System.Drawing.Color.Transparent;
        btnStopLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnStopLeft.CausesValidation = false;
        btnStopLeft.ForeColor = System.Drawing.Color.White;
        btnStopLeft.Location = new System.Drawing.Point(2179, 1063);
        btnStopLeft.Name = "btnStopLeft";
        btnStopLeft.Size = new System.Drawing.Size(104, 82);
        btnStopLeft.TabIndex = 17;
        btnStopLeft.Text = "停止";
        btnStopLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnStopLeft.Visible = false;
        // 
        // btnPlayLeft
        // 
        btnPlayLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnPlayLeft.BackColor = System.Drawing.Color.Transparent;
        btnPlayLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnPlayLeft.CausesValidation = false;
        btnPlayLeft.ForeColor = System.Drawing.Color.White;
        btnPlayLeft.Location = new System.Drawing.Point(2069, 1063);
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
        btnPlayRight.Location = new System.Drawing.Point(2069, 1063);
        btnPlayRight.Name = "btnPlayRight";
        btnPlayRight.Size = new System.Drawing.Size(104, 82);
        btnPlayRight.TabIndex = 23;
        btnPlayRight.Text = "播放";
        btnPlayRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnPlayRight.Visible = false;
        // 
        // btnStopRight
        // 
        btnStopRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        btnStopRight.BackColor = System.Drawing.Color.Transparent;
        btnStopRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnStopRight.CausesValidation = false;
        btnStopRight.ForeColor = System.Drawing.Color.White;
        btnStopRight.Location = new System.Drawing.Point(2179, 1063);
        btnStopRight.Name = "btnStopRight";
        btnStopRight.Size = new System.Drawing.Size(104, 82);
        btnStopRight.TabIndex = 22;
        btnStopRight.Text = "停止";
        btnStopRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnStopRight.Visible = false;
        // 
        // btnVideoTrackRight
        // 
        btnVideoTrackRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        btnVideoTrackRight.BackColor = System.Drawing.Color.Transparent;
        btnVideoTrackRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        btnVideoTrackRight.CausesValidation = false;
        btnVideoTrackRight.ForeColor = System.Drawing.Color.White;
        btnVideoTrackRight.Location = new System.Drawing.Point(394, 1063);
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
        btnSubtitleTrackRight.Location = new System.Drawing.Point(882, 1063);
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
        btnAudioTrackRight.Location = new System.Drawing.Point(638, 1063);
        btnAudioTrackRight.Name = "btnAudioTrackRight";
        btnAudioTrackRight.Size = new System.Drawing.Size(238, 82);
        btnAudioTrackRight.TabIndex = 19;
        btnAudioTrackRight.Text = "音轨";
        btnAudioTrackRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        btnAudioTrackRight.Visible = false;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(2333, 1284);
        Controls.Add(btnPlayRight);
        Controls.Add(btnStopRight);
        Controls.Add(btnVideoTrackRight);
        Controls.Add(btnSubtitleTrackRight);
        Controls.Add(btnAudioTrackRight);
        Controls.Add(btnPlayLeft);
        Controls.Add(btnStopLeft);
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
    private System.Windows.Forms.Label btnStopLeft;
    private System.Windows.Forms.Label btnPlayLeft;
    private System.Windows.Forms.Label btnPlayRight;
    private System.Windows.Forms.Label btnStopRight;
    public System.Windows.Forms.Label btnVideoTrackRight;
    public System.Windows.Forms.Label btnSubtitleTrackRight;
    public System.Windows.Forms.Label btnAudioTrackRight;
}