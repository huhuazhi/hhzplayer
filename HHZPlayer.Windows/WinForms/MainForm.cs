using CommunityToolkit.Mvvm.Messaging;
using HHZPlayer.MVVM;
using HHZPlayer.Windows.HHZ;
using HHZPlayer.Windows.UI;
using MyApp;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using static HHZPlayer.AppSettings;
using static HHZPlayer.Windows.Help.WinApiHelp;
using static HHZPlayer.Windows.Native.WinApi;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace HHZPlayer.Windows.WinForms;

public partial class MainForm : Form
{
    public SnapManager SnapManager = new SnapManager();
    public IntPtr MpvWindowHandle { get; set; }
    public bool WasShown { get; set; }
    public static MainForm? Instance { get; set; }

    Point _lastCursorPosition;

    int _lastCursorChanged;
    int _cursorAutohide = 1000;
    bool bPause = false;

    //WpfControls.MenuItem? _fullScreenUIMemuItem;
    /// <summary>
    /// 用于控制是否开启3D字幕的命令（在hhzlib.dll 线路B的源码里新增）
    /// </summary>
    const string CMD_sub_stereo_on = "sub-stereo-on";

    // 定义在 Form 类里
    //private Timer clickTimer;
    private bool isDoubleClick = false;

    private Timer ToastTimer;

    public MainForm()
    {
        InitializeComponent();

        //鼠标单击和双击区分用的Timer
        //clickTimer = new Timer();
        //clickTimer.Interval = 300; //给一个双击比较短的固定值300ms，体验会好很多,避免有用户吧系统双击事件调的过长，导致单击等待太久，系统默认为500ms
        //SystemInformation.DoubleClickTime; // 系统双击时间
        //clickTimer.Tick += ClickTimer_Tick;

        //弹Toast信息用的Timer
        ToastTimer = new Timer();
        ToastTimer.Interval = 1000; // Toast显示时长
        ToastTimer.Tick += (s, e) =>
        {
            ToastTimer.Stop();
            HideToast(); // 清除提示
        };

        DoubleBuffered = true;     // 开启双缓冲，避免闪烁
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.OptimizedDoubleBuffer, true);
        this.UpdateStyles();

        cmdlineArg = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (cmdlineArg.Length > 0)
        {
            if (CommandlineHelper.ProcessCommandline(cmdlineArg))
            {
                //未来处理其他命令行参数备用
            }
        }
        UpdateDarkMode();
        InitializehhzOverlay();
        InitPlayer();

