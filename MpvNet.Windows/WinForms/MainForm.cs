
using CommunityToolkit.Mvvm.Messaging;
using MpvNet.ExtensionMethod;
using MpvNet.Help;
using MpvNet.MVVM;
using MpvNet.Windows.UI;
using MpvNet.Windows.WPF;
using MpvNet.Windows.WPF.MsgBox;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using static HandyControl.Tools.Interop.InteropValues;
using static MpvNet.AppSettings;
using static MpvNet.Windows.Help.WinApiHelp;
using static MpvNet.Windows.Native.WinApi;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace MpvNet.Windows.WinForms;

public partial class MainForm : Form
{
    public SnapManager SnapManager = new SnapManager();
    public IntPtr MpvWindowHandle { get; set; }
    public bool WasShown { get; set; }
    public static MainForm? Instance { get; set; }
    ContextMenu ContextMenu { get; } = new ContextMenu();
    AutoResetEvent MenuAutoResetEvent { get; } = new AutoResetEvent(false);
    Point _lastCursorPosition;
    Taskbar? _taskbar;
    Point _mouseDownLocation;
    List<Binding>? _confBindings;

    int _lastCursorChanged;
    int _lastCycleFullscreen;
    int _taskbarButtonCreatedMessage;
    int _cursorAutohide = 1000;

    bool _contextMenuIsReady;
    bool _wasMaximized;
    bool _maxSizeSet;

    //Cursor trancur = CreateTransparentCursor();

    const int WM_SYSCOMMAND = 0x0112;
    const int SC_MAXIMIZE = 0xF030;

    /// <summary>
    /// 3D模式菜单头
    /// </summary>
    MenuItem? _3DModeMenuItem;
    MenuItem? _s3DModeSwitchMenuItem;
    /// <summary>
    /// 双眼字幕菜单头
    /// </summary>
    MenuItem? _SbsSubMemuItem;

    MenuItem? _sSbsSubAutoMenuItem;
    MenuItem? _sSbsSubOnMenuItem;
    MenuItem? _sSbsSubOffMenuItem;

    //WpfControls.MenuItem? _fullScreenUIMemuItem;
    /// <summary>
    /// 用于控制是否开启3D字幕的命令（在libmpv-2.dll 线路B的源码里新增）
    /// </summary>
    const string CMD_sub_stereo_on = "sub-stereo-on";
    //const string CMD_sub_stereo_
    /// <summary>
    /// 是否开启线路A的方法实现3D字幕的命令（在libmpv-2.dll），设置为false的花则使用线路B方法实现，默认libmpv-2.dll是使用线路A的
    /// </summary>
    const string CMD_sub_stereo_duplicate = "sub-stereo-duplicate";

    int btnLeft = 40;
    int progressBarLeftWidth = 1981;

    public MainForm()
    {
        InitializeComponent();
        //Player.SetPropertyString("osc", "no");
        //InitializeLogoOverlay();
        DoubleBuffered = true;     // 开启双缓冲，避免闪烁
        //this.SetStyle(ControlStyles.AllPaintingInWmPaint |
        //      ControlStyles.UserPaint |
        //      ControlStyles.OptimizedDoubleBuffer, true);
        //this.UpdateStyles();

        UpdateDarkMode();
        InitializehhzOverlay();
        InitPlayerEvents();
        progressBarLeftWidth = progressBarLeft.Width;
        int cmdlineArgLength = Environment.GetCommandLineArgs().Length;
        if (cmdlineArgLength > 1)
        {
            // 如果有命令行参数，直接初始化播放器并加载文件
            hhzMainPage.Visible = false;
            overlayPanel.Visible = true;
            List<string> files = [];

            foreach (string arg in Environment.GetCommandLineArgs().Skip(1))
            {
                if (!arg.StartsWith("--") && (arg == "-" || arg.Contains("://") ||
                    arg.Contains(":\\") || arg.StartsWith("\\\\") || arg.StartsWith('.') ||
                    File.Exists(arg)))
                {
                    files.Add(arg);
                }
            }
            Player.LoadFiles([.. files], true, false);
        }
    }

