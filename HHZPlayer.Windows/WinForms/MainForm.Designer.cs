
namespace HHZPlayer.Windows.WinForms;

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
        cmbVO = new System.Windows.Forms.ComboBox();
        cmbhwdec = new System.Windows.Forms.ComboBox();
        cmbgpuapi = new System.Windows.Forms.ComboBox();
        cmbGpuContext = new System.Windows.Forms.ComboBox();
        panelTestLeft = new System.Windows.Forms.Panel();
        chkTestModeLeft = new System.Windows.Forms.CheckBox();
        label4 = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        label1 = new System.Windows.Forms.Label();
        panelTestRight = new System.Windows.Forms.Panel();
        chkTestModeRight = new System.Windows.Forms.CheckBox();
        comboBox2 = new System.Windows.Forms.ComboBox();
        comboBox3 = new System.Windows.Forms.ComboBox();
        comboBox4 = new System.Windows.Forms.ComboBox();
        comboBox5 = new System.Windows.Forms.ComboBox();
        label5 = new System.Windows.Forms.Label();
        label6 = new System.Windows.Forms.Label();
        label7 = new System.Windows.Forms.Label();
        label8 = new System.Windows.Forms.Label();
        gbRifeLeft = new System.Windows.Forms.GroupBox();
        chkFromPlayLeft = new System.Windows.Forms.CheckBox();
        lblVSRLeft = new System.Windows.Forms.Label();
        lblRifeLeft = new System.Windows.Forms.Label();
        chkVSRLeft = new System.Windows.Forms.CheckBox();
        cbRifeTimesLeft = new System.Windows.Forms.ComboBox();
        chkRifeLeft = new System.Windows.Forms.CheckBox();
        gbRifeRight = new System.Windows.Forms.GroupBox();
        chkFromPlayRight = new System.Windows.Forms.CheckBox();
        lblVSRRight = new System.Windows.Forms.Label();
        lblRifeRight = new System.Windows.Forms.Label();
        chkVSRRight = new System.Windows.Forms.CheckBox();
        cbRifeTimesRight = new System.Windows.Forms.ComboBox();
        chkRifeRight = new System.Windows.Forms.CheckBox();
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).BeginInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).BeginInit();
        panelTestLeft.SuspendLayout();
        panelTestRight.SuspendLayout();
        gbRifeLeft.SuspendLayout();
        gbRifeRight.SuspendLayout();
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
        // cmbVO
        // 
        cmbVO.BackColor = System.Drawing.Color.Black;
        cmbVO.ForeColor = System.Drawing.Color.White;
        cmbVO.FormattingEnabled = true;
        cmbVO.Items.AddRange(new object[] { "gpu", "gpu-next", "direct3d" });
        cmbVO.Location = new System.Drawing.Point(161, 24);
        cmbVO.Name = "cmbVO";
        cmbVO.Size = new System.Drawing.Size(139, 40);
        cmbVO.TabIndex = 37;
        cmbVO.TabStop = false;
        cmbVO.Text = "gpu";
        cmbVO.SelectedIndexChanged += cmbVO_SelectedIndexChanged;
        // 
        // cmbhwdec
        // 
        cmbhwdec.BackColor = System.Drawing.Color.Black;
        cmbhwdec.ForeColor = System.Drawing.Color.White;
        cmbhwdec.FormattingEnabled = true;
        cmbhwdec.Items.AddRange(new object[] { "auto", "yes", "no", "auto-copy", "auto-safe", "dxva2", "dxva2-copy", "d3d11va", "d3d11va-copy", "cuda", "cuda-copy", "nvdec", "nvdec-copy" });
        cmbhwdec.Location = new System.Drawing.Point(161, 70);
        cmbhwdec.Name = "cmbhwdec";
        cmbhwdec.Size = new System.Drawing.Size(139, 40);
        cmbhwdec.TabIndex = 38;
        cmbhwdec.TabStop = false;
        cmbhwdec.Text = "no";
        cmbhwdec.SelectedIndexChanged += cmbhwdec_SelectedIndexChanged;
        // 
        // cmbgpuapi
        // 
        cmbgpuapi.BackColor = System.Drawing.Color.Black;
        cmbgpuapi.ForeColor = System.Drawing.Color.White;
        cmbgpuapi.FormattingEnabled = true;
        cmbgpuapi.Items.AddRange(new object[] { "auto", "d3d11", "opengl", "vulkan" });
        cmbgpuapi.Location = new System.Drawing.Point(161, 116);
        cmbgpuapi.Name = "cmbgpuapi";
        cmbgpuapi.Size = new System.Drawing.Size(139, 40);
        cmbgpuapi.TabIndex = 39;
        cmbgpuapi.TabStop = false;
        cmbgpuapi.Text = "auto";
        cmbgpuapi.SelectedIndexChanged += cmbgpuapi_SelectedIndexChanged;
        // 
        // cmbGpuContext
        // 
        cmbGpuContext.BackColor = System.Drawing.Color.Black;
        cmbGpuContext.ForeColor = System.Drawing.Color.White;
        cmbGpuContext.FormattingEnabled = true;
        cmbGpuContext.Items.AddRange(new object[] { "auto", "d3d11", "angle", "win", "dxinterop", "winvk" });
        cmbGpuContext.Location = new System.Drawing.Point(161, 162);
        cmbGpuContext.Name = "cmbGpuContext";
        cmbGpuContext.Size = new System.Drawing.Size(139, 40);
        cmbGpuContext.TabIndex = 40;
        cmbGpuContext.TabStop = false;
        cmbGpuContext.Text = "auto";
        cmbGpuContext.SelectedIndexChanged += cmbGpuContext_SelectedIndexChanged;
        // 
        // panelTestLeft
        // 
        panelTestLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        panelTestLeft.BackColor = System.Drawing.Color.Black;
        panelTestLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelTestLeft.Controls.Add(chkTestModeLeft);
        panelTestLeft.Controls.Add(cmbVO);
        panelTestLeft.Controls.Add(cmbGpuContext);
        panelTestLeft.Controls.Add(cmbhwdec);
        panelTestLeft.Controls.Add(cmbgpuapi);
        panelTestLeft.Controls.Add(label4);
        panelTestLeft.Controls.Add(label3);
        panelTestLeft.Controls.Add(label2);
        panelTestLeft.Controls.Add(label1);
        panelTestLeft.ForeColor = System.Drawing.Color.White;
        panelTestLeft.Location = new System.Drawing.Point(41, 585);
        panelTestLeft.Name = "panelTestLeft";
        panelTestLeft.Size = new System.Drawing.Size(359, 263);
        panelTestLeft.TabIndex = 41;
        panelTestLeft.Visible = false;
        // 
        // chkTestModeLeft
        // 
        chkTestModeLeft.AutoSize = true;
        chkTestModeLeft.BackColor = System.Drawing.Color.Black;
        chkTestModeLeft.ForeColor = System.Drawing.Color.White;
        chkTestModeLeft.Location = new System.Drawing.Point(104, 218);
        chkTestModeLeft.Name = "chkTestModeLeft";
        chkTestModeLeft.Size = new System.Drawing.Size(121, 36);
        chkTestModeLeft.TabIndex = 45;
        chkTestModeLeft.TabStop = false;
        chkTestModeLeft.Text = "不隐藏";
        chkTestModeLeft.UseVisualStyleBackColor = false;
        chkTestModeLeft.CheckedChanged += chkTestModeLeft_CheckedChanged;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.BackColor = System.Drawing.Color.Black;
        label4.ForeColor = System.Drawing.Color.White;
        label4.Location = new System.Drawing.Point(41, 165);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(114, 32);
        label4.TabIndex = 44;
        label4.Text = "视频渲染\r\n";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.BackColor = System.Drawing.Color.Black;
        label3.ForeColor = System.Drawing.Color.White;
        label3.Location = new System.Drawing.Point(41, 119);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(109, 32);
        label3.TabIndex = 43;
        label3.Text = "GPU接口";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.BackColor = System.Drawing.Color.Black;
        label2.ForeColor = System.Drawing.Color.White;
        label2.Location = new System.Drawing.Point(41, 73);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(114, 32);
        label2.TabIndex = 42;
        label2.Text = "硬解码器";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.BackColor = System.Drawing.Color.Black;
        label1.ForeColor = System.Drawing.Color.White;
        label1.Location = new System.Drawing.Point(41, 27);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(114, 32);
        label1.TabIndex = 41;
        label1.Text = "视频输出";
        // 
        // panelTestRight
        // 
        panelTestRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        panelTestRight.BackColor = System.Drawing.Color.Black;
        panelTestRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        panelTestRight.Controls.Add(chkTestModeRight);
        panelTestRight.Controls.Add(comboBox2);
        panelTestRight.Controls.Add(comboBox3);
        panelTestRight.Controls.Add(comboBox4);
        panelTestRight.Controls.Add(comboBox5);
        panelTestRight.Controls.Add(label5);
        panelTestRight.Controls.Add(label6);
        panelTestRight.Controls.Add(label7);
        panelTestRight.Controls.Add(label8);
        panelTestRight.ForeColor = System.Drawing.Color.White;
        panelTestRight.Location = new System.Drawing.Point(406, 585);
        panelTestRight.Name = "panelTestRight";
        panelTestRight.Size = new System.Drawing.Size(359, 263);
        panelTestRight.TabIndex = 42;
        panelTestRight.Visible = false;
        // 
        // chkTestModeRight
        // 
        chkTestModeRight.AutoSize = true;
        chkTestModeRight.BackColor = System.Drawing.Color.Black;
        chkTestModeRight.ForeColor = System.Drawing.Color.White;
        chkTestModeRight.Location = new System.Drawing.Point(104, 218);
        chkTestModeRight.Name = "chkTestModeRight";
        chkTestModeRight.Size = new System.Drawing.Size(121, 36);
        chkTestModeRight.TabIndex = 45;
        chkTestModeRight.TabStop = false;
        chkTestModeRight.Text = "不隐藏";
        chkTestModeRight.UseVisualStyleBackColor = false;
        chkTestModeRight.CheckedChanged += chkTestModeRight_CheckedChanged;
        // 
        // comboBox2
        // 
        comboBox2.BackColor = System.Drawing.Color.Black;
        comboBox2.ForeColor = System.Drawing.Color.White;
        comboBox2.FormattingEnabled = true;
        comboBox2.Items.AddRange(new object[] { "gpu", "gpu-next", "direct3d" });
        comboBox2.Location = new System.Drawing.Point(161, 24);
        comboBox2.Name = "comboBox2";
        comboBox2.Size = new System.Drawing.Size(139, 40);
        comboBox2.TabIndex = 37;
        comboBox2.TabStop = false;
        comboBox2.Text = "gpu";
        comboBox2.SelectedIndexChanged += cmbVO_SelectedIndexChanged;
        // 
        // comboBox3
        // 
        comboBox3.BackColor = System.Drawing.Color.Black;
        comboBox3.ForeColor = System.Drawing.Color.White;
        comboBox3.FormattingEnabled = true;
        comboBox3.Items.AddRange(new object[] { "d3d11", "angle", "win", "dxinterop", "winvk" });
        comboBox3.Location = new System.Drawing.Point(161, 162);
        comboBox3.Name = "comboBox3";
        comboBox3.Size = new System.Drawing.Size(139, 40);
        comboBox3.TabIndex = 40;
        comboBox3.TabStop = false;
        comboBox3.Text = "auto";
        comboBox3.SelectedIndexChanged += cmbGpuContext_SelectedIndexChanged;
        // 
        // comboBox4
        // 
        comboBox4.BackColor = System.Drawing.Color.Black;
        comboBox4.ForeColor = System.Drawing.Color.White;
        comboBox4.FormattingEnabled = true;
        comboBox4.Items.AddRange(new object[] { "no", "auto", "yes", "auto-copy", "auto-safe", "dxva2", "dxva2-copy", "d3d11va", "d3d11va-copy", "cuda", "cuda-copy", "nvdec", "nvdec-copy" });
        comboBox4.Location = new System.Drawing.Point(161, 70);
        comboBox4.Name = "comboBox4";
        comboBox4.Size = new System.Drawing.Size(139, 40);
        comboBox4.TabIndex = 38;
        comboBox4.TabStop = false;
        comboBox4.Text = "no";
        comboBox4.SelectedIndexChanged += cmbhwdec_SelectedIndexChanged;
        // 
        // comboBox5
        // 
        comboBox5.BackColor = System.Drawing.Color.Black;
        comboBox5.ForeColor = System.Drawing.Color.White;
        comboBox5.FormattingEnabled = true;
        comboBox5.Items.AddRange(new object[] { "auto", "d3d11", "opengl", "vulkan" });
        comboBox5.Location = new System.Drawing.Point(161, 116);
        comboBox5.Name = "comboBox5";
        comboBox5.Size = new System.Drawing.Size(139, 40);
        comboBox5.TabIndex = 39;
        comboBox5.TabStop = false;
        comboBox5.Text = "auto";
        comboBox5.SelectedIndexChanged += cmbgpuapi_SelectedIndexChanged;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.BackColor = System.Drawing.Color.Black;
        label5.ForeColor = System.Drawing.Color.White;
        label5.Location = new System.Drawing.Point(41, 165);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(114, 32);
        label5.TabIndex = 44;
        label5.Text = "视频渲染\r\n";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.BackColor = System.Drawing.Color.Black;
        label6.ForeColor = System.Drawing.Color.White;
        label6.Location = new System.Drawing.Point(41, 119);
        label6.Name = "label6";
        label6.Size = new System.Drawing.Size(109, 32);
        label6.TabIndex = 43;
        label6.Text = "GPU接口";
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.BackColor = System.Drawing.Color.Black;
        label7.ForeColor = System.Drawing.Color.White;
        label7.Location = new System.Drawing.Point(41, 73);
        label7.Name = "label7";
        label7.Size = new System.Drawing.Size(114, 32);
        label7.TabIndex = 42;
        label7.Text = "硬解码器";
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.BackColor = System.Drawing.Color.Black;
        label8.ForeColor = System.Drawing.Color.White;
        label8.Location = new System.Drawing.Point(41, 27);
        label8.Name = "label8";
        label8.Size = new System.Drawing.Size(114, 32);
        label8.TabIndex = 41;
        label8.Text = "视频输出";
        // 
        // gbRifeLeft
        // 
        gbRifeLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        gbRifeLeft.Controls.Add(chkFromPlayLeft);
        gbRifeLeft.Controls.Add(lblVSRLeft);
        gbRifeLeft.Controls.Add(lblRifeLeft);
        gbRifeLeft.Controls.Add(chkVSRLeft);
        gbRifeLeft.Controls.Add(cbRifeTimesLeft);
        gbRifeLeft.Controls.Add(chkRifeLeft);
        gbRifeLeft.ForeColor = System.Drawing.Color.White;
        gbRifeLeft.Location = new System.Drawing.Point(1656, 599);
        gbRifeLeft.Name = "gbRifeLeft";
        gbRifeLeft.Size = new System.Drawing.Size(214, 249);
        gbRifeLeft.TabIndex = 43;
        gbRifeLeft.TabStop = false;
        gbRifeLeft.Text = "AI功能";
        gbRifeLeft.Visible = false;
        // 
        // chkFromPlayLeft
        // 
        chkFromPlayLeft.AutoSize = true;
        chkFromPlayLeft.Location = new System.Drawing.Point(15, 190);
        chkFromPlayLeft.Name = "chkFromPlayLeft";
        chkFromPlayLeft.Size = new System.Drawing.Size(196, 36);
        chkFromPlayLeft.TabIndex = 45;
        chkFromPlayLeft.TabStop = false;
        chkFromPlayLeft.Text = "记住当前位置";
        chkFromPlayLeft.UseVisualStyleBackColor = false;
        chkFromPlayLeft.CheckedChanged += chkFromPlay_CheckedChanged;
        // 
        // lblVSRLeft
        // 
        lblVSRLeft.BackColor = System.Drawing.Color.Black;
        lblVSRLeft.ForeColor = System.Drawing.Color.Red;
        lblVSRLeft.Location = new System.Drawing.Point(6, 155);
        lblVSRLeft.Name = "lblVSRLeft";
        lblVSRLeft.Size = new System.Drawing.Size(202, 32);
        lblVSRLeft.TabIndex = 44;
        lblVSRLeft.Tag = "abcdef";
        lblVSRLeft.Text = "超过1080p";
        lblVSRLeft.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        lblVSRLeft.Visible = false;
        // 
        // lblRifeLeft
        // 
        lblRifeLeft.BackColor = System.Drawing.Color.Black;
        lblRifeLeft.ForeColor = System.Drawing.Color.Red;
        lblRifeLeft.Location = new System.Drawing.Point(6, 84);
        lblRifeLeft.Name = "lblRifeLeft";
        lblRifeLeft.Size = new System.Drawing.Size(202, 32);
        lblRifeLeft.TabIndex = 43;
        lblRifeLeft.Text = "视频超过30帧";
        lblRifeLeft.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        lblRifeLeft.Visible = false;
        // 
        // chkVSRLeft
        // 
        chkVSRLeft.AutoSize = true;
        chkVSRLeft.Checked = true;
        chkVSRLeft.CheckState = System.Windows.Forms.CheckState.Checked;
        chkVSRLeft.Location = new System.Drawing.Point(22, 118);
        chkVSRLeft.Name = "chkVSRLeft";
        chkVSRLeft.Size = new System.Drawing.Size(186, 36);
        chkVSRLeft.TabIndex = 40;
        chkVSRLeft.TabStop = false;
        chkVSRLeft.Text = "RTX超分辨率";
        chkVSRLeft.UseVisualStyleBackColor = false;
        chkVSRLeft.CheckedChanged += chkVSR_CheckedChanged;
        // 
        // cbRifeTimesLeft
        // 
        cbRifeTimesLeft.BackColor = System.Drawing.Color.Black;
        cbRifeTimesLeft.ForeColor = System.Drawing.Color.White;
        cbRifeTimesLeft.FormattingEnabled = true;
        cbRifeTimesLeft.Items.AddRange(new object[] { "2x", "3x", "4x" });
        cbRifeTimesLeft.Location = new System.Drawing.Point(36, 231);
        cbRifeTimesLeft.Name = "cbRifeTimesLeft";
        cbRifeTimesLeft.Size = new System.Drawing.Size(147, 40);
        cbRifeTimesLeft.TabIndex = 38;
        cbRifeTimesLeft.TabStop = false;
        cbRifeTimesLeft.Text = "2x";
        cbRifeTimesLeft.Visible = false;
        cbRifeTimesLeft.SelectedIndexChanged += cbRifeTimes_SelectedIndexChanged;
        // 
        // chkRifeLeft
        // 
        chkRifeLeft.AutoSize = true;
        chkRifeLeft.Location = new System.Drawing.Point(22, 45);
        chkRifeLeft.Name = "chkRifeLeft";
        chkRifeLeft.Size = new System.Drawing.Size(146, 36);
        chkRifeLeft.TabIndex = 0;
        chkRifeLeft.TabStop = false;
        chkRifeLeft.Text = "实时补帧";
        chkRifeLeft.UseVisualStyleBackColor = false;
        chkRifeLeft.CheckedChanged += chkRife_CheckedChanged;
        // 
        // gbRifeRight
        // 
        gbRifeRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
        gbRifeRight.Controls.Add(chkFromPlayRight);
        gbRifeRight.Controls.Add(lblVSRRight);
        gbRifeRight.Controls.Add(lblRifeRight);
        gbRifeRight.Controls.Add(chkVSRRight);
        gbRifeRight.Controls.Add(cbRifeTimesRight);
        gbRifeRight.Controls.Add(chkRifeRight);
        gbRifeRight.ForeColor = System.Drawing.Color.White;
        gbRifeRight.Location = new System.Drawing.Point(1436, 599);
        gbRifeRight.Name = "gbRifeRight";
        gbRifeRight.Size = new System.Drawing.Size(214, 249);
        gbRifeRight.TabIndex = 44;
        gbRifeRight.TabStop = false;
        gbRifeRight.Text = "AI功能";
        gbRifeRight.Visible = false;
        // 
        // chkFromPlayRight
        // 
        chkFromPlayRight.AutoSize = true;
        chkFromPlayRight.Location = new System.Drawing.Point(15, 190);
        chkFromPlayRight.Name = "chkFromPlayRight";
        chkFromPlayRight.Size = new System.Drawing.Size(196, 36);
        chkFromPlayRight.TabIndex = 45;
        chkFromPlayRight.TabStop = false;
        chkFromPlayRight.Text = "记住当前位置";
        chkFromPlayRight.UseVisualStyleBackColor = false;
        chkFromPlayRight.CheckedChanged += chkFromPlay_CheckedChanged;
        // 
        // lblVSRRight
        // 
        lblVSRRight.BackColor = System.Drawing.Color.Black;
        lblVSRRight.ForeColor = System.Drawing.Color.Red;
        lblVSRRight.Location = new System.Drawing.Point(6, 155);
        lblVSRRight.Name = "lblVSRRight";
        lblVSRRight.Size = new System.Drawing.Size(202, 32);
        lblVSRRight.TabIndex = 44;
        lblVSRRight.Tag = "abcdef";
        lblVSRRight.Text = "超过1080p";
        lblVSRRight.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        lblVSRRight.Visible = false;
        // 
        // lblRifeRight
        // 
        lblRifeRight.BackColor = System.Drawing.Color.Black;
        lblRifeRight.ForeColor = System.Drawing.Color.Red;
        lblRifeRight.Location = new System.Drawing.Point(6, 84);
        lblRifeRight.Name = "lblRifeRight";
        lblRifeRight.Size = new System.Drawing.Size(202, 32);
        lblRifeRight.TabIndex = 42;
        lblRifeRight.Text = "视频超过30帧";
        lblRifeRight.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        lblRifeRight.Visible = false;
        // 
        // chkVSRRight
        // 
        chkVSRRight.AutoSize = true;
        chkVSRRight.Checked = true;
        chkVSRRight.CheckState = System.Windows.Forms.CheckState.Checked;
        chkVSRRight.Location = new System.Drawing.Point(22, 118);
        chkVSRRight.Name = "chkVSRRight";
        chkVSRRight.Size = new System.Drawing.Size(186, 36);
        chkVSRRight.TabIndex = 39;
        chkVSRRight.TabStop = false;
        chkVSRRight.Text = "RTX超分辨率";
        chkVSRRight.UseVisualStyleBackColor = false;
        chkVSRRight.CheckedChanged += chkVSR_CheckedChanged;
        // 
        // cbRifeTimesRight
        // 
        cbRifeTimesRight.BackColor = System.Drawing.Color.Black;
        cbRifeTimesRight.ForeColor = System.Drawing.Color.White;
        cbRifeTimesRight.FormattingEnabled = true;
        cbRifeTimesRight.Items.AddRange(new object[] { "2x", "3x", "4x" });
        cbRifeTimesRight.Location = new System.Drawing.Point(40, 231);
        cbRifeTimesRight.Name = "cbRifeTimesRight";
        cbRifeTimesRight.Size = new System.Drawing.Size(147, 40);
        cbRifeTimesRight.TabIndex = 38;
        cbRifeTimesRight.TabStop = false;
        cbRifeTimesRight.Text = "2x";
        cbRifeTimesRight.Visible = false;
        cbRifeTimesRight.SelectedIndexChanged += cbRifeTimes_SelectedIndexChanged;
        // 
        // chkRifeRight
        // 
        chkRifeRight.AutoSize = true;
        chkRifeRight.Location = new System.Drawing.Point(22, 45);
        chkRifeRight.Name = "chkRifeRight";
        chkRifeRight.Size = new System.Drawing.Size(146, 36);
        chkRifeRight.TabIndex = 0;
        chkRifeRight.TabStop = false;
        chkRifeRight.Text = "实时补帧";
        chkRifeRight.UseVisualStyleBackColor = false;
        chkRifeRight.CheckedChanged += chkRife_CheckedChanged;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(1920, 1080);
        Controls.Add(gbRifeRight);
        Controls.Add(gbRifeLeft);
        Controls.Add(panelTestRight);
        Controls.Add(panelTestLeft);
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
        StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        FormClosing += MainForm_FormClosing;
        KeyDown += MainForm_KeyDown;
        ((System.ComponentModel.ISupportInitialize)btnBackLeft).EndInit();
        ((System.ComponentModel.ISupportInitialize)btnBackRight).EndInit();
        panelTestLeft.ResumeLayout(false);
        panelTestLeft.PerformLayout();
        panelTestRight.ResumeLayout(false);
        panelTestRight.PerformLayout();
        gbRifeLeft.ResumeLayout(false);
        gbRifeLeft.PerformLayout();
        gbRifeRight.ResumeLayout(false);
        gbRifeRight.PerformLayout();
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
    private System.Windows.Forms.ComboBox cmbVO;
    private System.Windows.Forms.ComboBox cmbhwdec;
    private System.Windows.Forms.ComboBox cmbgpuapi;
    private System.Windows.Forms.ComboBox cmbGpuContext;
    private System.Windows.Forms.Panel panelTestLeft;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox chkTestModeLeft;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Panel panelTestRight;
    private System.Windows.Forms.CheckBox chkTestModeRight;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox comboBox2;
    private System.Windows.Forms.ComboBox comboBox3;
    private System.Windows.Forms.ComboBox comboBox4;
    private System.Windows.Forms.ComboBox comboBox5;
    private System.Windows.Forms.GroupBox gbRifeLeft;
    private System.Windows.Forms.ComboBox cbRifeTimesLeft;
    private System.Windows.Forms.CheckBox chkRifeLeft;
    private System.Windows.Forms.GroupBox gbRifeRight;
    private System.Windows.Forms.ComboBox cbRifeTimesRight;
    private System.Windows.Forms.CheckBox chkRifeRight;
    private System.Windows.Forms.CheckBox chkVSRLeft;
    private System.Windows.Forms.CheckBox chkVSRRight;
    private System.Windows.Forms.Label lblRifeRight;
    private System.Windows.Forms.Label lblVSRLeft;
    private System.Windows.Forms.Label lblRifeLeft;
    private System.Windows.Forms.Label lblVSRRight;
    private System.Windows.Forms.CheckBox chkFromPlayLeft;
    private System.Windows.Forms.CheckBox chkFromPlayRight;
}