        if (cmdlineArg.Length > 0)
        {
            //// 如果有命令行参数，直接初始化播放器并加载文件
            //hhzMainPage.Visible = false;
            //overlayPanel.Visible = true;
            //CursorTimer.Enabled = true;
            //List<string> files = [];

            //if (App.Settings.Enable3DMode)
            //{
            //    btn3DLeft.Visible = false;
            //    btn3DRight.Visible = false;
            //}
            //else
            //{
            //    btn3DLeft.Visible = false;
            //    btnFullScreenLeft.Visible = false;
            //}
            HhzMainPage_FileOpened(null, [.. CommandlineHelper.files]);
        }
    }

    // 定时器 Tick
    private void ClickTimer_Tick(object sender, EventArgs e)
    {
        //clickTimer.Stop();

        //if (!isDoubleClick)
        //{
        //    BtnPlay_Click(null, null);
        //}
    }

    double timepos;
    private void InitPlayer()
    {
        Player.Init(@overlayPanel.Handle, false, CommandlineHelper.originalArgs.ToArray());
        //Player.SetPropertyString("log-file", "hhzplayer-vs.log");
        //Player.SetPropertyString("msg-level", "all=v");
        Player.Command("set pause yes");

        Player.FileLoaded += Player_FileLoaded;

        // 订阅播放进度
        Player.ObservePropertyDouble("time-pos", (double value) =>
        {
            if (!double.IsNaN(value))
            {
                timepos = value;
                // 在 UI 线程里更新控件
                this.Invoke((MethodInvoker)(() =>
                {
                    lblDurationLeft.Text = $"{TimeSpan.FromSeconds(timepos).ToString(@"hh\:mm\:ss")} / {TimeSpan.FromSeconds(Player.Duration.TotalSeconds).ToString(@"hh\:mm\:ss")}";
                    lblDurationRight.Text = lblDurationLeft.Text;
                    //Debug.Print($"进度: {value:F1} 秒");
                    SetProgressBarMax(Player.Duration.TotalSeconds);
                    if (value <= progressBarLeft.Maximum)
                    {
                        SetProgressBarValue(value);
                    }
                }));
            }
        });

        Player.ObservePropertyBool("pause", (bool value) =>
        {
            this.Invoke((MethodInvoker)(() =>
            {
                if (value)
                {
                    bPause = true;
                    btnPlayLeft.Text = "播放";
                    btnPlayRight.Text = "播放";
                }
                else
                {
                    bPause = false;
                    btnPlayLeft.Text = "暂停";
                    btnPlayRight.Text = "暂停";
                }
                lblStatusLeft.Text = $"{(bPause ? "暂停中" : "播放中")}";
                lblStatusRight.Text = lblStatusLeft.Text;
                if (Player.Duration.TotalSeconds > 0)
                {
                    ShowCursor();
                    ShowVideoOSD();
                }
            }));
        });
    }

    HHZMainPage hhzMainPage = new HHZMainPage();
    private void InitializehhzOverlay()
    {
        frmMediaProperty = new FormMediaProperty(this);
        hhzMainPage.BringToFront();
        // 如果上次目录存在就恢复
        if (!string.IsNullOrEmpty(App.Settings.LastOpenedFolder) &&
            Directory.Exists(App.Settings.LastOpenedFolder))
        {
            hhzMainPage.LoadFolder(App.Settings.LastOpenedFolder);
        }

        Controls.Add(hhzMainPage);
        overlayPanel = new Panel
        {
            Parent = this,
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
        };
        overlayPanel.Visible = false;

        Controls.Add(overlayPanel);

        Text = "hhzPlayer";

        CycleFullScreenFor3D(App.Settings.Enable3DMode);
        hhzMainPage.FileOpened += HhzMainPage_FileOpened;
        hhzMainPage.FileDropped += HhzMainPage_FileDropped;

        overlayPanel.MouseClick += OverlayPanel_MouseClick;
        overlayPanel.MouseMove += OverlayPanel_MouseMove;
        overlayPanel.MouseDoubleClick += OverlayPanel_MouseDoubleClick;
        overlayPanel.AllowDrop = true;
        overlayPanel.DragEnter += OverlayPanel_DragEnter; ;
        overlayPanel.DragDrop += overlayPanel_DragDrop;

        btnBackLeft.Click += btnBack_Click;
        btnBackRight.Click += btnBack_Click;

        btn3DLeft.Click += btn3D_Click;
        btn3DRight.Click += btn3D_Click;

        btn3DSubtitleModeLeft.Click += btnSubtitle_Click;
        btn3DSubtitleModeRight.Click += btnSubtitle_Click;

        btnRenderLeft.Click += BtnRender_Click;
        btnRenderRight.Click += BtnRender_Click;

        btnVideoTrackLeft.Click += BtnVideoTrack_Click;
        btnVideoTrackRight.Click += BtnVideoTrack_Click;

        btnAudioTrackLeft.Click += BtnAudioTrack_Click;
        btnAudioTrackRight.Click += BtnAudioTrack_Click;

        btnSubtitleTrackLeft.Click += BtnSubtitleTrack_Click;
        btnSubtitleTrackRight.Click += BtnSubtitleTrack_Click;

        btnPlayLeft.Click += BtnPlay_Click;
        btnPlayRight.Click += BtnPlay_Click;

        btnFullScreenLeft.Click += BtnFullScreen_Click;

        progressBarLeft.MouseClick += ProgressBar_MouseClick;
        progressBarRight.MouseClick += ProgressBar_MouseClick;
        progressBarLeft.MouseMove += ProgressBar_MouseMove;
        progressBarRight.MouseMove += ProgressBar_MouseMove;
        progressBarLeft.MouseLeave += ProgressBar_MouseLeave;
        progressBarRight.MouseLeave += ProgressBar_MouseLeave;

        lblVolumeLeft.Text = $"{App.Settings.Volume}%";
        lblVolumeRight.Text = lblVolumeLeft.Text;
        HideVideoOSD();
        if (App.Settings.Enable3DMode)
        {
            btn3DLeft.Visible = true;
            btn3DRight.Visible = true;
        }
        else
        {
            btn3DLeft.Visible = true;
            btnFullScreenLeft.Visible = true;
        }
    }

    ToolTip tip = new ToolTip();
    private void ProgressBar_MouseLeave(object? sender, EventArgs e)
    {
        tip.Hide((ProgressBar)sender);
    }

    int brvalue;
    private void ProgressBar_MouseMove(object? sender, MouseEventArgs e)
    {
        brvalue = (int)((double)e.X / progressBarLeft.Width * progressBarLeft.Maximum);
        tip.Show(TimeSpan.FromSeconds(brvalue).ToString(@"hh\:mm\:ss"), progressBarLeft, e.Location.X + 1 + Screen.FromControl((ProgressBar)sender).Bounds.Left, e.Location.Y - 40);
    }

    private void overlayPanel_DragDrop(object? sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files.Length > 0)
        {
            try
            {
                hhzMainPage.Visible = false;
                overlayPanel.Visible = true;
                CursorTimer.Enabled = true;
                Player.LoadFiles(files, true, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("播放失败: " + ex.Message);
            }
        }
    }

    private void OverlayPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy; // 显示允许放下
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    void setVSR()
    {
        vw = Player.GetPropertyInt("width");
        vh = Player.GetPropertyInt("height");
        if (vw <= 1920 && vh <= 1080)
        {
            switch (hhzSettingsManager.Current.RenderText)
            {
                case "2D渲染器":
                    chkVSRLeft.Enabled = true;
                    chkVSRRight.Enabled = true;
                    lblVSRLeft.Visible = false;
                    lblVSRRight.Visible = false;
                    if (hhzSettingsManager.Current.VSR)
                    {
                        if (!bRTXOn()) Player.Command($"no-osd vf add d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
                    }
                    else
                    {
                        if (bRTXOn()) Player.Command($"no-osd vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
                    }
                    break;
                case "3D渲染器":
                    chkVSRLeft.Enabled = false; //3D模式下超分无效
                    chkVSRRight.Enabled = false;
                    lblVSRLeft.Text = "3D渲染下无效";
                    lblVSRRight.Text = "3D渲染下无效";
                    lblVSRLeft.Visible = true;
                    lblVSRRight.Visible = true;
                    if (bRTXOn()) Player.Command($"no-osd vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
                    break;
            }
            chkVSRLeft.Checked = hhzSettingsManager.Current.VSR;
            chkVSRRight.Checked = hhzSettingsManager.Current.VSR;
        }
        else
        {
            chkVSRLeft.Enabled = false;
            chkVSRRight.Enabled = false;
            chkVSRLeft.Checked = false;
            chkVSRRight.Checked = false;
            lblVSRLeft.Text = "超过1080p";
            lblVSRRight.Text = "超过1080p";
            lblVSRLeft.Visible = true;
            lblVSRRight.Visible = true;
        }
    }

    void setRender(string RenderText)
    {
        bSetRender = true;
        switch (RenderText)
        {
            case "2D渲染器":                
                btnRenderLeft.Text = "2D渲染器";
                Player.SetPropertyString("vo", "gpu-next");
                Player.SetPropertyString("hwdec", "auto-copy");
                Player.SetPropertyString("gpu-api", "auto");

                //Player.SetPropertyString("vo", "gpu-next");
                //Player.SetPropertyString("gpu-api", "d3d11");

                cmbVO.Text = "gpu-next";
                cmbhwdec.Text = "auto-copy";
                cmbgpuapi.Text = "auto";
                cmbGpuContext.Text = "auto";
                //setVSR();
                break;
            case "3D渲染器":
                btnRenderLeft.Text = "3D渲染器";
                //setVSR();
                Player.SetPropertyString("vo", "gpu");
                Player.SetPropertyString("hwdec", "auto");
                Player.SetPropertyString("gpu-api", "opengl");
                cmbVO.Text = "gpu";
                cmbhwdec.Text = "auto";
                cmbgpuapi.Text = "opengl";
                cmbGpuContext.Text = "auto";
                break;
            default:
                break;
        }
        bSetRender = false;
    }

    private void BtnRender_Click(object? sender, EventArgs e)
    {
        //Player.SetPropertyString("video-aspect-override", "32:9");
        if (hhzSettingsManager.Current.RenderText == "3D渲染器")
        {
            hhzSettingsManager.Current.RenderText = "2D渲染器";
        }
        else
        {
            hhzSettingsManager.Current.RenderText = "3D渲染器";
        }
        btnRenderLeft.Text = hhzSettingsManager.Current.RenderText;
        btnRenderRight.Text = hhzSettingsManager.Current.RenderText;
        setRender(hhzSettingsManager.Current.RenderText);
    }

    private void BtnFullScreen_Click(object? sender, EventArgs e)
    {
        Logger.Info($"全屏-{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        if (!App.Settings.Enable3DMode)
        {
            if (this.FormBorderStyle == FormBorderStyle.None)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                Bounds = new Rectangle(App.Settings.WindowLocation.X, App.Settings.WindowLocation.Y,
                                        App.Settings.WindowSize.Width, App.Settings.WindowSize.Height);
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            App.Settings.FormBorderStyle = GetAppFormBorderStyle(FormBorderStyle);
            App.Settings.WindowsStatus = GetAppWindowsStatus(WindowState);
            ShowVideoOSD();
        }
    }

    private void BtnPlay_Click(object? sender, EventArgs e)
    {
        Player.Command("cycle pause");
        _lastCursorChanged = Environment.TickCount;

    }
    private void BtnSubtitleTrack_Click(object? sender, EventArgs e)
    {
        _subMenuLeft?.Show(MousePosition);
    }

    private void BtnAudioTrack_Click(object? sender, EventArgs e)
    {
        _audioMenuLeft?.Show(MousePosition);
    }

    private void BtnVideoTrack_Click(object? sender, EventArgs e)
    {
        _videoMenuLeft?.Show(MousePosition);
    }

    private void ProgressBar_MouseClick(object? sender, MouseEventArgs e)
    {
        // 根据点击位置计算百分比
        double percent = (double)e.X / progressBarLeft.Width;
        //int value = (int)(percent * (progressBar.Maximum - progressBar.Minimum));
        //progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(value, progressBar.Maximum));

        // 跳转到对应时间
        //double duration = Player.GetPropertyDouble("duration");
        double target = percent * Player.Duration.TotalSeconds;
        Player.SetPropertyDouble("time-pos", target);
        _lastCursorChanged = Environment.TickCount;
    }

    private void btn3D_Click(object? sender, EventArgs e)
    {
        App.Settings.Enable3DMode = !App.Settings.Enable3DMode;
        CycleFullScreenFor3D(App.Settings.Enable3DMode);
        _lastCursorChanged = Environment.TickCount;
    }

    private void btnBack_Click(object? sender, EventArgs e)
    {

        if (hhzMainPage.Visible != true)
        {
            fps = 0;            
            Player.Command("stop");
            Player.Command("set pause yes");
            if (bRifeOn()) Player.Command($"no-osd vf remove vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");            
            if (bRTXOn()) Player.Command($"no-osd vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
            hhzMainPage.Visible = true;
            overlayPanel.Visible = false;
            CursorTimer.Enabled = false;
            ShowCursor();
            HideVideoOSD();
            btn3DLeft.Visible = true;
            btnFullScreenLeft.Visible = true;
            lblRifeLeft.Visible = false;
            lblRifeRight.Visible = false;
            lblVSRLeft.Visible = false;
            lblVSRRight.Visible = false;
            bProgSet = true;
            chkRifeLeft.Checked = true;
            chkRifeRight.Checked = true;
            bProgSet = false;
            if (hhzSettingsManager.Current.FromLastPosPlay) hhzSettingsManager.Current.LastTimePos = Player.GetPropertyDouble("time-pos");
        }        
    }

    private void OverlayPanel_MouseClick(object? sender, MouseEventArgs e)
    {
        //if (e.Button == MouseButtons.Left)
        //{
        //    isDoubleClick = false; // 重置状态
        //    clickTimer.Stop();
        //    clickTimer.Start(); // 启动延时
        //}
    }

    private void OverlayPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e == null || e.Button == MouseButtons.Left)
        {
            //isDoubleClick = true;            
            //clickTimer.Stop(); //双击发生，取消单击逻辑  // 👉 在这里写双击逻辑
            //bPressEnter = false;
            if (!App.Settings.Enable3DMode && (e == null || e.Button == MouseButtons.Left))
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
                App.Settings.FormBorderStyle = GetAppFormBorderStyle(FormBorderStyle);
                App.Settings.WindowsStatus = GetAppWindowsStatus(WindowState);
                ShowCursor();
                ShowVideoOSD();
            }
        }
    }

    private void OverlayPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        //if (IsCursorPosDifferent(_lastCursorPosition))
        if (_lastCursorPosition != MousePosition /*&& ClientRectangle.Contains(PointToClient(MousePosition)) && ActiveForm == this*/)
        {
            //Debug.Print($"{DateTime.Now.ToString()}-MouseMoveShowCursor");
            if (btn3DLeft.Visible == false || progressBarLeft.Visible == false)
            {
                ShowCursor();
                ShowVideoOSD();
            }
        }
        //SaveWindowProperties();
        base.OnMouseMove(e);
        if (App.Settings.Enable3DMode == false && /*IsCursorPosDifferent(_mouseDownLocation) &&*/
            WindowState == FormWindowState.Normal &&
            e.Button == MouseButtons.Left && /*!IsMouseInOsc() &&*/
            Player.GetPropertyBool("window-dragging"))
        {
            var HTCAPTION = new IntPtr(2);
            var WM_NCLBUTTONDOWN = 0xA1;
            ReleaseCapture();
            PostMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
        }
    }

    private void HhzMainPage_FileOpened(HHZMainPage sender, string[] paths)
    {
        if (paths.Length > 0)
        {
            //Player.Destroy();

            //InitPlayer();
            progressBarLeft.Value = 0;
            progressBarRight.Value = 0;

            lblDurationLeft.Text = "00:00:00 / 00:00:00";
            lblDurationRight.Text = lblDurationLeft.Text;
            lblStatusLeft.Text = "正在加载...";
            lblStatusRight.Text = lblStatusLeft.Text;

            hhzMainPage.Visible = false;
            overlayPanel.Visible = true;
            CursorTimer.Enabled = true;

            //Player?.SetPropertyDouble("time-pos", 0);
            Player?.Command("stop");
            Player?.Command("set pause yes");

            if (bRifeOn()) Player?.Command($"no-osd vf remove vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
            if (bRTXOn()) Player?.Command($"no-osd vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");

            string newPath = $"{paths[0]}.hhz";
            hhzSettingsManager.Load(newPath);
            Set3DSubtitleMode(hhzSettingsManager.Current.SubtitleMode);
            setRender(hhzSettingsManager.Current.RenderText);
            bSetRender = false;
            Player?.LoadFiles(paths, true, false);

            if (FileTypes.IsAudio(Path.GetExtension(paths[0]).Replace(".", ""))/*Player.Path.Ext())*/) //音频格式
            {
                isAudio = true;
                overlayPanel.BackgroundImage = LoadMyLogo();
                overlayPanel.BackgroundImageLayout = ImageLayout.Center;
                ShowAudioUI();
                //Player.SetPropertyString("audio-display", "visualizer");
                //Player.SetPropertyString("audio-display", "waveform");
                //Player.Command("af add lavfi=[showspectrum=s=1280x720:mode=combined:color=intensity]");
                //Player.Command("af add lavfi=[showwaves=s=1280x720:mode=cline]");
            }
            else
            {
                isAudio = false;
                overlayPanel.BackgroundImage = null;
            }
            if (App.Settings.Enable3DMode)
            {
                btn3DLeft.Visible = false;
                btn3DRight.Visible = false;
            }
            else
            {
                btn3DLeft.Visible = false;
                btnFullScreenLeft.Visible = false;
            }
        }
    }

    private void HhzMainPage_FileDropped(object? sender, string[] files)
    {
        if (files != null && files.Length > 0)
        {
            HhzMainPage_FileOpened(null, files);
        }
    }

    void UpdateDarkMode()
    {
        if (Environment.OSVersion.Version >= new Version(10, 0, 18985))
            DwmSetWindowAttribute(Handle, 20, new[] { Theme.DarkMode ? 1 : 0 }, 4);  // DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    private Panel overlayPanel;
    void set3DFullHalf()
    {
        if (Player.Duration.TotalMicroseconds > 0 && (hhzSettingsManager.Current.VideoAspestW == "0" && hhzSettingsManager.Current.VideoAspestH == "0"))
        {
            vw = Player.GetPropertyInt("width");
            vh = Player.GetPropertyInt("height");
            //FullSBS画面比例最小值为2.35 * 2 : 1
            if ((double)vw / vh < 2.35 / 1) // half-SBS
            {
                if ((Width / Height) <= 16.00 / 9)
                {
                    //One Screen
                    Player.SetPropertyString("video-aspect-override", $"{Width}:{(Width / 2) / (vw / 2) * vh}");
                }
                else
                {
                    //Two Screen
                    Player.SetPropertyString("video-aspect-override", $"{Width * 2}:{(Width / 2) / (vw / 2) * vh}");
                }
            }
            else // full-SBS
            {
                if ((Width / Height) <= 16.00 / 9)
                {
                    //One Screen
                    Player.SetPropertyString("video-aspect-override", $"{Width}:{Width / (vw / 2) * vh}");
                }
                else
                {
                    if (vw > 0 && vh > 0)
                    //Two Screen
                    Player.SetPropertyString("video-aspect-override", $"{Width}:{Width / vw * vh}");
                    //Player.SetPropertyString("video-aspect-override", $"{Width}:{vh}");
                }
            }
        }
    }

    private void CycleFullScreenFor3D(bool enable3DMode)
    {
        if (enable3DMode)
        {
            if (FormBorderStyle != FormBorderStyle.None) FormBorderStyle = FormBorderStyle.None;
            if (WindowState != FormWindowState.Normal) WindowState = FormWindowState.Normal;

            var primary = Screen.PrimaryScreen;

            // 主屏幕的右边界 X
            int rightEdge = primary.Bounds.Right;

            // 查找有没有屏幕的左边界正好贴在主屏幕右侧
            var rightScreen = Screen.AllScreens
                .FirstOrDefault(s => s.Bounds.Left == rightEdge);

            if (rightScreen != null)
            {
                //Debug.WriteLine($"右边有扩展屏幕：{rightScreen.Bounds}");
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                bounds.Width = bounds.Width * 2;
                this.Bounds = bounds;
            }
            else
            {
                //Debug.WriteLine("右边没有扩展屏幕");
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }

            if (!isAudio && !hhzMainPage.Visible) set3DFullHalf();
            //HideVideoUI();
            ShowVideoOSD();
        }
        else
        {
            if (!isAudio) setDefaultAspect();
            _isReturn2D = true;
            switch (App.Settings.FormBorderStyle)
            {
                case enumFormBorderStyle.Sizable:
                    FormBorderStyle = FormBorderStyle.Sizable;
                    break;
                case enumFormBorderStyle.None:
                    //if (FormBorderStyle != FormBorderStyle.None)
                    //FormBorderStyle = FormBorderStyle.None;
                    break;
            }
            switch (App.Settings.WindowsStatus)
            {
                case enumWindowsStatus.Normal:
                    WindowState = FormWindowState.Normal;
                    isDefault();
                    Bounds = new Rectangle(App.Settings.WindowLocation.X, App.Settings.WindowLocation.Y,
                       App.Settings.WindowSize.Width, App.Settings.WindowSize.Height);
                    break;
                case enumWindowsStatus.Maximized:
                    if (App.Settings.FormBorderStyle != enumFormBorderStyle.None)
                    {
                        appScreen = Screen.FromPoint(new Point(App.Settings.WindowLocation.X + App.Settings.WindowSize.Width / 2, App.Settings.WindowLocation.Y + App.Settings.WindowSize.Height / 2));
                        Bounds = appScreen.Bounds;
                    }
                    else
                    {
                        Bounds = new Rectangle(App.Settings.WindowLocation.X, App.Settings.WindowLocation.Y,
                                            App.Settings.WindowSize.Width, App.Settings.WindowSize.Height);
                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Maximized;
                    }
                    break;
                case enumWindowsStatus.Minimized:
                    WindowState = FormWindowState.Minimized;
                    break;
            }
            //v3DSubtitleMode.Sub2D(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
            _isReturn2D = false;
            ShowVideoOSD();
            //HideVideoUI();
        }
    }

    private void isDefault()
    {
        if (App.Settings.WindowLocation.X == 0 && App.Settings.WindowLocation.Y == 0 && App.Settings.WindowSize.Width == 0 && App.Settings.WindowSize.Height == 0)
        {
            App.Settings.WindowLocation.X = (int)(Screen.FromControl(this).Bounds.Width - Screen.FromControl(this).Bounds.Width * 0.8) / 2;
            App.Settings.WindowLocation.Y = (int)(Screen.FromControl(this).Bounds.Height - Screen.FromControl(this).Bounds.Height * 0.8) / 2;
            App.Settings.WindowSize.Width = (int)(Screen.FromControl(this).Bounds.Width * 0.8);
            App.Settings.WindowSize.Height = (int)(Screen.FromControl(this).Bounds.Height * 0.8);
        }
    }

    Screen appScreen;

    private enumFormBorderStyle GetAppFormBorderStyle(FormBorderStyle formBorderStyle)
    {
        switch (formBorderStyle)
        {
            case FormBorderStyle.None:
                return enumFormBorderStyle.None;
            case FormBorderStyle.Sizable:
                return enumFormBorderStyle.Sizable;
            default:
                break;
        }
        return enumFormBorderStyle.other;
    }

    private AppSettings.enumWindowsStatus GetAppWindowsStatus(FormWindowState windowState)
    {
        switch (WindowState)
        {
            case FormWindowState.Normal:
                return enumWindowsStatus.Normal;
            case FormWindowState.Minimized:
                return enumWindowsStatus.Minimized;
            case FormWindowState.Maximized:
                return enumWindowsStatus.Maximized;
            default:
                break;
        }
        return enumWindowsStatus.Other;
    }

    public int GetHorizontalLocation(Screen screen)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
        Rectangle rect = new Rectangle(Left - workingArea.X, Top - workingArea.Y, Width, Height);

        if (workingArea.Width / (float)Width < 1.1)
            return 0;

        if (rect.X * 3 < workingArea.Width - rect.Right)
            return -1;

        if (rect.X > (workingArea.Width - rect.Right) * 3)
            return 1;

        return 0;
    }

    public int GetVerticalLocation(Screen screen)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
        Rectangle rect = new Rectangle(Left - workingArea.X, Top - workingArea.Y, Width, Height);

        if (workingArea.Height / (float)Height < 1.1)
            return 0;

        if (rect.Y * 3 < workingArea.Height - rect.Bottom)
            return -1;

        if (rect.Y > (workingArea.Height - rect.Bottom) * 3)
            return 1;

        return 0;
    }

    void SetTitleInternal()
    {
        if (Player.Path.Length > 0)
        {
            Text = $"hhzPlayer - {Player.Path.Replace("\\\\", "\\")}";
        }
        else
        {
            Text = "hhzPlayer";
        }
    }

    void SaveWindowProperties()
    {
        if (!App.Settings.Enable3DMode)
        {
            if (WindowState != FormWindowState.Minimized && WindowState != FormWindowState.Maximized
                && FormBorderStyle != FormBorderStyle.None && WasShown && !_isReturn2D)
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    App.Settings.WindowPosition = new Point(Left, Top);
                    App.Settings.WindowLocation = new Point(Left, Top);
                    App.Settings.WindowSize = new Size(Width, Height);

                }
                App.Settings.WindowsStatus = GetAppWindowsStatus(WindowState);
                App.Settings.FormBorderStyle = GetAppFormBorderStyle(FormBorderStyle);
            }
        }
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
    }

    double GetActualFps()
    {
        double fps = 0;
        try { fps = Player.GetPropertyDouble("estimated-vf-fps"); } catch { }
        if (fps <= 0) try { fps = Player.GetPropertyDouble("video-params/fps"); } catch { }
        if (fps <= 0) try { fps = Player.GetPropertyDouble("container-fps"); } catch { }
        return fps;
    }

    void CursorTimer_Tick(object sender, EventArgs e)
    {
        //Debug.Print($"{DateTime.Now.ToString()}-CursorPositionDiff: {_lastCursorPosition != MousePosition}");
        //Debug.Print($"CursorTimer:{Environment.TickCount - _lastCursorChanged > _cursorAutohide} ActiveForm:{ActiveForm == this} Duration:{Player.Duration.TotalMilliseconds > 0} Playing:{!Player.GetPropertyBool("pause")} MainPage:{hhzMainPage.Visible == false} MouseInWindws:{ClientRectangle.Contains(PointToClient(MousePosition))}" );
        //string state = Player.GetPropertyString("cache-buffering-state");
        //string state = Player.GetPropertyString("demuxer-cache-state");
        strstate = Player.GetPropertyString("demuxer-cache-state");
        if (strstate != "")
        {
            var state = DemuxerCacheParser.Parse(Player.GetPropertyString("demuxer-cache-state"));

            if (Player.Paused)
            {
                StreamingContextStates = $"{sHDR} 暂停中...";
            }
            else
            {
                StreamingContextStates = $"{sHDR} 播放中...  帧率:{GetActualFps():F2}帧/秒 ";
            }
            if (state.Underrun)
            {
                StreamingContextStates += "缓存不足";
            }
            else
            {
                StreamingContextStates += $"缓存:{state.CacheDuration:F2}秒-{(state.TotalBytes / 1024.0 / 1024 / state.CacheDuration):F2} MB/s";
            }
            lblStatusLeft.Text = StreamingContextStates;
            lblStatusRight.Text = lblStatusLeft.Text;
        }
        if (_lastCursorPosition != MousePosition && ClientRectangle.Contains(PointToClient(MousePosition))/*IsCursorPosDifferent(_lastCursorPosition)*/)
        {
            _lastCursorPosition = MousePosition;
            _lastCursorChanged = Environment.TickCount;
        }
        else if (Environment.TickCount - _lastCursorChanged > _cursorAutohide
            && !(lblStatusLeft.Text?.StartsWith("正在") ?? false)
            && (!Player.GetPropertyBool("pause") || Player.Duration.TotalSeconds == 0)
            && hhzMainPage.Visible == false)
        {
            if ((_videoMenuLeft != null && _audioMenuLeft != null && _subMenuLeft != null) && (_videoMenuLeft.Visible || _audioMenuLeft.Visible || _subMenuLeft.Visible))
                return;
            if (chkTestModeLeft.Checked || chkTestModeRight.Checked)
                return;
            if (btn3DLeft.Visible == true || progressBarLeft.Visible == true/*ClientRectangle.Contains(PointToClient(MousePosition)) && ActiveForm == this && !ContextMenu.IsVisible && !IsMouseInOsc()*/)
            {
                HideCursor();
                HideVideoOSD();
                Debug.Print($"{DateTime.Now.ToString()}-HideCursor");
            }
        }
    }

    void SetProgressBarMax(double Maximum)
    {
        progressBarLeft.Maximum = (int)Maximum;
        if (App.Settings.Enable3DMode) progressBarRight.Maximum = (int)Maximum;
    }
    void SetProgressBarValue(double value)
    {
        progressBarLeft.Value = (int)value;
        if (App.Settings.Enable3DMode) progressBarRight.Value = (int)value;
    }

    // 三个菜单字段
    private ContextMenuStrip _videoMenuLeft;
    private ContextMenuStrip _audioMenuLeft;
    private ContextMenuStrip _subMenuLeft;
    private ContextMenuStrip _videoMenuRight;
    private ContextMenuStrip _audioMenuRight;
    private ContextMenuStrip _subMenuRight;

    /// <summary>
    /// 一次性刷新并构建：视频 / 音频 / 字幕 三个菜单（详细信息版）
    /// </summary>
    private void BuildAllTrackMenus()
    {
        _videoMenuLeft = new ContextMenuStrip();
        _audioMenuLeft = new ContextMenuStrip();
        _subMenuLeft = new ContextMenuStrip();
        _videoMenuRight = new ContextMenuStrip();
        _audioMenuRight = new ContextMenuStrip();
        _subMenuRight = new ContextMenuStrip();

        string json = Player.GetPropertyString("track-list");
        if (string.IsNullOrEmpty(json)) return;

        // 读取当前选择，用于“无”选项的打勾
        string curVid = Player.GetPropertyString("vid");
        string curAid = Player.GetPropertyString("aid");
        string curSid = Player.GetPropertyString("sid");

        using var doc = JsonDocument.Parse(json);
        foreach (var track in doc.RootElement.EnumerateArray())
        {
            string type = track.GetProperty("type").GetString();
            int id = track.GetProperty("id").GetInt32();
            bool selected = track.TryGetProperty("selected", out var s) && s.GetBoolean();

            string lang = track.TryGetProperty("lang", out var l) ? l.GetString() : null;
            string codec = track.TryGetProperty("codec", out var c) ? c.GetString() : null;
            string profile = track.TryGetProperty("codec-profile", out var p) ? p.GetString() : null;
            bool isDefault = track.TryGetProperty("default", out var d) && d.GetBoolean();

            string label;

            string title = track.TryGetProperty("title", out var t) ? t.GetString() : null;
            string langDisplay = MapLang(lang, title);

            if (type == "video")
            {
                string w = track.TryGetProperty("demux-w", out var ww) ? ww.GetRawText() : "?";
                string h = track.TryGetProperty("demux-h", out var hh) ? hh.GetRawText() : "?";
                string fps = track.TryGetProperty("demux-fps", out var f) ? TryFormatDouble(f, "0.##") : "?";
                long bps = ReadBitrateBps(track);
                string br = FormatBitrate(bps, preferMbForVideo: true);

                string codecPart = string.IsNullOrEmpty(profile) ? codec : $"{codec}, {profile}";
                label = $"视频: {codecPart}, {w}x{h}, {br}, {fps} FPS";
            }
            else if (type == "audio")
            {
                string ch = track.TryGetProperty("demux-channel-count", out var cc) ? $"{cc.GetInt32()} ch" : "?";
                string rate = track.TryGetProperty("demux-samplerate", out var sr) ? $"{sr.GetInt32() / 1000.0:0.0} kHz" : "?";
                long bps = ReadBitrateBps(track);
                string br = FormatBitrate(bps, preferMbForVideo: false);

                label = $"音频: {langDisplay}, {codec}, {br}, {ch}, {rate}";
                if (isDefault) label += ", Default";
            }
            else // sub
            {
                label = $"字幕: {langDisplay}, {codec}";
                if (isDefault) label += ", Default";
            }
            Debug.Print(track.GetRawText());
            var iteml = new ToolStripMenuItem(label)
            {
                Tag = id,
                Checked = selected,
                CheckOnClick = false
            };
            var itemr = new ToolStripMenuItem(label)
            {
                Tag = id,
                Checked = selected,
                CheckOnClick = false
            };

            iteml.Click += (_, __) =>
            {
                switch (type)
                {
                    case "video":
                        Player.SetPropertyString("vid", id.ToString());
                        hhzSettingsManager.Current.LastVideoTrackId = id;
                        break;
                    case "audio":
                        Player.SetPropertyString("aid", id.ToString());
                        hhzSettingsManager.Current.LastAudioTrackId = id;
                        break;
                    case "sub":
                        Player.SetPropertyString("sid", id.ToString());
                        hhzSettingsManager.Current.LastSubtitleTrackId = id;
                        break;
                }
                BuildAllTrackMenus(); // 刷新勾选状态
            };
            itemr.Click += (_, __) =>
            {
                switch (type)
                {
                    case "video":
                        Player.SetPropertyString("vid", id.ToString());
                        hhzSettingsManager.Current.LastVideoTrackId = id;
                        break;
                    case "audio":
                        Player.SetPropertyString("aid", id.ToString());
                        hhzSettingsManager.Current.LastAudioTrackId = id;
                        break;
                    case "sub":
                        Player.SetPropertyString("sid", id.ToString());
                        hhzSettingsManager.Current.LastSubtitleTrackId = id;
                        break;
                }
                BuildAllTrackMenus(); // 刷新勾选状态
            };

            switch (type)
            {
                case "video": _videoMenuLeft.Items.Add(iteml); _videoMenuRight.Items.Add(itemr); break;
                case "audio": _audioMenuLeft.Items.Add(iteml); _audioMenuRight.Items.Add(itemr); break;
                case "sub": _subMenuLeft.Items.Add(iteml); _subMenuRight.Items.Add(itemr); break;
            }
        }

        // --- “无”选项（要根据 vid/aid/sid == "no" 来勾选） ---
        _videoMenuLeft.Items.Add(new ToolStripSeparator());
        _videoMenuRight.Items.Add(new ToolStripSeparator());
        var noVidl = new ToolStripMenuItem("视频: 无视频")
        {
            Checked = curVid == "no",
            CheckOnClick = false
        };
        var noVidr = new ToolStripMenuItem("视频: 无视频")
        {
            Checked = curVid == "no",
            CheckOnClick = false
        };
        noVidl.Click += (_, __) => { Player.SetPropertyString("vid", "no"); BuildAllTrackMenus(); };
        _videoMenuLeft.Items.Add(noVidl);
        noVidr.Click += (_, __) => { Player.SetPropertyString("vid", "no"); BuildAllTrackMenus(); };
        _videoMenuRight.Items.Add(noVidr);

        _videoMenuLeft.Items.Add(new ToolStripSeparator());
        _videoMenuRight.Items.Add(new ToolStripSeparator());
        var defVidl = new ToolStripMenuItem("恢复默认")
        {
            Text = $"恢复自动(目前:{getcurrentVideoTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        var defVidr = new ToolStripMenuItem("恢复默认")
        {
            Text = $"恢复自动(目前:{getcurrentVideoTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        defVidl.Click += (_, __) => { hhzSettingsManager.Current.LastVideoTrackId = -1; Player.SetPropertyString("vid", AutoVideoid); BuildAllTrackMenus(); };
        _videoMenuLeft.Items.Add(defVidl);
        defVidr.Click += (_, __) => { hhzSettingsManager.Current.LastVideoTrackId = -1; Player.SetPropertyString("vid", AutoVideoid); BuildAllTrackMenus(); };
        _videoMenuRight.Items.Add(defVidr);

        _videoMenuLeft.Items.Add(new ToolStripSeparator());
        _videoMenuRight.Items.Add(new ToolStripSeparator());
        Videoaspectl = new ToolStripMenuItem("强制播放比例")
        {
            Checked = curVid == "no",
            CheckOnClick = false
        };
        Videoaspectr = new ToolStripMenuItem("强制播放比例")
        {
            //Checked = curVid == "no",
            CheckOnClick = false
        };
        if (hhzSettingsManager.Current.VideoAspestW != "0" && hhzSettingsManager.Current.VideoAspestH != "0")
        {
            Videoaspectl.Text = $"播放比例: {hhzSettingsManager.Current.VideoAspestW}:{hhzSettingsManager.Current.VideoAspestH}";
            Videoaspectr.Text = $"播放比例: {hhzSettingsManager.Current.VideoAspestW}:{hhzSettingsManager.Current.VideoAspestH}";
            Videoaspectl.Checked = true;
        }
        else
        {
            Videoaspectl.Text = $"播放比例: 默认";
            Videoaspectr.Text = $"播放比例: 默认";
            Videoaspectl.Checked = false;
        }
        Videoaspectl.Click += Videoaspectl_Click;
        _videoMenuLeft.Items.Add(Videoaspectl);
        Videoaspectr.Click += Videoaspectl_Click;
        _videoMenuRight.Items.Add(Videoaspectr);

        _audioMenuLeft.Items.Add(new ToolStripSeparator());
        _audioMenuRight.Items.Add(new ToolStripSeparator());
        var noAidl = new ToolStripMenuItem("音频: 无音频")
        {
            Checked = curAid == "no",
            CheckOnClick = false
        };
        var noAidr = new ToolStripMenuItem("音频: 无音频")
        {
            Checked = curAid == "no",
            CheckOnClick = false
        };
        noAidl.Click += (_, __) => { Player.SetPropertyString("aid", "no"); BuildAllTrackMenus(); };
        _audioMenuLeft.Items.Add(noAidl);
        noAidr.Click += (_, __) => { Player.SetPropertyString("aid", "no"); BuildAllTrackMenus(); };
        _audioMenuRight.Items.Add(noAidr);

        _audioMenuLeft.Items.Add(new ToolStripSeparator());
        _audioMenuRight.Items.Add(new ToolStripSeparator());
        
        var defAidl = new ToolStripMenuItem("恢复默认")
        {            
            Text = $"恢复自动(目前:{getcurrentAudioTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        var defAidr = new ToolStripMenuItem("恢复默认")
        {
            Text = $"恢复自动(目前:{getcurrentAudioTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        defAidl.Click += (_, __) => { hhzSettingsManager.Current.LastAudioTrackId = -1; Player.SetPropertyString("aid", AutoAudioid); BuildAllTrackMenus(); };
        _audioMenuLeft.Items.Add(defAidl);
        defAidr.Click += (_, __) => { hhzSettingsManager.Current.LastAudioTrackId = -1; Player.SetPropertyString("aid", AutoAudioid); BuildAllTrackMenus(); };
        _audioMenuRight.Items.Add(defAidr);

        _subMenuLeft.Items.Add(new ToolStripSeparator());
        _subMenuRight.Items.Add(new ToolStripSeparator());
        var noSubl = new ToolStripMenuItem("字幕: 无字幕")
        {
            Checked = curSid == "no",
            CheckOnClick = false
        };
        var noSubr = new ToolStripMenuItem("字幕: 无字幕")
        {
            Checked = curSid == "no",
            CheckOnClick = false
        };
        noSubl.Click += (_, __) => { Player.SetPropertyString("sid", "no"); BuildAllTrackMenus(); };
        _subMenuLeft.Items.Add(noSubl);
        noSubr.Click += (_, __) => { Player.SetPropertyString("sid", "no"); BuildAllTrackMenus(); };
        _subMenuRight.Items.Add(noSubr);

        var defSubl = new ToolStripMenuItem("恢复默认")
        {
            Text = $"恢复自动(目前:{getcurrentSubtitleTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        var defSubr = new ToolStripMenuItem("恢复默认")
        {
            Text = $"恢复自动(目前:{getcurrentSubtitleTrack()})",
            Checked = false,
            CheckOnClick = false
        };
        defSubl.Click += (_, __) => { hhzSettingsManager.Current.LastSubtitleTrackId = -1; Player.SetPropertyString("sid", AutoSubtitleid); BuildAllTrackMenus(); };
        _subMenuLeft.Items.Add(defSubl);
        defSubr.Click += (_, __) => { hhzSettingsManager.Current.LastSubtitleTrackId = -1; Player.SetPropertyString("sid", AutoSubtitleid); BuildAllTrackMenus(); };
        _subMenuRight.Items.Add(defSubr);
    }

    private object getcurrentSubtitleTrack()
    {
        if (hhzSettingsManager.Current.LastSubtitleTrackId == -1)
        {
            return "自动";
        }
        else
        {
            foreach (var track in _subMenuLeft.Items)
            {
                if (track is ToolStripMenuItem item && item.Checked)
                {
                    return item.Text;
                }
            }
            return null;
        }
    }

    private object getcurrentVideoTrack()
    {
        if (hhzSettingsManager.Current.LastVideoTrackId == -1)
        {
            return "自动";
        }
        else
        {
            foreach (var track in _videoMenuLeft.Items)
            {
                if (track is ToolStripMenuItem item && item.Checked)
                {
                    return item.Text;
                }
            }
            return null;
        }
    }

    string getcurrentSubtitleTrackId()
    {
        foreach (var track in _subMenuLeft.Items)
        {
            if (track is ToolStripMenuItem item && item.Checked)
            {
                return item.Tag == null ? "" : item.Tag.ToString();
            }
        }
        return null;
    }

    string getcurrentVideoTrackId()
    {
        foreach (var track in _videoMenuLeft.Items)
        {
            if (track is ToolStripMenuItem item && item.Checked)
            {
                return item.Tag == null ? "" : item.Tag.ToString();
            }
        }
        return null;
    }

    string getcurrentAudioTrackId()
    {
        foreach (var track in _audioMenuLeft.Items)
        {
            if (track is ToolStripMenuItem item && item.Checked)
            {
                return item.Tag == null ? "" : item.Tag.ToString();
            }
        }
        return null;
    }

    string getcurrentAudioTrack()
    {
        if (hhzSettingsManager.Current.LastAudioTrackId == -1)
        {
            return "自动";
        } 
        else
        {
            foreach (var track in _audioMenuLeft.Items)
            {
                if (track is ToolStripMenuItem item && item.Checked)
                {
                    return item.Text;
                }
            }
            return null;
        }
    }

    FormMediaProperty frmMediaProperty;
    private void Videoaspectl_Click(object? sender, EventArgs e)
    {
        if (frmMediaProperty.Visible != true) frmMediaProperty.Show();
        //Player.SetPropertyString("video-aspect-override", "4:3");
    }
    // ======== 帮助函数 ========

    // 读取码率（bps）。优先 demux-bitrate（number），退回 metadata.BPS（string，单位 bps）
    private static long ReadBitrateBps(JsonElement track)
    {
        // 1) demux-bitrate（有些封装会提供）
        if (track.TryGetProperty("demux-bitrate", out var db) && db.ValueKind == JsonValueKind.Number)
        {
            if (db.TryGetInt64(out long v) && v > 0) return v;
            if (db.TryGetDouble(out double dv) && dv > 0) return (long)dv;
        }

        // 2) metadata.BPS（字符串，单位 bps）
        if (track.TryGetProperty("metadata", out var meta) && meta.ValueKind == JsonValueKind.Object)
        {
            if (meta.TryGetProperty("BPS", out var bpsProp) && bpsProp.ValueKind == JsonValueKind.String)
            {
                var s = bpsProp.GetString();
                if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out long bps) && bps > 0)
                    return bps;
            }
        }

        return 0;
    }

    // 将 bps 格式化成 "33.6 Mb/s" 或 "4,134 kb/s"
    private static string FormatBitrate(long bps, bool preferMbForVideo)
    {
        if (bps <= 0) return "?";

        if (preferMbForVideo)
        {
            // 视频：Mb/s，保留 1 位小数
            double mbs = bps / 1_000_000.0;
            return $"{mbs:0.0} Mb/s";
        }
        else
        {
            // 音频：kb/s，千分位空格
            int kbps = (int)Math.Round(bps / 1000.0);
            string grouped = string.Format(CultureInfo.InvariantCulture, "{0:N0}", kbps);
            return $"{grouped} kb/s";
        }
    }

    // 读取 double 的文本并格式化
    private static string TryFormatDouble(JsonElement numElem, string fmt)
    {
        if (numElem.ValueKind == JsonValueKind.Number)
        {
            if (numElem.TryGetDouble(out double v))
                return v.ToString(fmt, CultureInfo.InvariantCulture);
        }
        return "?";
    }
    // ======== 语言/标题映射 ========
    private static string MapLang(string lang, string title)
    {
        string src = !string.IsNullOrEmpty(title) ? title : lang;
        if (string.IsNullOrEmpty(src)) return "未知";

        // 全部转大写，方便匹配
        src = src.ToUpperInvariant();

        // 如果是复合的（比如 CHS/ENG、chi+jpn），拆分处理
        var parts = src.Split(new[] { '/', '+', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);

        List<string> mapped = new();
        foreach (var p in parts)
        {
            switch (p.Trim())
            {
                // ====== 中文 ======
                case "CHS": mapped.Add("中文(简体)"); break;
                case "CHT": mapped.Add("中文(繁體)"); break;
                case "CHI": mapped.Add("中文"); break;
                case "ZHO": mapped.Add("中文"); break;
                case "ZH": mapped.Add("中文"); break;

                // ====== 英语 ======
                case "ENG": mapped.Add("英文"); break;
                case "EN": mapped.Add("英文"); break;

                // ====== 日语 / 韩语 ======
                case "JPN": mapped.Add("日文"); break;
                case "JP": mapped.Add("日文"); break;
                case "KOR": mapped.Add("韩文"); break;
                case "KO": mapped.Add("韩文"); break;

                // ====== 欧洲常见语言 ======
                case "FRA": case "FR": mapped.Add("法文"); break;
                case "DEU": case "GER": case "DE": mapped.Add("德文"); break;
                case "SPA": case "ES": mapped.Add("西班牙文"); break;
                case "ITA": case "IT": mapped.Add("意大利文"); break;
                case "POR": case "PT": mapped.Add("葡萄牙文"); break;
                case "RUS": case "RU": mapped.Add("俄文"); break;
                case "HUN": mapped.Add("匈牙利文"); break;
                case "TUR": mapped.Add("土耳其文"); break;
                case "ARA": mapped.Add("阿拉伯文"); break;
                case "THA": mapped.Add("泰文"); break;
                case "VIE": mapped.Add("越南文"); break;
                case "IND": case "ID": mapped.Add("印尼文"); break;

                // ====== 默认 ======
                default: mapped.Add(p); break;
            }
        }

        return string.Join("/", mapped);
    }
    private Image LoadMyLogo()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylogo.png");
        if (File.Exists(path)) return new Bitmap(path);
        var bmp = new Bitmap(1, 1);
        using (var g = Graphics.FromImage(bmp)) g.Clear(Color.Transparent);
        return bmp;
    }

    void ShowAudioUI()
    {
        if (App.Settings.Enable3DMode)
        {
            btnFullScreenLeft.Visible = false;
            btn3DLeft.Visible = true;
            btn3DRight.Visible = true;
            btn3DSubtitleModeLeft.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btnRenderLeft.Visible = false;
            btnRenderRight.Visible = false;
            btnBackLeft.Visible = true;
            btnBackRight.Visible = true;
            progressBarLeft.Visible = true;
            progressBarRight.Visible = true;
            btnVideoTrackLeft.Visible = true;
            btnVideoTrackRight.Visible = true;
            btnAudioTrackLeft.Visible = true;
            btnAudioTrackRight.Visible = true;
            btnSubtitleTrackLeft.Visible = true;
            btnSubtitleTrackRight.Visible = true;
            btnPlayLeft.Visible = true;
            btnPlayRight.Visible = true;
            lblDurationLeft.Visible = true;
            lblDurationRight.Visible = true;
            lblStatusLeft.Visible = true;
            lblStatusRight.Visible = true;
            lblVolumeLeft.Visible = true;
            lblVolumeRight.Visible = true;
        }
        else
        {
            btnFullScreenLeft.Visible = true;
            btn3DLeft.Visible = true;
            btn3DRight.Visible = false;
            btn3DSubtitleModeLeft.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btnRenderLeft.Visible = false;
            btnRenderRight.Visible = false;
            btnBackLeft.Visible = true;
            btnBackRight.Visible = false;
            progressBarLeft.Visible = true;
            progressBarRight.Visible = false;
            btnVideoTrackLeft.Visible = true;
            btnVideoTrackRight.Visible = false;
            btnAudioTrackLeft.Visible = true;
            btnAudioTrackRight.Visible = false;
            btnSubtitleTrackLeft.Visible = true;
            btnSubtitleTrackRight.Visible = false;
            btnPlayLeft.Visible = true;
            btnPlayRight.Visible = false;
            lblDurationLeft.Visible = true;
            lblDurationRight.Visible = false;
            lblStatusLeft.Visible = true;
            lblStatusRight.Visible = false;
            lblVolumeLeft.Visible = true;
            lblVolumeRight.Visible = false;
        }
    }

    void InterpOff()
    {
        Player.Command("vf remove @interp");
    }

    void InterpTo60()
    {
        Player.Command("vf remove @interp");
        Player.Command(@"vf add @interp:lavfi=[minterpolate=fps=60000/1001:mi_mode=mci:mc_mode=aobmc:me_mode=bidir]");
    }

    // 按倍数补帧：2× / 3×
    void InterpMul(int mul)
    {
        double src = GetActualFps();
        if (src <= 0) return;
        double target = src * mul;

        // 防止 23.976×3 变 71.928，想贴 60 的话，你也可以在这里把 target 量化到 59.94
        // target = 60000.0 / 1001;

        Player.Command("vf remove @interp");
        Player.Command($"vf add @interp:lavfi=[minterpolate=fps={target.ToString(CultureInfo.InvariantCulture)}:mi_mode=mci:mc_mode=aobmc:me_mode=bidir]");
    }

    void Player_FileLoaded()
    {
        BeginInvoke((Delegate)(() =>
        {
            //if (Player.Duration.TotalMilliseconds > 0)
            {
                string currfile = Player.GetPropertyString("filename");
                ShowToast(currfile, 2000);
                lblDurationLeft.Text = $"{TimeSpan.FromSeconds(0).ToString(@"hh\:mm\:ss")} / {TimeSpan.FromSeconds(Player.Duration.TotalSeconds).ToString(@"hh\:mm\:ss")}";
                lblDurationRight.Text = lblDurationLeft.Text;

                BuildAllTrackMenus();
                if (hhzSettingsManager.Current.LastVideoTrackId != -1)
                {
                    Player.SetPropertyString("vid", hhzSettingsManager.Current.LastVideoTrackId.ToString());
                }
                else
                {
                    AutoVideoid = getcurrentVideoTrackId();
                }
                if (hhzSettingsManager.Current.LastAudioTrackId != -1)
                {
                    Player.SetPropertyString("aid", hhzSettingsManager.Current.LastAudioTrackId.ToString());
                }
                else
                {
                    AutoAudioid = getcurrentAudioTrackId();
                }
                if (hhzSettingsManager.Current.LastSubtitleTrackId != -1)
                {
                    Player.SetPropertyString("sid", hhzSettingsManager.Current.LastSubtitleTrackId.ToString());
                }
                else
                {
                    AutoSubtitleid = getcurrentSubtitleTrackId();
                }
                if (hhzSettingsManager.Current.VideoAspestW != "0" && hhzSettingsManager.Current.VideoAspestH != "0") Player.SetPropertyString("video-aspect-override", $"{hhzSettingsManager.Current.VideoAspestW}:{hhzSettingsManager.Current.VideoAspestH}");

                hhzMainPage.Visible = false;
                //SetProgressBarMax(Player.Duration.TotalSeconds);
                if (App.Settings.Enable3DMode)
                {
                    set3DFullHalf();
                }
                else
                {
                    setDefaultAspect();
                }
                //Player.SetPropertyString("video-aspect-override", "7680:2072");
                //v3DSubtitleMode.Auto(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);

                SetTitleInternal();

                int interval = (int)(Player.Duration.TotalMilliseconds / 100);

                if (interval < 100)
                    interval = 100;

                if (interval > 1000)
                    interval = 1000;

                ProgressTimer.Interval = interval;

                if (!Player.GetPropertyBool("pause"))
                {
                    btnPlayLeft.Text = "暂停";
                    btnPlayRight.Text = "暂停";
                    lblStatusLeft.Text = "播放中";
                    lblStatusRight.Text = lblStatusLeft.Text;
                }
                else
                {
                    btnPlayLeft.Text = "播放";
                    btnPlayRight.Text = "播放";
                    lblStatusLeft.Text = "暂停中";
                    lblStatusRight.Text = lblStatusLeft.Text;
                }
                string path = Player.GetPropertyString("path");
                path = MainPlayer.ConvertFilePath(path);

                if (!string.IsNullOrEmpty(path) && path != "-" && path != @"bd://" && path != @"dvd://")
                {
                    if (App.Settings.RecentFiles.Contains(path))
                        App.Settings.RecentFiles.Remove(path);

                    App.Settings.RecentFiles.Insert(0, path);

                    while (App.Settings.RecentFiles.Count > App.RecentCount)
                        App.Settings.RecentFiles.RemoveAt(App.RecentCount);
                }
                //if (App.Settings.Enable3DMode)
                //{
                //    btnBackRight.Visible = true;
                //    btn3DSubtitleModeRight.Visible = true;
                //    btn3DRight.Visible = true;
                //    progressBarRight.Visible = true;
                //}
                //UpdateProgressBar();
                if (bPressPageDownUp)
                {
                    if (!Player.GetPropertyBool("pause")) Player.Command("cycle pause");
                    bPressPageDownUp = false;
                }
            }
            //bFileloaded = true;
            //Player.SetPropertyString("hwdec", "nvdec-copy");

            //Player.SetPropertyString("vo", "gpu-next");
            //Player.SetPropertyString("hwdec", "d3d11va");
            //Player.SetPropertyString("gpu-api", "d3d11");
            //Player.SetPropertyString("ao", "wasapi");
            //Player.SetPropertyString("af", "format=s16:48000");
            //Player.SetPropertyString("audio-exclusive", "no");
            //Player.SetPropertyString("audio-spdif", "yes");
            //Player.SetPropertyString("audio-channels", "auto");
            //Player.SetPropertyString("audio-format", "s16");
            //string vfList = Player.GetPropertyString("vf");
            //Debug.WriteLine(vfList);
            fps = Player.GetPropertyDouble("container-fps");
            if (fps <= 35)
            {
                lblRifeLeft.Visible = false;
                lblRifeRight.Visible = false;
                if (hhzSettingsManager.Current.Rife)
                {
                    chkRifeLeft.Enabled = true;
                    chkRifeLeft.Enabled = true;
                    bProgSet = true;
                    chkRifeLeft.Checked = true;
                    chkRifeRight.Checked = true;
                    bProgSet = false;
                    cbRifeTimesLeft.SelectedIndex = 0;
                    cbRifeTimesRight.SelectedIndex = 0;
                    cbRifeTimesLeft.Enabled = true;
                    cbRifeTimesRight.Enabled = true;
                    if (!bRifeOn())
                    {
                        //if (vw > 1920 || vh > 1080) Player.SetPropertyString("hwdec", "nvdec-copy");
                        //if (vw > 1920 || vh > 1080) Player.SetPropertyString("hwdec", "no");                        
                        Player.Command($"no-osd vf add vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                        //Player.Command($"no-osd vf add d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
                        //Player.Command($"vf add @interp:lavfi=[minterpolate=fps=60:mi_mode=mci:mc_mode=aobmc:me_mode=bidir]");
                        //InterpMul(3);
                    }
                }
                else
                {
                    bProgSet = true;
                    chkRifeLeft.Checked = false;
                    chkRifeRight.Checked = false;
                    bProgSet = false;
                }
                //if (vw <= 1920 && vh <= 1080)
                //{
                //    Player.Command($"no-osd vf set vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_1080p_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                //}
                //else
                //{
                //    Player.Command($"no-osd vf set vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_4k_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                //}
            }
            else
            {
                lblRifeLeft.Visible = true;
                lblRifeRight.Visible = true;
                gbRifeLeft.Visible = false;
                gbRifeRight.Visible = false;
                chkRifeLeft.Checked = false;
                chkRifeLeft.Checked = false;
                cbRifeTimesLeft.Enabled = false;
                cbRifeTimesRight.Enabled = false;
                chkRifeLeft.Enabled = false;
                chkRifeLeft.Enabled = false;
                if (bRifeOn())
                {
                    Player.Command($"no-osd vf remove vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                }
            }
            setVSR();
            sHDR = GetHdrType(Player);
            ShowVideoOSD();
            if (!hhzSettingsManager.Current.FromLastPosPlay)
            {
                Player.SetPropertyDouble("time-pos", 0);
            }
            else
            {
                if (hhzSettingsManager.Current.LastTimePos > 0 && hhzSettingsManager.Current.LastTimePos < Player.Duration.TotalSeconds)
                {
                    Player.SetPropertyDouble("time-pos", hhzSettingsManager.Current.LastTimePos);
                }
            }
            Player.Command("set pause no");
        }));

        string path = Player.GetPropertyString("path");

        path = MainPlayer.ConvertFilePath(path);

        if (path.Contains("://"))
        {
            string title = Player.GetPropertyString("media-title");

            if (!string.IsNullOrEmpty(title) && path != title)
                path = path + "|" + title;
        }

        if (!string.IsNullOrEmpty(path) && path != "-" && path != @"bd://" && path != @"dvd://")
        {
            if (App.Settings.RecentFiles.Contains(path))
                App.Settings.RecentFiles.Remove(path);

            App.Settings.RecentFiles.Insert(0, path);

            while (App.Settings.RecentFiles.Count > App.RecentCount)
                App.Settings.RecentFiles.RemoveAt(App.RecentCount);
        }

        //string subFile = Player.GetPropertyString("sub-files");
        //string offsetFile = "sub_3d.ass";
        //MakeOffsetSubtitle(subFile, offsetFile, /*videoWidth / 2*/100);

        //Player.CommandV("sub-add", offsetFile, "select");

        //Player.CommandV("script", "3dsub.lua");
        // 设置内封字幕轨道（例如轨道 ID=2）
        //Player.SetPropertyString("sid", "4");
        //Player.SetPropertyString("sid", "5");
        //Player.SetPropertyString("sid", "6");
        //Player.SetPropertyString("sid", "7");
        //Player.SetPropertyString("sid", "8");

        //var s = Player.GetPropertyString("track-list");

        //SetDefaultSubId();
    }

    private string sHDR;
    int vw;
    int vh;
    double fps;

    bool bRifeOn()
    {
        string vfList = Player.GetPropertyString("vf");
        if (vfList != "" && vfList.Contains("vapoursynth=file="))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void setDefaultAspect()
    {
        if (Player.Duration.TotalMicroseconds > 0 && (hhzSettingsManager.Current.VideoAspestW == "0" && hhzSettingsManager.Current.VideoAspestH == "0"))
        {
            Player.SetPropertyString("video-aspect-override", "0");
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (hhzMainPage.Visible == false)
        {
            double vol = Player.GetPropertyDouble("volume");

            if (e.Delta > 0)
            {
                vol = Math.Min(vol + 5, 150); // 向上滚动增加音量
            }
            else if (e.Delta < 0)
            {
                vol = Math.Max(vol - 5, 0);   // 向下滚动减少音量
            }
            ShowVolumeUI(vol);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (WindowState == FormWindowState.Maximized)
            //Player.SetPropertyBool("window-maximized", true);

        //WpfApplication.Init();
        //Theme.UpdateWpfColors();
        //MessageBoxEx.MessageForeground = Theme.Current?.GetBrush("heading");
        //MessageBoxEx.MessageBackground = Theme.Current?.GetBrush("background");
        //MessageBoxEx.ButtonBackground = Theme.Current?.GetBrush("highlight");
        //InitAndBuildContextMenu();
        Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y);
        GlobalHotkey.RegisterGlobalHotkeys(Handle);
        StrongReferenceMessenger.Default.Send(new MainWindowIsLoadedMessage());
        WasShown = true;
    }

    //void ContextMenu_Closed(object sender, System.Windows.RoutedEventArgs e) => MenuAutoResetEvent.Set();
    protected override void OnResize(EventArgs e)
    {

        base.OnResize(e);
        if (WasShown)
        {
            ShowCursor();
            ShowVideoOSD();
        }
        //HideVideoOSD();
        //if (Player.Duration.TotalMilliseconds > 0)
        {
            if (!App.Settings.Enable3DMode)
            {
                SaveWindowProperties();
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        //if (Player == null || !bPlayerinited)
        //    return;
        //if (Player.IsQuitNeeded)
        //    Player.CommandV("quit");

        //if (!Player.ShutdownAutoResetEvent.WaitOne(10000))
        //    Msg.ShowError(_("Shutdown thread failed to complete within 10 seconds."));

        //Player.Destroy();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        //OverlayPanel_MouseMove(null, e);
        base.OnMouseMove(e);
        if (App.Settings.Enable3DMode == false && /*IsCursorPosDifferent(_mouseDownLocation) &&*/
            WindowState == FormWindowState.Normal &&
            e.Button == MouseButtons.Left && /*!IsMouseInOsc() &&*/
            Player.GetPropertyBool("window-dragging"))
        {
            var HTCAPTION = new IntPtr(2);
            var WM_NCLBUTTONDOWN = 0xA1;
            ReleaseCapture();
            PostMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
        }
    }

    protected override void OnMove(EventArgs e)
    {
        if (!App.Settings.Enable3DMode)
        {
            SaveWindowProperties();
        }
        base.OnMove(e);
    }

    void ShowVolumeUI(double vol)
    {
        Player.SetPropertyDouble("volume", vol);
        App.Settings.Volume = (int)vol;
        lblVolumeLeft.Text = $"音量:{App.Settings.Volume}%";
        lblVolumeRight.Text = lblVolumeLeft.Text;

        if (App.Settings.Volume > 100)
        {
            lblVolumeLeft.ForeColor = Color.Red;
            lblVolumeRight.ForeColor = Color.Red;
        }
        else if (App.Settings.Volume != 0)
        {
            lblVolumeLeft.ForeColor = Color.White;
            lblVolumeRight.ForeColor = Color.White;
        }
        else
        {
            lblVolumeLeft.Text = "静音";
            lblVolumeRight.Text = lblVolumeLeft.Text;
            lblVolumeLeft.ForeColor = Color.Red;
            lblVolumeRight.ForeColor = Color.Red;
        }

        if (App.Settings.Enable3DMode)
        {
            lblVolumeLeft.Visible = true;
            lblVolumeRight.Visible = true;

        }
        else
        {
            lblVolumeLeft.Visible = true;
            lblVolumeRight.Visible = false;
        }
        _lastCursorChanged = Environment.TickCount;
    }

    private void ShowProgressBar()
    {
        //Player.SetPropertyString("osc", "no");
        //Player.SetPropertyDouble("osd-level", 0);
        if (App.Settings.Enable3DMode)
        {
            progressBarRight.Visible = true;
            lblDurationRight.Visible = true;
            lblStatusRight.Visible = true;
            lblVolumeRight.Visible = true;
        }

        progressBarLeft.Visible = true;
        lblDurationLeft.Visible = true;
        lblStatusLeft.Visible = true;
        lblVolumeLeft.Visible = true;

        progressBarLeft.BringToFront();
        lblDurationLeft.BringToFront();
        lblStatusLeft.BringToFront();
        lblVolumeLeft.BringToFront();

        progressBarRight.BringToFront();
        lblDurationRight.BringToFront();
        lblStatusRight.BringToFront();
        lblVolumeRight.BringToFront();
        _lastCursorChanged = Environment.TickCount;
    }

    void ShowCursor()
    {
        overlayPanel.Cursor = Cursors.Default;
        //Cursor.Show();
        _lastCursorPosition = MousePosition;
        _lastCursorChanged = Environment.TickCount;
    }

    void ShowVideoOSD()
    {
        //Debug.Print($"{DateTime.Now.ToString()}-progressBarRight.visible={progressBarRight.Visible}");
        btnBackLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        btn3DSubtitleModeLeft.Visible = (!isAudio && hhzMainPage.Visible == false) ? true : false;
        btn3DLeft.Visible = true;
        progressBarLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        btnVideoTrackLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        btnAudioTrackLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        btnSubtitleTrackLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        btnRenderLeft.Visible = (!isAudio && hhzMainPage.Visible == false) ? true : false;
        btnPlayLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        lblDurationLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        lblStatusLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        lblVolumeLeft.Visible = (hhzMainPage.Visible == false) ? true : false;
        panelTestLeft.Visible = (hhzMainPage.Visible == false && bTestMode) ? true : false;
        gbRifeLeft.Visible = (hhzMainPage.Visible == false /*&& (fps > 0 && fps <= 30)*/) ? true : false;

        if (App.Settings.Enable3DMode)
        {
            btn3DLeft.Text = "3D模式";
            btn3DRight.Text = "3D模式";
            btnBackRight.Left = Width / 2 + btnBackLeft.Left;
            btn3DRight.Left = Width / 2 + btn3DLeft.Left;
            btn3DSubtitleModeRight.Left = Width / 2 + btn3DSubtitleModeLeft.Left;
            btnVideoTrackRight.Left = Width / 2 + btnVideoTrackLeft.Left;
            btnAudioTrackRight.Left = Width / 2 + btnAudioTrackLeft.Left;
            btnSubtitleTrackRight.Left = Width / 2 + btnSubtitleTrackLeft.Left;
            btnRenderRight.Left = Width / 2 + btnRenderLeft.Left;
            btnPlayLeft.Left = btnFullScreenLeft.Left;
            btnPlayRight.Left = btnFullScreenLeft.Left - Width / 2;
            //btnFullScreenLeft.Left = btnFullScreenRight.Left - Width / 2;            
            lblDurationRight.Left = Width / 2 + lblDurationLeft.Left;
            lblStatusLeft.Left = lblDurationLeft.Left + lblDurationLeft.Width;
            lblStatusRight.Left = Width / 2 + lblStatusLeft.Left;
            progressBarRight.Left = Width / 2 + progressBarLeft.Left;
            panelTestRight.Left = Width / 2 + panelTestLeft.Left;
            lblVolumeLeft.Left = lblStatusLeft.Left + lblStatusLeft.Width;
            lblVolumeRight.Left = Width / 2 + lblVolumeLeft.Left;
            gbRifeLeft.Left = lblVolumeLeft.Right - gbRifeLeft.Width;
            gbRifeRight.Left = lblVolumeRight.Right - gbRifeRight.Width;

            progressBarLeft.Width = btnPlayRight.Right - btn3DLeft.Left; /*(int)(3750.0000 / 3840 * Width / 2);*/
            progressBarRight.Width = progressBarLeft.Width;
            lblStatusLeft.Width = (progressBarLeft.Width - lblDurationLeft.Width - lblVolumeLeft.Width);
            lblStatusRight.Width = lblStatusLeft.Width;

            btnFullScreenLeft.Visible = false;

            btnBackRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btn3DSubtitleModeRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btn3DRight.Visible = true;
            progressBarRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btnVideoTrackRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btnAudioTrackRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btnSubtitleTrackRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btnRenderRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            btnPlayRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            lblDurationRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            lblStatusRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            lblVolumeRight.Visible = (hhzMainPage.Visible == false) ? true : false;
            panelTestRight.Visible = (hhzMainPage.Visible == false && bTestMode) ? true : false;
            gbRifeRight.Visible = (hhzMainPage.Visible == false /*&& (fps > 0 && fps <= 30)*/) ? true : false;

            progressBarRight.BringToFront();
        }
        else
        {
            progressBarLeft.Top = lblDurationLeft.Top + lblDurationLeft.Height;
            progressBarLeft.Width = btnFullScreenLeft.Left + btnFullScreenLeft.Width - btn3DLeft.Left;
            //progressBarRight.Width = progressBarLeft.Width;

            lblStatusLeft.Left = lblDurationLeft.Left + lblDurationLeft.Width;
            lblStatusLeft.Width = (progressBarLeft.Width - lblDurationLeft.Width - lblVolumeLeft.Width);
            lblVolumeLeft.Left = lblStatusLeft.Left + lblStatusLeft.Width;
            gbRifeLeft.Left = lblVolumeLeft.Right - gbRifeLeft.Width;

            btnPlayLeft.Left = btnFullScreenLeft.Left - btnFullScreenLeft.Width - 10;
            //btnPlayRight.Left = btnFullScreenLeft.Left - Width / 2;

            btnBackRight.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btn3DRight.Visible = false;
            btn3DLeft.Text = "2D模式";
            btn3DRight.Text = "2D模式";
            progressBarRight.Visible = false;
            btnVideoTrackRight.Visible = false;
            btnAudioTrackRight.Visible = false;
            btnSubtitleTrackRight.Visible = false;
            btnRenderRight.Visible = false;
            btnPlayRight.Visible = false;
            btnFullScreenLeft.Visible = true;
            lblDurationRight.Visible = false;
            lblStatusRight.Visible = false;
            lblVolumeRight.Visible = false;
            panelTestRight.Visible = false;
            gbRifeRight.Visible = false;
        }

        btnBackLeft.BringToFront();
        btn3DSubtitleModeLeft.BringToFront();
        btn3DLeft.BringToFront();
        progressBarLeft.BringToFront();
        btnVideoTrackLeft.BringToFront();
        btnAudioTrackLeft.BringToFront();
        btnSubtitleTrackLeft.BringToFront();
        btnRenderLeft.BringToFront();
        btnPlayLeft.BringToFront();
        btnFullScreenLeft.BringToFront();
        lblDurationLeft.BringToFront();
        lblStatusLeft.BringToFront();
        lblVolumeLeft.BringToFront();
        gbRifeLeft.BringToFront();

        btnBackRight.BringToFront();
        btn3DSubtitleModeRight.BringToFront();
        btn3DRight.BringToFront();
        btnVideoTrackRight.BringToFront();
        btnAudioTrackRight.BringToFront();
        btnSubtitleTrackRight.BringToFront();
        btnRenderRight.BringToFront();
        btnPlayRight.BringToFront();
        lblDurationRight.BringToFront();
        lblStatusRight.BringToFront();
        lblVolumeRight.BringToFront();
        lblDurationLeft.BringToFront();
        if (isAudio)
        {
            ShowAudioUI();
        }
        _lastCursorChanged = Environment.TickCount;
    }
    void HideCursor()
    {
        //Cursor.Hide();
        overlayPanel.Cursor = CreateTransparentCursor();
        //overlayPanel.Cursor = trancur;
    }
    void HideVideoOSD()
    {
        //if (App.Settings.Enable3DMode)
        {
            btnBackLeft.Visible = false;
            btn3DSubtitleModeLeft.Visible = false;
            btn3DLeft.Visible = false;
            progressBarLeft.Visible = false;
            btnVideoTrackLeft.Visible = false;
            btnAudioTrackLeft.Visible = false;
            btnSubtitleTrackLeft.Visible = false;
            btnRenderLeft.Visible = false;
            btnPlayLeft.Visible = false;
            btnFullScreenLeft.Visible = false;
            lblDurationLeft.Visible = false;
            lblStatusLeft.Visible = false;
            lblVolumeLeft.Visible = false;
            panelTestLeft.Visible = false;
            gbRifeLeft.Visible = false;

            btnBackRight.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btn3DRight.Visible = false;
            progressBarRight.Visible = false;
            btnVideoTrackRight.Visible = false;
            btnAudioTrackRight.Visible = false;
            btnSubtitleTrackRight.Visible = false;
            btnRenderRight.Visible = false;
            btnPlayRight.Visible = false;
            lblDurationRight.Visible = false;
            lblStatusRight.Visible = false;
            lblVolumeRight.Visible = false;
            panelTestRight.Visible = false;
            gbRifeRight.Visible = false;
        }
    }

    private Cursor CreateTransparentCursor()
    {
        // 创建 1x1 的透明位图
        using (Bitmap bmp = new Bitmap(1, 1))
        {
            // 设置透明
            bmp.MakeTransparent();

            // 创建光标
            return new Cursor(bmp.GetHicon());
        }
    }

    [DllImport("DwmApi")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        //SaveWindowProperties();
        //Player.Command("write-watch-later-config");
        App.Settings.Save();
        //if (SettingsManager.Current.IsModify)
        //SettingsManager.Save();

        //if (Player.IsQuitNeeded)
        Player.CommandV("quit");
        //if (!Player.ShutdownAutoResetEvent.WaitOne(10000))
        //    Msg.ShowError(_("Shutdown thread failed to complete within 10 seconds."));

        //Player.Destroy();
    }


    public static class v3DSubtitleMode
    {
        public static void Auto(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {
            var vw = Player.GetPropertyInt("width");
            var vh = Player.GetPropertyInt("height");
            if ((double)vw / vh < 2.35 / 1) // half-SBS
            //if (!App.Settings.Enable3DMode)
            {
                Player.SetPropertyBool(CMD_sub_stereo_on, true);
                //Player.SetPropertyBool("sub-stereo-duplicate", false);
            }
            else
            {
                Player.SetPropertyBool(CMD_sub_stereo_on, false);
                //Player.SetPropertyBool("sub-stereo-duplicate", false);
            }
            Lbutton.Text = "3D字幕模式:自动";
            Rbutton.Text = "3D字幕模式:自动";
            App.Settings.SubtitleMode = enumSubtitleMode.Auto;
        }

        public static void Sub2D(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {

            Lbutton.Text = "2D字幕(自动3D)";
            Rbutton.Text = "2D字幕(自动3D)";
            Player.SetPropertyBool(CMD_sub_stereo_on, false);
            //Player.SetPropertyBool("sub-stereo-duplicate", false);
            App.Settings.SubtitleMode = enumSubtitleMode.On;
        }

        public static void Sub3D(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, true);
            //Player.SetPropertyBool("sub-stereo-duplicate", false);
            Lbutton.Text = "3D字幕";
            Rbutton.Text = "3D字幕";
            App.Settings.SubtitleMode = enumSubtitleMode.Off;
        }
    }

    int isub = 0;
    private bool _isReturn2D;
    private bool isAudio;
    private bool bPressEnter;
    private DateTime _lastEscapeTime;
    private bool bPressPageDownUp;
    private string StreamingContextStates;
    private string strstate;
    private bool bTestMode;
    private ToolStripMenuItem Videoaspectl;
    private ToolStripMenuItem Videoaspectr;
    private bool bNvidia;
    private int ibufferframe = 32;
    private int iconcurrentframe = 2;
    private bool bSetRender;
    private int iVsrScale = 2;
    private string AutoAudioid;
    private string AutoVideoid;
    private string AutoSubtitleid;
    private bool bProgSet;
    private string[] cmdlineArg;

    //private bool bvapoursynth;

    void Set3DSubtitleMode(string mode3DSubtitle)
    {
        if (mode3DSubtitle.Contains("2D字幕(自动3D)"))
        {
            v3DSubtitleMode.Sub2D(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
        }
        else
        {
            v3DSubtitleMode.Sub3D(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
        }
        _lastCursorChanged = Environment.TickCount;
    }

    private void btnSubtitle_Click(object? sender, EventArgs e)
    {
        if (hhzSettingsManager.Current.SubtitleMode.Contains("2D字幕(自动3D)"))
        {
            hhzSettingsManager.Current.SubtitleMode = "3D字幕";
            v3DSubtitleMode.Sub3D(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
        }
        else
        {
            hhzSettingsManager.Current.SubtitleMode = "2D字幕(自动3D)";
            v3DSubtitleMode.Sub2D(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
        }
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.KeyCode:
                break;
            case Keys.Modifiers:
                break;
            case Keys.None:
                break;
            case Keys.LButton:
                break;
            case Keys.RButton:
                break;
            case Keys.Cancel:
                break;
            case Keys.MButton:
                break;
            case Keys.XButton1:
                break;
            case Keys.XButton2:
                break;
            case Keys.Tab:
                break;
            case Keys.LineFeed:
                break;
            case Keys.Clear:
                break;
            case Keys.Enter:
                bPressEnter = true;
                if (App.Settings.Enable3DMode == false)
                {
                    OverlayPanel_MouseDoubleClick(null, null);
                }
                break;
            case Keys.ShiftKey:
                break;
            case Keys.ControlKey:
                break;
            case Keys.Menu:
                break;
            case Keys.Pause:
                break;
            case Keys.CapsLock:
                break;
            case Keys.HangulMode:
                break;
            case Keys.JunjaMode:
                break;
            case Keys.FinalMode:
                break;
            case Keys.HanjaMode:
                break;
            case Keys.Back:
            case Keys.Escape:
                if (hhzMainPage.Visible == false)
                {
                    //if (App.Settings.Enable3DMode == false)
                    //{
                    //    if (WindowState == FormWindowState.Normal)
                    //    {
                    //        btnBack_Click(null, null);
                    //    }
                    //    else
                    //    {
                    //        this.WindowState = FormWindowState.Normal;
                    //        this.FormBorderStyle = FormBorderStyle.Sizable;
                    //    }
                    //}
                    //else
                    //{
                        btnBack_Click(null, null);
                    //}
                }
                else
                {
                    //双击Escape退出程序
                    if (_lastEscapeTime.AddMilliseconds(1000) > DateTime.Now)
                    {
                        // 双击处理
                        this.Close();
                    }
                    else
                    {
                        // 第一次按Escape，记录时间
                        _lastEscapeTime = DateTime.Now;
                        ShowToast("再按一次ESC退出。。。", 1000);
                    }
                }
                break;
            case Keys.IMEConvert:
                break;
            case Keys.IMENonconvert:
                break;
            case Keys.IMEAccept:
                break;
            case Keys.IMEModeChange:
                break;
            case Keys.Space:
                Player.Command("cycle pause");
                e.Handled = true;
                break;
            case Keys.PageUp:
                //HideCursor();
                //HideVideoOSD();
                Player.Command("playlist-prev");   // 上一个媒体
                if (Player.Duration.TotalSeconds == 0) bPressPageDownUp = true; /*Player.Command("pause");*/
                break;
            case Keys.PageDown:
                //HideCursor();
                //HideVideoOSD();
                Player.Command("playlist-next");   // 下一个媒体
                if (Player.Duration.TotalSeconds == 0) bPressPageDownUp = true;/*Player.Command("cycle pause");*/
                break;
            case Keys.End:
                break;
            case Keys.Home:
                break;
            case Keys.Left:
                Player.Command("seek -5 relative");  // 从当前位置往后退 5 秒
                ShowProgressBar();
                break;
            case Keys.Up:
                if (hhzMainPage.Visible == false)
                {
                    double vol = Player.GetPropertyDouble("volume");
                    vol = Math.Min(vol + 5, 150); // 向上滚动增加音量
                    ShowVolumeUI(vol);
                }
                break;
            case Keys.Right:
                //string format = Player.GetPropertyString("file-format")?.ToLower();
                Player.Command("seek 5 relative");   // 从当前位置往前跳 5 秒                
                ShowProgressBar();
                break;
            case Keys.Down:
                if (hhzMainPage.Visible == false)
                {
                    double vol = Player.GetPropertyDouble("volume");
                    vol = Math.Max(vol - 5, 0);   // 向下滚动减少音量
                    ShowVolumeUI(vol);
                }
                break;
            case Keys.Select:
                break;
            case Keys.Print:
                break;
            case Keys.Execute:
                break;
            case Keys.PrintScreen:
                break;
            case Keys.Insert:
                break;
            case Keys.Delete:
                break;
            case Keys.Help:
                break;
            case Keys.D0:
                break;
            case Keys.D1:
                break;
            case Keys.D2:
                break;
            case Keys.D3:
                break;
            case Keys.D4:
                break;
            case Keys.D5:
                break;
            case Keys.D6:
                break;
            case Keys.D7:
                break;
            case Keys.D8:
                break;
            case Keys.D9:
                break;
            case Keys.A:
                break;
            case Keys.B:
                break;
            case Keys.C:
                break;
            case Keys.D:
                break;
            case Keys.E:
                break;
            case Keys.F:
                break;
            case Keys.G:
                break;
            case Keys.H:
                break;
            case Keys.I:
                break;
            case Keys.J:
                break;
            case Keys.K:
                break;
            case Keys.L:
                break;
            case Keys.M:
                break;
            case Keys.N:
                break;
            case Keys.O:
                break;
            case Keys.P:
                break;
            case Keys.Q:
                break;
            case Keys.R:
                break;
            case Keys.S:
                break;
            case Keys.T:
                break;
            case Keys.U:
                break;
            case Keys.V:
                break;
            case Keys.W:
                break;
            case Keys.X:
                break;
            case Keys.Y:
                break;
            case Keys.Z:
                break;
            case Keys.LWin:
                break;
            case Keys.RWin:
                break;
            case Keys.Apps:
                break;
            case Keys.Sleep:
                break;
            case Keys.NumPad0:
                break;
            case Keys.NumPad1:
                break;
            case Keys.NumPad2:
                break;
            case Keys.NumPad3:
                break;
            case Keys.NumPad4:
                break;
            case Keys.NumPad5:
                break;
            case Keys.NumPad6:
                break;
            case Keys.NumPad7:
                break;
            case Keys.NumPad8:
                break;
            case Keys.NumPad9:
                break;
            case Keys.Multiply:
                break;
            case Keys.Add:
                break;
            case Keys.Separator:
                break;
            case Keys.Subtract:
                break;
            case Keys.Decimal:
                break;
            case Keys.Divide:
                break;
            case Keys.F1:
                break;
            case Keys.F2:
                break;
            case Keys.F3:
                break;
            case Keys.F4:
                break;
            case Keys.F5:
                break;
            case Keys.F6:
                break;
            case Keys.F7:
                break;
            case Keys.F8:
                break;
            case Keys.F9:
                break;
            case Keys.F10:
                break;
            case Keys.F11:
                //Player.Command($"vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
                break;
            case Keys.F12:
                if (bTestMode == false)
                {
                    bTestMode = true;
                }
                else
                {
                    bTestMode = false;
                }
                showtestMode();
                break;
            case Keys.F13:
                break;
            case Keys.F14:
                break;
            case Keys.F15:
                break;
            case Keys.F16:
                break;
            case Keys.F17:
                break;
            case Keys.F18:
                break;
            case Keys.F19:
                break;
            case Keys.F20:
                break;
            case Keys.F21:
                break;
            case Keys.F22:
                break;
            case Keys.F23:
                break;
            case Keys.F24:
                break;
            case Keys.NumLock:
                break;
            case Keys.Scroll:
                break;
            case Keys.LShiftKey:
                break;
            case Keys.RShiftKey:
                break;
            case Keys.LControlKey:
                break;
            case Keys.RControlKey:
                break;
            case Keys.LMenu:
                break;
            case Keys.RMenu:
                break;
            case Keys.BrowserBack:
                break;
            case Keys.BrowserForward:
                break;
            case Keys.BrowserRefresh:
                break;
            case Keys.BrowserStop:
                break;
            case Keys.BrowserSearch:
                break;
            case Keys.BrowserFavorites:
                break;
            case Keys.BrowserHome:
                break;
            case Keys.VolumeMute:
                break;
            case Keys.VolumeDown:
                break;
            case Keys.VolumeUp:
                break;
            case Keys.MediaNextTrack:
                break;
            case Keys.MediaPreviousTrack:
                break;
            case Keys.MediaStop:
                break;
            case Keys.MediaPlayPause:
                break;
            case Keys.LaunchMail:
                break;
            case Keys.SelectMedia:
                break;
            case Keys.LaunchApplication1:
                break;
            case Keys.LaunchApplication2:
                break;
            case Keys.OemSemicolon: //oem1
                break;
            case Keys.Oemplus:
                break;
            case Keys.Oemcomma:
                break;
            case Keys.OemMinus:
                break;
            case Keys.OemPeriod:
                break;
            case Keys.OemQuestion: //oem2
                break;
            case Keys.Oemtilde:  //oem3
                break;
            case Keys.OemOpenBrackets: //oem4
                break;
            case Keys.OemPipe:      //oem5
                break;
            case Keys.OemCloseBrackets: //oem7
                break;
            case Keys.OemQuotes:        //oem8
                break;
            case Keys.OemBackslash:     //oem102
                break;
            case Keys.ProcessKey:
                break;
            case Keys.Packet:
                break;
            case Keys.Attn:
                break;
            case Keys.Crsel:
                break;
            case Keys.Exsel:
                break;
            case Keys.EraseEof:
                break;
            case Keys.Play:
                break;
            case Keys.Zoom:
                break;
            case Keys.NoName:
                break;
            case Keys.Pa1:
                break;
            case Keys.OemClear:
                break;
            case Keys.Shift:
                break;
            case Keys.Control:
                break;
            case Keys.Alt:
                break;
            default:
                break;
        }
    }

    private void showtestMode()
    {
        if (App.Settings.Enable3DMode)
        {
            if (bTestMode)
            {
                panelTestLeft.Visible = true;
                panelTestRight.Visible = true;
            }
            else
            {
                panelTestLeft.Visible = false;
                panelTestRight.Visible = false;
            }
        }
        else
        {
            if (bTestMode)
            {
                panelTestLeft.Visible = true;
            }
            else
            {
                panelTestLeft.Visible = false;
            }
        }
    }

    void ShowToast(string message, int showtime)
    {
        //启动Toast计时器并显示提示
        ToastTimer.Stop();
        if (showtime > 0) ToastTimer.Interval = showtime;
        ToastTimer.Start();
        if (App.Settings.Enable3DMode)
        {
            lblToastLeft.Left = 0;
            lblToastLeft.Width = Width / 2;
            lblToastLeft.Text = message;
            lblToastLeft.Visible = true;
            lblToastLeft.BringToFront();

            lblToastRight.Left = Width / 2;
            lblToastRight.Width = lblToastLeft.Width;
            lblToastRight.Text = message;
            lblToastRight.Visible = true;
            lblToastRight.BringToFront();
        }
        else
        {
            lblToastLeft.Left = 0;
            lblToastLeft.Width = Width;
            lblToastLeft.Text = message;
            lblToastLeft.Visible = true;
            lblToastLeft.BringToFront();
        }
    }
    void HideToast()
    {
        lblToastLeft.Text = "";
        lblToastLeft.Visible = false;
        lblToastRight.Text = "";
        lblToastRight.Visible = false;
    }

    private void cmbVO_SelectedIndexChanged(object sender, EventArgs e)
    {
        Player.SetPropertyString("vo", cmbVO.Text);
    }

    private void cmbhwdec_SelectedIndexChanged(object sender, EventArgs e)
    {
        Player.SetPropertyString("hwdec", cmbhwdec.Text);
    }

    private void cmbgpuapi_SelectedIndexChanged(object sender, EventArgs e)
    {
        Player.SetPropertyString("gpu-api", cmbgpuapi.Text);
    }

    private void cmbGpuContext_SelectedIndexChanged(object sender, EventArgs e)
    {
        Player.SetPropertyString("gpu-context", cmbGpuContext.Text);
    }

    private void chkTestModeLeft_CheckedChanged(object sender, EventArgs e)
    {
        chkTestModeRight.Checked = chkTestModeLeft.Checked;
    }

    private void chkTestModeRight_CheckedChanged(object sender, EventArgs e)
    {
        chkTestModeLeft.Checked = chkTestModeRight.Checked;
    }

    private void chkRife_CheckedChanged(object sender, EventArgs e)
    {
        if (WasShown && !bProgSet)
        {
            if (((CheckBox)sender).Checked)
            {
                cbRifeTimesLeft.Enabled = true;
                cbRifeTimesRight.Enabled = true;
                if (!bRifeOn()) Player.Command($"no-osd vf add vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                //Player.Command($"vf toggle vapoursynth=file=D:/HHZTEST/mpv-lazy/portable_config/vs/MEMC_MVT_LQ.vpy");
                hhzSettingsManager.Current.Rife = true;
            }
            else
            {
                cbRifeTimesLeft.Enabled = false;
                cbRifeTimesRight.Enabled = false;
                //if (bRifeOn()) Player.Command("no-osd vf set vapoursynth=file=NULL");
                if (bRifeOn()) Player.Command($"no-osd vf remove vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
                //Player.Command($"vf remove vapoursynth=file=D:/HHZTEST/mpv-lazy/portable_config/vs/MEMC_MVT_LQ.vpy");
                hhzSettingsManager.Current.Rife = false;
            }
        }
    }

    private void cbRifeTimes_SelectedIndexChanged(object sender, EventArgs e)
    {
        Player.Command($"no-osd vf set vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
    }
    //public static string GetHdrType(MainPlayer player)
    //{ 
    //    string hdr = player.GetPropertyString("video-params/hdr-format") ?? "";
    //    string tr = player.GetPropertyString("video-params/transfer") ?? "";
    //    string pr = player.GetPropertyString("video-params/primaries") ?? "";

    //    if (hdr.Equals("HLG", StringComparison.OrdinalIgnoreCase) || tr == "arib-std-b67")
    //        return "HLG";
    //    if (hdr.Equals("PQ", StringComparison.OrdinalIgnoreCase) || tr == "smpte2084")
    //        return "PQ HDR10+)";

    //    // 补个保守判断：bt.2020 + 10bit 但没读到传输函数时，仍可能是 HDR
    //    string pix = player.GetPropertyString("video-params/pixelformat") ?? "";
    //    if (pr.Contains("2020") /*|| pix.Contains("10")*/)
    //        return "HDR10";

    //    return "SDR";
    //}
    //static string Prim(string k) => Player.GetPropertyString(k) ?? "";
    //static double Dbl(string k) { try { return Player.GetPropertyDouble(k); } catch { return double.NaN; } }

    //public enum HdrKind { SDR, PQ10, HDR10, HDR10Plus, HLG, PQ_Unknown /*DV/Vivid等*/ }

    //public static HdrKind DetectHdrKind()
    //{
    //    var gamma = Prim("video-params/gamma").ToLowerInvariant();   // mpv手册推荐字段
    //    if (string.IsNullOrEmpty(gamma))
    //        gamma = Prim("video-params/transfer").ToLowerInvariant(); // 旧脚手架兜底（你以前用过）

    //    if (gamma == "hlg") return HdrKind.HLG;

    //    if (gamma == "pq" || gamma == "smpte2084")
    //    {
    //        bool hasStatic = !double.IsNaN(Dbl("video-params/max-cll")) ||
    //                         !double.IsNaN(Dbl("video-params/max-luma")) ||
    //                         !double.IsNaN(Dbl("video-params/max-fall"));

    //        bool hasDyn = !double.IsNaN(Dbl("video-params/scene-max-r")) ||
    //                      !double.IsNaN(Dbl("video-params/scene-max-g")) ||
    //                      !double.IsNaN(Dbl("video-params/scene-max-b"));

    //        if (hasDyn) return HdrKind.HDR10Plus;
    //        if (hasStatic) return HdrKind.HDR10;

    //        // 仍是PQ，但没看到静态/动态元数据：可能是 PQ10、也可能是 DV/HDR Vivid 的落地轨
    //        return HdrKind.PQ_Unknown;
    //    }
    //    return HdrKind.SDR;
    //}

    public static string GetHdrType(MainPlayer player)
    {
        string Prim(string k) => player.GetPropertyString(k) ?? "";
        double Dbl(string k) { try { return player.GetPropertyDouble(k); } catch { return double.NaN; } }
        var gamma = Prim("video-params/gamma").ToLowerInvariant();   // mpv手册推荐字段
        if (string.IsNullOrEmpty(gamma))
            gamma = Prim("video-params/transfer").ToLowerInvariant(); // 旧脚手架兜底（你以前用过）

        if (gamma == "hlg") return "HDR HLG";

        if (gamma == "pq" || gamma == "smpte2084")
        {
            bool hasStatic = !double.IsNaN(Dbl("video-params/max-cll")) ||
                             !double.IsNaN(Dbl("video-params/max-luma")) ||
                             !double.IsNaN(Dbl("video-params/max-fall"));

            bool hasDyn = !double.IsNaN(Dbl("video-params/scene-max-r")) ||
                          !double.IsNaN(Dbl("video-params/scene-max-g")) ||
                          !double.IsNaN(Dbl("video-params/scene-max-b"));

            if (hasDyn) return "HDR10+";
            if (hasStatic) return "HDR10";

            // 仍是PQ，但没看到静态/动态元数据：可能是 PQ10、也可能是 DV/HDR Vivid 的落地轨
            return "HDR Vivid";
        }
        return "SDR";
    }
    private void chkVSR_CheckedChanged(object sender, EventArgs e)
    {
        if (WasShown && !bSetRender)
        {
            if (((CheckBox)sender).Checked)
            {                
                hhzSettingsManager.Current.VSR = true;                
            }
            else
            {
                hhzSettingsManager.Current.VSR = false;
            }
            setVSR();
        }
    }

    private bool bRTXOn()
    {
        string vfList = Player.GetPropertyString("vf");
        if (vfList != "" && vfList.Contains($"d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12"))
        {
            return true;
        }
        else
        {            
            return false;
        }
    }
    public void OpenFromIpc(string[] args)
    {
        if (args == null || args.Length == 0) return;

        if (CommandlineHelper.ProcessCommandline(args))
        {

        }
        if (CommandlineHelper.files.Count > 0)
        {        
            //有命令行参数，直接初始化播放器并加载文件
            //Player?.Command("stop");
            //Player?.Command("set pause yes");
            //if (bRifeOn()) Player?.Command($"no-osd vf remove vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rife_2x.vpy").Replace("\\", "/")}:buffered-frames={ibufferframe}:concurrent-frames={iconcurrentframe}");
            //if (bRTXOn()) Player?.Command($"no-osd vf remove d3d11vpp=scale={iVsrScale}:scaling-mode=nvidia:format=nv12");
            //hhzMainPage.Visible = false;
            //overlayPanel.Visible = true;
            //CursorTimer.Enabled = true;

            //if (App.Settings.Enable3DMode)
            //{
            //    btn3DLeft.Visible = false;
            //    btn3DRight.Visible = false;
            //}
            //else
            //{
            //    btn3DLeft.Visible = false;
            //    btnFullScreenLeft.Visible = false;
            //}

            HhzMainPage_FileOpened(null, [.. CommandlineHelper.files]);
            //if (CommandlineHelper.OnlyConsole) QuitEvent.WaitOne();
            //Player.LoadFiles([.. files], true, false);
        }
    }
}