    private void InitPlayerEvents()
    {
        Player.FileLoaded += Player_FileLoaded;
        // 订阅播放进度
        Player.ObservePropertyDouble("time-pos", (double value) =>
        {
            if (!double.IsNaN(value))
            {
                // 在 UI 线程里更新控件
                this.Invoke((MethodInvoker)(() =>
                {
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
                    btnPlayLeft.Text = "播放";
                    btnPlayRight.Text = "播放";
                }
                else
                {
                    btnPlayLeft.Text = "暂停";
                    btnPlayRight.Text = "暂停";
                }
            }));
        });
    }

    HHZMainPage hhzMainPage = new HHZMainPage();
    private void InitializehhzOverlay()
    {
        hhzMainPage.BringToFront();
        // 如果上次目录存在就恢复
        if (!string.IsNullOrEmpty(App.Settings.LastOpenedFolder) &&
            Directory.Exists(App.Settings.LastOpenedFolder))
        {
            hhzMainPage.LoadFolder(App.Settings.LastOpenedFolder);
        }

        Controls.Add(hhzMainPage);
        overlayPanel = new System.Windows.Forms.Panel
        {
            Parent = this,
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
        };
        overlayPanel.Visible = false;

        Controls.Add(overlayPanel);

        Text = "hhzPlayer";

        Player.Init(overlayPanel.Handle, true);

        //App.Settings.Enable3DMode = true;
        CycleFullScreenFor3D(App.Settings.Enable3DMode);

        hhzMainPage.FileOpened += HhzMainPage_FileOpened;
        hhzMainPage.FileDropped += HhzMainPage_FileDropped;        

        overlayPanel.MouseMove += OverlayPanel_MouseMove;
        overlayPanel.MouseDoubleClick += OverlayPanel_MouseDoubleClick;

        btnBackLeft.Click += btnBack_Click;
        btnBackRight.Click += btnBack_Click;
        
        btn3DLeft.Click += btn3D_Click;
        btn3DRight.Click += btn3D_Click;
        
        btn3DSubtitleModeLeft.Click += btnSubtitle_Click;
        btn3DSubtitleModeRight.Click += btnSubtitle_Click;
        
        btnFullHalfLeft.Click += BtnFullHalf_Click;
        btnFullHalfRight.Click += BtnFullHalf_Click;

        btnVideoTrackLeft.Click += BtnVideoTrack_Click;
        btnVideoTrackRight.Click += BtnVideoTrack_Click;
        
        btnAudioTrackLeft.Click += BtnAudioTrack_Click;
        btnAudioTrackRight.Click += BtnAudioTrack_Click;
        
        btnSubtitleTrackLeft.Click += BtnSubtitleTrack_Click;
        btnSubtitleTrackRight.Click += BtnSubtitleTrack_Click;
        
        btnPlayLeft.Click += BtnPlay_Click;
        btnPlayRight.Click += BtnPlay_Click;
        
        btnFullScreenLeft.Click += BtnFullScreen_Click;
        btnFullScreenRight.Click += BtnFullScreen_Click;

        progressBarLeft.MouseClick += ProgressBar_MouseClick;
        progressBarRight.MouseClick += ProgressBar_MouseClick;
        

        HideVideoUI();
    }

    private void BtnFullHalf_Click(object? sender, EventArgs e)
    {

    }

    private void BtnFullScreen_Click(object? sender, EventArgs e)
    {
        if (!App.Settings.Enable3DMode)
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
        }
    }

    private void BtnPlay_Click(object? sender, EventArgs e)
    {
        Player.Command("cycle pause");
        _lastCursorChanged = Environment.TickCount;

    }
    private void BtnSubtitleTrack_Click(object? sender, EventArgs e)
    {

    }

    private void BtnAudioTrack_Click(object? sender, EventArgs e)
    {
        
    }

    private void BtnVideoTrack_Click(object? sender, EventArgs e)
    {

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
        Player.Command("stop");
        hhzMainPage.Visible = true;
        overlayPanel.Visible = false;
        HideVideoUI();        
    }

    private void OverlayPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (!App.Settings.Enable3DMode)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            App.Settings.FormBorderStyle = GetAppFormBorderStyle(FormBorderStyle);
            App.Settings.WindowsStatus = GetAppWindowsStatus(WindowState);
        }
    }

    private void OverlayPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        //if (IsCursorPosDifferent(_lastCursorPosition))
        if (_lastCursorPosition != MousePosition)
        {
            Debug.Print($"{DateTime.Now.ToString()}-MouseMoveShowCursor");
            ShowCursor();
            _lastCursorChanged = Environment.TickCount;
            _lastCursorPosition = MousePosition;
        }
        if (e.Button == MouseButtons.Right)
        {
            //UpdateMenu();
        }
    }

    private void HhzMainPage_FileOpened(HHZMainPage sender, string path)
    {
        if (path.Length > 0)
        {
            hhzMainPage.Visible = false;
            overlayPanel.Visible = true;
            Player.LoadFiles(new[] { path }, true, false);
        }
    }

    private void HhzMainPage_FileDropped(object? sender, string[] files)
    {
        if (files != null && files.Length > 0)
        {
            hhzMainPage.Visible = false;
            overlayPanel.Visible = true;
            Player.LoadFiles(files, true, false); // 你项目里的 Player 调用
        }
    }

    void UpdateDarkMode()
    {
        if (Environment.OSVersion.Version >= new Version(10, 0, 18985))
            DwmSetWindowAttribute(Handle, 20, new[] { Theme.DarkMode ? 1 : 0 }, 4);  // DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    Rectangle prebounds;
    private bool bPlayerinited;
    private System.Windows.Forms.Panel overlayPanel;

    void set3DFullHalf()
    {
        if (Player.Duration.TotalMicroseconds > 0)
        {
            var vw = Player.GetPropertyInt("width");
            var vh = Player.GetPropertyInt("height");
            Player.SetPropertyString("video-aspect-override", (Width).ToString() + ":" + vh.ToString());
            //FullSBS画面比例最小值为2.35 * 2 : 1
            //if ((double)vw / vh <= 2.35 * 2 / 1) // half-SBS
            //{
            //    Player.SetPropertyString("video-aspect-override", (vw * 2).ToString() + ":" + vh.ToString());
            //}
            //else // full-SBS
            //{
            //    Player.SetPropertyString("video-aspect-override", vw.ToString() + ":" + vh.ToString());
            //}
        }
    }

    private void CycleFullScreenFor3D(bool enable3DMode)
    {
        if (enable3DMode)
        {
            Player.SetPropertyString("vo", "gpu");
            Player.SetPropertyString("gpu-api", "opengl");

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
                //Console.WriteLine($"右边有扩展屏幕：{rightScreen.Bounds}");
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                bounds.Width = bounds.Width * 2;
                this.Bounds = bounds;
            }
            else
            {
                //Console.WriteLine("右边没有扩展屏幕");
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }

            set3DFullHalf();
            //HideVideoUI();
            btn3DLeft.Text = "3D模式";
            ShowVideoUI();
        }
        else
        {
            Player.SetPropertyString("vo", "");
            Player.SetPropertyString("gpu-api", "");
            if (Player.Duration.TotalMicroseconds > 0)
            {
                var vw = Player.GetPropertyInt("width");
                var vh = Player.GetPropertyInt("height");
                Player.SetPropertyString("video-aspect-override", vw.ToString() + ":" + vh.ToString());
            }
            _isReturn2D = true;
            switch (App.Settings.FormBorderStyle)
            {
                case enumFormBorderStyle.Sizable:
                    FormBorderStyle = FormBorderStyle.Sizable;
                    break;
                case enumFormBorderStyle.None:
                    FormBorderStyle = FormBorderStyle.None;
                    break;
            }
            switch (App.Settings.WindowsStatus)
            {
                case enumWindowsStatus.Normal:
                    WindowState = FormWindowState.Normal;
                    break;
                case enumWindowsStatus.Maximized:
                    WindowState = FormWindowState.Maximized;
                    break;
                case enumWindowsStatus.Minimized:
                    WindowState = FormWindowState.Minimized;
                    break;
            }
            Bounds = new Rectangle(App.Settings.WindowLocation.X, App.Settings.WindowLocation.Y,
                                   App.Settings.WindowSize.Width, App.Settings.WindowSize.Height);
            _isReturn2D = false;
            btn3DLeft.Text = "2D模式";
            HideVideoUI();
        }
    }

    private enumFormBorderStyle GetAppFormBorderStyle(FormBorderStyle formBorderStyle)
    {
        switch (formBorderStyle) { 
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
            if (WindowState != FormWindowState.Minimized && WasShown && !_isReturn2D)
            {
                progressBarLeftWidth = progressBarLeft.Width;
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

    void CursorTimer_Tick(object sender, EventArgs e)
    {
        Debug.Print($"{DateTime.Now.ToString()}-CursorPositionDiff: {_lastCursorPosition != MousePosition}");
        //Debug.Print($"CursorTimer:{Environment.TickCount - _lastCursorChanged > _cursorAutohide} ActiveForm:{ActiveForm == this} Duration:{Player.Duration.TotalMilliseconds > 0}");

        if (_lastCursorPosition != MousePosition /*IsCursorPosDifferent(_lastCursorPosition)*/)
        {
            _lastCursorPosition = MousePosition;
            _lastCursorChanged = Environment.TickCount;
        }
        else if ((Environment.TickCount - _lastCursorChanged > _cursorAutohide) &&
            /*ClientRectangle.Contains(PointToClient(MousePosition)) &&*/
            ActiveForm == this /*&& !ContextMenu.IsVisible && !IsMouseInOsc()*/ && Player.Duration.TotalMilliseconds > 0)
        {
            if (/*!Player.GetPropertyBool("pause") && */hhzMainPage.Visible == false)
            {
                HideCursor();
                Debug.Print($"{DateTime.Now.ToString()}-HideCursor");
            }                        
        }
    }

    void SetProgressBarMax(double Maximum)
    {
        progressBarLeft.Maximum = (int)Maximum;
        progressBarRight.Maximum = (int)Maximum;
    }
    void SetProgressBarValue(double value)
    {
        progressBarLeft.Value = (int)value;
        progressBarRight.Value = (int)value;
    }

    void Player_FileLoaded()
    {
        BeginInvoke(() =>
        {
            if (Player.Duration.TotalMilliseconds > 0)
            {
                hhzMainPage.Visible = false;
                SetProgressBarMax(Player.Duration.TotalSeconds);
                if (App.Settings.Enable3DMode)
                {
                    set3DFullHalf();
                }
                //Player.SetPropertyString("video-aspect-override", "7680:2072");
                v3DSubtitleMode.On(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
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
                }
                else
                {
                    btnPlayLeft.Text = "播放";
                    btnPlayRight.Text = "播放";
                }

                //if (App.Settings.Enable3DMode)
                //{
                //    btnBackRight.Visible = true;
                //    btn3DSubtitleModeRight.Visible = true;
                //    btn3DRight.Visible = true;
                //    progressBarRight.Visible = true;
                //}
                //UpdateProgressBar();
            }
        });

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

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (WindowState == FormWindowState.Maximized)
            Player.SetPropertyBool("window-maximized", true);

        WpfApplication.Init();
        Theme.UpdateWpfColors();
        MessageBoxEx.MessageForeground = Theme.Current?.GetBrush("heading");
        MessageBoxEx.MessageBackground = Theme.Current?.GetBrush("background");
        MessageBoxEx.ButtonBackground = Theme.Current?.GetBrush("highlight");
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
        if (Player == null || !bPlayerinited)
            return;
        if (Player.IsQuitNeeded)
            Player.CommandV("quit");

        if (!Player.ShutdownAutoResetEvent.WaitOne(10000))
            Msg.ShowError(_("Shutdown thread failed to complete within 10 seconds."));

        Player.Destroy();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseDownLocation = PointToScreen(e.Location);
    }

    //protected override void OnMouseMove(MouseEventArgs e)
    //{
    //    base.OnMouseMove(e);

    //    if (App.Settings.Enable3DMode == false && IsCursorPosDifferent(_mouseDownLocation) &&
    //        WindowState == FormWindowState.Normal &&
    //        e.Button == MouseButtons.Left && !IsMouseInOsc() &&
    //        Player.GetPropertyBool("window-dragging"))
    //    {
    //        var HTCAPTION = new IntPtr(2);
    //        var WM_NCLBUTTONDOWN = 0xA1;
    //        ReleaseCapture();
    //        PostMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
    //    }
    //}

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        SaveWindowProperties();
    }

    void ShowCursor()
    {
        ShowVideoUI();
        overlayPanel.Cursor = Cursors.Default;
        Cursor.Show();
    }

    void ShowVideoUI()
    {
        if (App.Settings.Enable3DMode)
        {            
            btnBackRight.Left = Width / 2 + btnBackLeft.Left;
            btn3DRight.Left = Width / 2 + btn3DLeft.Left;
            btn3DSubtitleModeRight.Left = Width / 2 + btn3DSubtitleModeLeft.Left;            
            btnVideoTrackRight.Left = Width / 2 + btnVideoTrackLeft.Left;
            btnAudioTrackRight.Left = Width / 2 + btnAudioTrackLeft.Left;
            btnSubtitleTrackRight.Left = Width / 2 + btnSubtitleTrackLeft.Left;
            btnFullHalfRight.Left = Width / 2 + btnFullHalfLeft.Left;
            btnPlayRight.Left = btnPlayLeft.Left - Width / 2;
            //btnFullScreenLeft.Left = btnFullScreenRight.Left - Width / 2;
            progressBarRight.Left = Width / 2 + progressBarLeft.Left;
            progressBarLeft.Width = (int)(0.95 * Width) / 2;
            progressBarRight.Width = progressBarLeft.Width;

            btnBackLeft.Visible = true;
            btn3DSubtitleModeLeft.Visible = true;
            btn3DLeft.Visible = true;
            progressBarLeft.Visible = true;
            btnVideoTrackLeft.Visible = true;
            btnAudioTrackLeft.Visible = true;
            btnSubtitleTrackLeft.Visible = true;
            btnFullHalfLeft.Visible = true;
            btnPlayLeft.Visible = true;
            btnFullScreenLeft.Visible = false;
            

            btnBackRight.Visible = true;
            btn3DSubtitleModeRight.Visible = true;
            btn3DRight.Visible = true;
            progressBarRight.Visible = true;
            btnVideoTrackRight.Visible = true;
            btnAudioTrackRight.Visible = true;
            btnSubtitleTrackRight.Visible = true;
            btnFullHalfRight.Visible = true;
            btnPlayRight.Visible = true;
            //btnFullScreenRight.Visible = false;
        }
        else
        {
            progressBarLeft.Width = (int)(0.95 * Width);
            btnBackLeft.Visible = true;
            btn3DSubtitleModeLeft.Visible = true;
            btn3DLeft.Visible = true;
            progressBarLeft.Visible = true;
            btnVideoTrackLeft.Visible = true;
            btnAudioTrackLeft.Visible = true;
            btnSubtitleTrackLeft.Visible = true;
            btnFullHalfLeft.Visible = true;
            btnPlayLeft.Visible = true;
            btnFullScreenLeft.Visible = true;            

            btnBackRight.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btn3DRight.Visible = false;
            progressBarRight.Visible = false;
            btnVideoTrackRight.Visible = false;
            btnAudioTrackRight.Visible = false;
            btnSubtitleTrackRight.Visible = false;
            btnFullHalfRight.Visible = false;
            btnPlayRight.Visible = false;
            //btnFullScreenRight.Visible = false;            
        }
        btnBackLeft.BringToFront();
        btn3DSubtitleModeLeft.BringToFront();
        btn3DLeft.BringToFront();
        progressBarLeft.BringToFront();
        btnVideoTrackLeft.BringToFront();
        btnAudioTrackLeft.BringToFront();
        btnSubtitleTrackLeft.BringToFront();
        btnFullHalfLeft.BringToFront();
        btnPlayLeft.BringToFront();
        btnFullScreenLeft.BringToFront();

        btnBackRight.BringToFront();
        btn3DSubtitleModeRight.BringToFront();
        btn3DRight.BringToFront();
        progressBarRight.BringToFront();
        btnVideoTrackRight.BringToFront();
        btnAudioTrackRight.BringToFront();
        btnSubtitleTrackRight.BringToFront();
        btnFullHalfRight.BringToFront();
        btnPlayRight.BringToFront();
        btnFullScreenRight.BringToFront();
    }

    void HideVideoUI()
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
            btnFullHalfLeft.Visible = false;
            btnPlayLeft.Visible = false;
            btnFullScreenLeft.Visible = false;            

            btnBackRight.Visible = false;
            btn3DSubtitleModeRight.Visible = false;
            btn3DRight.Visible = false;
            progressBarRight.Visible = false;
            btnVideoTrackRight.Visible = false;
            btnAudioTrackRight.Visible = false;
            btnSubtitleTrackRight.Visible = false;
            btnFullHalfRight.Visible = false;
            btnPlayRight.Visible = false;
            //btnFullScreenRight.Visible = false;            
        }
    }
    void HideCursor()
    {
        HideVideoUI();
        Cursor.Hide();
        overlayPanel.Cursor = CreateTransparentCursor();
        //overlayPanel.Cursor = trancur;
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
        App.Settings.Save();
    }

    public static class v3DSubtitleMode
    {
        public static void Auto(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {
            if (Player.GetPropertyInt("width") > 3840)
            {
                Player.SetPropertyBool(CMD_sub_stereo_on, false);
            }
            else
            {
                Player.SetPropertyBool(CMD_sub_stereo_on, true);
                Player.SetPropertyBool("sub-stereo-duplicate", false);
            }
            Lbutton.Text = "3D字幕模式:自动";
            Rbutton.Text = "3D字幕模式:自动";
        }

        public static void On(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, true);
            Player.SetPropertyBool("sub-stereo-duplicate", false);
            Lbutton.Text = "3D字幕模式:双屏";
            Rbutton.Text = "3D字幕模式:双屏";
        }

        public static void Off(System.Windows.Forms.Label Lbutton, System.Windows.Forms.Label Rbutton)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, false);
            Lbutton.Text = "3D字幕模式:单屏";
            Rbutton.Text = "3D字幕模式:单屏";
        }
    }

    int isub = 0;
    private bool _isReturn2D;

    private void btnSubtitle_Click(object? sender, EventArgs e)
    {
        switch (isub)
        {
            case 0: //Auto模式转下一个
                v3DSubtitleMode.On(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
                isub = 1; //开
                break;
            case 1:
                v3DSubtitleMode.Off(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
                isub = 2;
                break;
            case 2:
                v3DSubtitleMode.Auto(btn3DSubtitleModeLeft, btn3DSubtitleModeRight);
                isub = 0;
                break;
            default:
                break;
        }
        _lastCursorChanged = Environment.TickCount;
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
            case Keys.Back:
                break;
            case Keys.Tab:
                break;
            case Keys.LineFeed:
                break;
            case Keys.Clear:
                break;
            case Keys.Enter:
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
            case Keys.Escape:
                if (App.Settings.Enable3DMode == false)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    btnBack_Click(null, null);
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
                break;
            case Keys.PageDown:
                break;
            case Keys.End:
                break;
            case Keys.Home:
                break;
            case Keys.Left:
                Player.Command("seek -5 relative");  // 从当前位置往后退 5 秒
                break;
            case Keys.Up:
                break;
            case Keys.Right:
                Player.Command("seek 5 relative");   // 从当前位置往前跳 5 秒
                break;
            case Keys.Down:
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
                break;
            case Keys.F12:
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
}
