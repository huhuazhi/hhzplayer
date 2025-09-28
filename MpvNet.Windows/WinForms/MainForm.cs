
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
        this.DoubleBuffered = true;     // 开启双缓冲，避免闪烁
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.OptimizedDoubleBuffer, true);
        this.UpdateStyles();

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
            //ShowCursor();
            //App.Settings.Enable3DMode = true;
            //InitializePlayer();
            //Player.Init(Handle, true);
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
    }
    //private void InitializePlayer()
    //{
    //    try
    //    {
    //        Instance = this;
    //        Player.FileLoaded += Player_FileLoaded;
    //        Player.Pause += Player_Pause;
    //        Player.PlaylistPosChanged += Player_PlaylistPosChanged;
    //        Player.Seek += UpdateProgressBar;
    //        Player.Shutdown += Player_Shutdown;
    //        Player.VideoSizeChanged += Player_VideoSizeChanged;
    //        Player.ClientMessage += Player_ClientMessage;

    //        GuiCommand.Current.ScaleWindow += GuiCommand_ScaleWindow;
    //        GuiCommand.Current.MoveWindow += GuiCommand_MoveWindow;
    //        GuiCommand.Current.WindowScaleNet += GuiCommand_WindowScaleNet;
    //        GuiCommand.Current.ShowMenu += GuiCommand_ShowMenu;

    //        Player.Init(Handle, true);

    //        Player.ObserveProperty("window-maximized", PropChangeWindowMaximized); // bool methods not working correctly
    //        Player.ObserveProperty("window-minimized", PropChangeWindowMinimized); // bool methods not working correctly
    //        Player.ObserveProperty("cursor-autohide", PropChangeCursorAutohide);
    //        //Player.ObservePropertyBool(CMD_sub_stereo_on, PropChangeSubStereoOn);

    //        Player.ObservePropertyBool("border", PropChangeBorder);
    //        Player.ObservePropertyBool("fullscreen", PropChangeFullscreen);
    //        Player.ObservePropertyBool("keepaspect-window", value => Player.KeepaspectWindow = value);
    //        //Player.ObservePropertyBool("ontop", PropChangeOnTop);
    //        Player.ObservePropertyBool("title-bar", PropChangeTitleBar);

    //        Player.ObservePropertyString("sid", PropChangeSid);
    //        Player.ObservePropertyString("aid", PropChangeAid);
    //        Player.ObservePropertyString("vid", PropChangeVid);

    //        Player.ObservePropertyString("title", PropChangeTitle);

    //        Player.ObservePropertyInt("edition", PropChangeEdition);

    //        Player.ObservePropertyDouble("window-scale", PropChangeWindowScale);

    //        CommandLine.ProcessCommandLineArgsPostInit();
    //        CommandLine.ProcessCommandLineFiles();

    //        _taskbarButtonCreatedMessage = RegisterWindowMessage("TaskbarButtonCreated");

    //        if (Player.Screen > -1)
    //        {
    //            int targetIndex = Player.Screen;
    //            Screen[] screens = Screen.AllScreens;

    //            if (targetIndex < 0)
    //                targetIndex = 0;

    //            if (targetIndex > screens.Length - 1)
    //                targetIndex = screens.Length - 1;

    //            Screen screen = screens[Array.IndexOf(screens, screens[targetIndex])];
    //            Rectangle target = screen.Bounds;
    //            Left = target.X + (target.Width - Width) / 2;
    //            Top = target.Y + (target.Height - Height) / 2;
    //        }

    //        if (!Player.Border)
    //            FormBorderStyle = FormBorderStyle.None;

    //        Point pos = App.Settings.WindowPosition;

    //        if ((pos.X != 0 || pos.Y != 0) && App.RememberWindowPosition)
    //        {
    //            Left = pos.X - Width / 2;
    //            Top = pos.Y - Height / 2;

    //            Point location = App.Settings.WindowLocation;

    //            if (location.X == -1) Left = pos.X;
    //            if (location.X == 1) Left = pos.X - Width;
    //            if (location.Y == -1) Top = pos.Y;
    //            if (location.Y == 1) Top = pos.Y - Height;
    //        }

    //        if (Player.WindowMaximized)
    //        {
    //            SetFormPosAndSize(true);
    //            WindowState = FormWindowState.Maximized;
    //        }

    //        if (Player.WindowMinimized)
    //        {
    //            SetFormPosAndSize(true);
    //            WindowState = FormWindowState.Minimized;
    //        }

    //        if (!App.Settings.Enable3DMode && App.StartSize == "always" && App.Settings.WindowSize != Size.Empty)
    //        {
    //            ClientSize = App.Settings.WindowSize;
    //        }
    //        bPlayerinited = true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Msg.ShowException(ex);
    //    }
    //}

    HHZMainPage hhzMainPage = new HHZMainPage();
    private void InitializehhzOverlay()
    {
        hhzMainPage.BringToFront();
        hhzMainPage.FileDropped += HhzMainPage_FileDropped;
        hhzMainPage.FileOpened += HhzMainPage_FileOpened;
        Controls.Add(hhzMainPage);
        overlayPanel = new System.Windows.Forms.Panel
        {
            Parent = this,
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
        };
        overlayPanel.Visible = false;

        overlayPanel.MouseMove += OverlayPanel_MouseMove;
        overlayPanel.MouseDoubleClick += OverlayPanel_MouseDoubleClick;

        Controls.Add(overlayPanel);

        Text = "hhzPlayer";

        Player.Init(overlayPanel.Handle, true);
        //Player.SetPropertyInt("wid", overlayPanel.Handle.ToInt32());

        //App.Settings.Enable3DMode = true;
        CycleFullScreenFor3D(App.Settings.Enable3DMode);

        btn3DLeft.Click += btn3D_Click;
        btn3DRight.Click += btn3D_Click;
        btnBackLeft.Click += btnBack_Click;
        btnBackRight.Click += btnBack_Click;
        progressBarLeft.MouseClick += ProgressBar_MouseClick;
        progressBarRight.MouseClick += ProgressBar_MouseClick;

        //hhzMainPage.MouseUp += (s, e) =>
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        ShowCursor();
        //        UpdateMenu();
        //        ContextMenu.IsOpen = true;
        //    }
        //};
    }
    private System.Drawing.Image LoadMyLogo()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylogo.png");
        if (File.Exists(path)) return new Bitmap(path);
        var bmp = new Bitmap(1, 1);
        using (var g = Graphics.FromImage(bmp)) g.Clear(Color.Transparent);
        return bmp;
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
    }

    private void btn3D_Click(object? sender, EventArgs e)
    {
        App.Settings.Enable3DMode = !App.Settings.Enable3DMode;
        CycleFullScreenFor3D(App.Settings.Enable3DMode);
        //Enable3DMode_Click(sender, null);
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
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
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
    private void HhzMainPage_FileOpened(object? sender, string path)
    {
        if (path.Length > 0)
        {
            hhzMainPage.Visible = false;
            overlayPanel.Visible = true;
            //ShowCursor();
            //App.Settings.Enable3DMode = true;
            //InitializePlayer();
            //Player.Init(Handle, true);
            Player.LoadFiles(new[] { path }, true, false);
            //hhzMainPage.BringToFront();
            //overlayPanel = new System.Windows.Forms.Panel
            //{
            //    Parent = this,
            //    Dock = DockStyle.Fill,
            //    BackColor = Color.Transparent,
            //};
            //Player.SetPropertyInt("wid", overlayPanel.Handle.ToInt32());
            //System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
            //button1.Bounds = new Rectangle(37, 37, 190, 105);
            //Controls.Add(overlayPanel);
            //Controls.Add(button1);
            //button1.BringToFront();
            //button1.Click += (s, e) =>
            //{
            //    Player.Command("stop");
            //    hhzMainPage.Visible = false;
            //};

            //overlayPanel.MouseMove += (s, e) =>
            //{
            //    ShowCursor();
            //    if (e.Button == MouseButtons.Right)
            //    {

            //    }
            //};
        }
    }

    private void HhzMainPage_FileDropped(object? sender, string[] files)
    {
        if (files != null && files.Length > 0)
        {
            hhzMainPage.Visible = false;
            //InitializePlayer();
            Player.LoadFiles(files, true, false); // 你项目里的 Player 调用
        }
    }

    void UpdateDarkMode()
    {
        if (Environment.OSVersion.Version >= new Version(10, 0, 18985))
            DwmSetWindowAttribute(Handle, 20, new[] { Theme.DarkMode ? 1 : 0 }, 4);  // DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    //void GuiCommand_ScaleWindow(float scale)
    //{
    //    BeginInvoke(() =>
    //    {
    //        int w, h;

    //        if (KeepSize())
    //        {
    //            w = (int)(ClientSize.Width * scale);
    //            h = (int)(ClientSize.Height * scale);
    //        }
    //        else
    //        {
    //            w = (int)(ClientSize.Width * scale);
    //            h = (int)Math.Floor(w * Player.VideoSize.Height / (double)Player.VideoSize.Width);
    //        }

    //        SetSize(w, h, Screen.FromControl(this), false);
    //    });
    //}

    void GuiCommand_MoveWindow(string direction)
    {
        BeginInvoke(() =>
        {
            Screen screen = Screen.FromControl(this);
            Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

            switch (direction)
            {
                case "left":
                    Left = workingArea.Left;
                    break;
                case "top":
                    Top = 0;
                    break;
                case "right":
                    Left = workingArea.Width - Width + workingArea.Left;
                    break;
                case "bottom":
                    Top = workingArea.Height - Height;
                    break;
                case "center":
                    Left = (screen.Bounds.Width - Width) / 2;
                    Top = (screen.Bounds.Height - Height) / 2;
                    break;
            }
        });
    }

    //void GuiCommand_WindowScaleNet(float scale)
    //{
    //    BeginInvoke(() =>
    //    {
    //        SetSize(
    //            (int)(Player.VideoSize.Width * scale),
    //            (int)Math.Floor(Player.VideoSize.Height * scale),
    //            Screen.FromControl(this), false);
    //        Player.Command($"show-text \"window-scale {scale.ToString(CultureInfo.InvariantCulture)}\"");
    //    });
    //}

    void GuiCommand_ShowMenu()
    {
        BeginInvoke(() =>
        {
            if (IsMouseInOsc())
                return;

            ShowCursor();
            UpdateMenu();
            ContextMenu.IsOpen = true;
        });
    }

    bool IsFullscreen => WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None;

    bool KeepSize() => App.StartSize == "session" || App.StartSize == "always";

    bool IsMouseInOsc()
    {
        Point pos = PointToClient(MousePosition);
        float top = 0;

        if (!Player.Border)
            top = ClientSize.Height * 0.1f;

        return pos.X < ClientSize.Width * 0.1 ||
               pos.X > ClientSize.Width * 0.9 ||
               pos.Y < top ||
               pos.Y > ClientSize.Height * 0.78;
    }

    void UpdateMenu()
    {
        Player.UpdateExternalTracks();

        lock (Player.MediaTracksLock)
        {
            var trackMenuItem = FindMenuItem(_("Track"), "Track");

            if (trackMenuItem != null)
            {
                trackMenuItem.Items.Clear();

                var audTracks = Player.MediaTracks.Where(track => track.Type == "a");
                var subTracks = Player.MediaTracks.Where(track => track.Type == "s");
                var vidTracks = Player.MediaTracks.Where(track => track.Type == "v");
                var ediTracks = Player.MediaTracks.Where(track => track.Type == "e");

                foreach (MediaTrack track in vidTracks)
                {
                    var menuItem = new MenuItem() { Header = track.Text.Replace("_", "__") };
                    menuItem.Click += (sender, args) => Player.CommandV("set", "vid", track.ID.ToString());
                    menuItem.IsChecked = Player.VID == track.ID.ToString();
                    trackMenuItem.Items.Add(menuItem);
                }

                if (vidTracks.Any())
                    trackMenuItem.Items.Add(new Separator());

                foreach (MediaTrack track in audTracks)
                {
                    var menuItem = new MenuItem() { Header = track.Text.Replace("_", "__") };
                    menuItem.Click += (sender, args) => Player.CommandV("set", "aid", track.ID.ToString());
                    menuItem.IsChecked = Player.AID == track.ID.ToString();
                    trackMenuItem.Items.Add(menuItem);
                }

                if (subTracks.Any())
                    trackMenuItem.Items.Add(new Separator());

                foreach (MediaTrack track in subTracks)
                {
                    var menuItem = new MenuItem() { Header = track.Text.Replace("_", "__") };
                    menuItem.Click += (sender, args) => Player.CommandV("set", "sid", track.ID.ToString());
                    menuItem.IsChecked = Player.SID == track.ID.ToString();
                    trackMenuItem.Items.Add(menuItem);
                }

                if (subTracks.Any())
                {
                    var menuItem = new MenuItem() { Header = "S: No subtitles" };
                    menuItem.Click += (sender, args) => Player.CommandV("set", "sid", "no");
                    menuItem.IsChecked = Player.SID == "no";
                    trackMenuItem.Items.Add(menuItem);
                }

                if (ediTracks.Any())
                    trackMenuItem.Items.Add(new Separator());

                foreach (MediaTrack track in ediTracks)
                {
                    var menuItem = new MenuItem() { Header = track.Text.Replace("_", "__") };
                    menuItem.Click += (sender, args) => Player.CommandV("set", "edition", track.ID.ToString());
                    menuItem.IsChecked = Player.Edition == track.ID;
                    trackMenuItem.Items.Add(menuItem);
                }
            }
        }

        var chaptersMenuItem = FindMenuItem(_("Chapter"), "Chapters");

        if (chaptersMenuItem != null)
        {
            chaptersMenuItem.Items.Clear();

            foreach (Chapter chapter in Player.GetChapters())
            {
                var menuItem = new MenuItem
                {
                    Header = chapter.Title,
                    InputGestureText = chapter.TimeDisplay
                };

                menuItem.Click += (sender, args) =>
                    Player.CommandV("seek", chapter.Time.ToString(CultureInfo.InvariantCulture), "absolute");

                chaptersMenuItem.Items.Add(menuItem);
            }
        }

        var recentMenuItem = FindMenuItem(_("Recent Files"), "Recent");

        if (recentMenuItem != null)
        {
            recentMenuItem.Items.Clear();

            foreach (string path in App.Settings.RecentFiles)
            {
                var file = AppClass.GetTitleAndPath(path);
                var menuItem = MenuHelp.Add(recentMenuItem.Items, file.Title.ShortPath(100));

                if (menuItem != null)
                    menuItem.Click += (sender, args) => Player.LoadFiles(new[] { file.Path }, true, false);
            }

            recentMenuItem.Items.Add(new Separator());
            var clearMenuItem = new MenuItem() { Header = _("Clear List") };
            clearMenuItem.Click += (sender, args) => App.Settings.RecentFiles.Clear();
            recentMenuItem.Items.Add(clearMenuItem);
        }

        var titlesMenuItem = FindMenuItem(_("Title"), "Titles");

        if (titlesMenuItem != null)
        {
            titlesMenuItem.Items.Clear();

            lock (Player.BluRayTitles)
            {
                List<(int Index, TimeSpan Length)> items = new List<(int, TimeSpan)>();

                for (int i = 0; i < Player.BluRayTitles.Count; i++)
                    items.Add((i, Player.BluRayTitles[i]));

                var titleItems = items.OrderByDescending(item => item.Length)
                                      .Take(20)
                                      .OrderBy(item => item.Index);

                foreach (var item in titleItems)
                {
                    if (item.Length != TimeSpan.Zero)
                    {
                        var menuItem = MenuHelp.Add(titlesMenuItem.Items, $"Title {item.Index + 1}");

                        if (menuItem != null)
                        {
                            menuItem.InputGestureText = item.Length.ToString();
                            menuItem.Click += (sender, args) => Player.SetBluRayTitle(item.Index);
                        }
                    }
                }
            }
        }

        var profilesMenuItem = FindMenuItem(_("Profile"), "Profile");

        if (profilesMenuItem != null && !profilesMenuItem.HasItems)
        {
            foreach (string profile in Player.ProfileNames)
            {
                if (!profile.StartsWith("extension."))
                {
                    var menuItem = MenuHelp.Add(profilesMenuItem.Items, profile);

                    if (menuItem != null)
                    {
                        menuItem.Click += (sender, args) =>
                        {
                            Player.CommandV("show-text", profile);
                            Player.CommandV("apply-profile", profile);
                        };
                    }
                }
            }

            profilesMenuItem.Items.Add(new Separator());
            var showProfilesMenuItem = new MenuItem() { Header = _("Show Profiles") };
            showProfilesMenuItem.Click += (sender, args) => Player.Command("script-message-to mpvnet show-profiles");
            profilesMenuItem.Items.Add(showProfilesMenuItem);
        }

        var audioDevicesMenuItem = FindMenuItem(_("Audio Device"), "Audio Device");

        if (audioDevicesMenuItem != null)
        {
            audioDevicesMenuItem.Items.Clear();

            foreach (var pair in Player.AudioDevices)
            {
                var menuItem = MenuHelp.Add(audioDevicesMenuItem.Items, pair.Value);

                if (menuItem != null)
                {
                    menuItem.IsChecked = pair.Name == Player.GetPropertyString("audio-device");

                    menuItem.Click += (sender, args) =>
                    {
                        Player.SetPropertyString("audio-device", pair.Name);
                        Player.CommandV("show-text", pair.Value);
                        App.Settings.AudioDevice = pair.Name;
                    };
                }
            }
        }

        var customMenuItem = FindMenuItem(_("Custom"), "Custom");

        if (customMenuItem != null && !customMenuItem.HasItems)
        {
            var customBindings = _confBindings!.Where(it => it.IsCustomMenu);

            if (customBindings.Any())
            {
                foreach (Binding binding in customBindings)
                {
                    var menuItem = MenuHelp.Add(customMenuItem.Items, binding.Comment);

                    if (menuItem != null)
                    {
                        menuItem.Click += (sender, args) => Player.Command(binding.Command);
                        menuItem.InputGestureText = binding.Input;
                    }
                }
            }
            else
            {
                if (ContextMenu.Items.Contains(customMenuItem))
                    ContextMenu.Items.Remove(customMenuItem);
            }
        }


        if (_3DModeMenuItem != null && !_3DModeMenuItem.Items.Contains(_s3DModeSwitchMenuItem))
        {
            _3DModeMenuItem.Items.Add(_s3DModeSwitchMenuItem);
        }

        if (_3DModeMenuItem != null && !_3DModeMenuItem.Items.Contains(_SbsSubMemuItem))
        {
            _3DModeMenuItem.Items.Add(_SbsSubMemuItem);
        }

        if (_sSbsSubAutoMenuItem != null && _SbsSubMemuItem?.Items.Contains(_sSbsSubAutoMenuItem) == false)
        {
            _SbsSubMemuItem.Items.Add(_sSbsSubAutoMenuItem);
        }
        if (_sSbsSubOnMenuItem != null && _SbsSubMemuItem?.Items.Contains(_sSbsSubOnMenuItem) == false)
        {
            _SbsSubMemuItem.Items.Add(_sSbsSubOnMenuItem);
        }
        if (_sSbsSubOffMenuItem != null && _SbsSubMemuItem?.Items.Contains(_sSbsSubOffMenuItem) == false)
        {
            _SbsSubMemuItem.Items.Add(_sSbsSubOffMenuItem);
        }


        if (!ContextMenu.Items.Contains(_3DModeMenuItem))
        {
            ContextMenu.Items.Add(_3DModeMenuItem);
        }
    }

    private void SbsSubAutoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        //_sSbsSubAutoMenuItem.IsChecked = true;
        //_sSbsSubOnMenuItem.IsChecked = false;
        //_sSbsSubOffMenuItem.IsChecked = false;

        if (Player.GetPropertyInt("width") > 3840)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, false);
        }
        else
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, true);
            Player.SetPropertyBool("sub-stereo-duplicate", false);
        }
    }

    private void SbsSubOnMemuItem_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _sSbsSubAutoMenuItem.IsChecked = false;
        _sSbsSubOnMenuItem.IsChecked = true;
        _sSbsSubOffMenuItem.IsChecked = false;
        Player.SetPropertyBool(CMD_sub_stereo_on, true);
        Player.SetPropertyBool("sub-stereo-duplicate", false);
    }

    private void SbsSubOffMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _sSbsSubAutoMenuItem.IsChecked = false;
        _sSbsSubOnMenuItem.IsChecked = false;
        _sSbsSubOffMenuItem.IsChecked = true;
        Player.SetPropertyBool(CMD_sub_stereo_on, false);
    }

    public MenuItem? FindMenuItem(string text, string text2 = "")
    {
        var ret = FindMenuItem(text, ContextMenu.Items);

        if (ret == null && text2 != "")
            return FindMenuItem(text2, ContextMenu.Items);

        return ret;
    }

    MenuItem? FindMenuItem(string text, ItemCollection? items)
    {
        foreach (object item in items!)
        {
            if (item is MenuItem mi)
            {
                if (mi.Header.ToString().StartsWithEx(text) && mi.Header.ToString().TrimEx() == text)
                    return mi;

                if (mi.Items.Count > 0)
                {
                    MenuItem? val = FindMenuItem(text, mi.Items);

                    if (val != null)
                        return val;
                }
            }
        }

        return null;
    }

    //void SetFormPosAndSize(bool force = false, bool checkAutofit = true, bool load = false)
    //{
    //    Debug.WriteLine($"force={force}, App.AutoFitImage={App.AutofitImage},Player.Autofit={Player.Autofit},Player.VideoSize={Player.VideoSize}");
    //    if (!force)
    //    {
    //        if (WindowState != FormWindowState.Normal)
    //            return;

    //        if (Player.Fullscreen)
    //        {
    //            CycleFullscreen(true);
    //            return;
    //        }
    //    }

    //    Screen screen = Screen.FromControl(this);
    //    Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
    //    int autoFitHeight = Convert.ToInt32(workingArea.Height * Player.Autofit);

    //    if (App.AutofitAudio > 1)
    //        App.AutofitAudio = 1;

    //    if (App.AutofitImage > 1)
    //        App.AutofitImage = 1;

    //    bool isAudio = FileTypes.IsAudio(Player.Path.Ext());

    //    if (isAudio)
    //        autoFitHeight = Convert.ToInt32(workingArea.Height * App.AutofitAudio);

    //    if (FileTypes.IsImage(Player.Path.Ext()))
    //        autoFitHeight = Convert.ToInt32(workingArea.Height * App.AutofitImage);

    //    if (Player.VideoSize.Height == 0 || Player.VideoSize.Width == 0)
    //        Player.VideoSize = new Size((int)(autoFitHeight * (16 / 9f)), autoFitHeight);

    //    float minAspectRatio = isAudio ? App.MinimumAspectRatioAudio : App.MinimumAspectRatio;

    //    if (minAspectRatio != 0 && Player.VideoSize.Width / (float)Player.VideoSize.Height < minAspectRatio)
    //        Player.VideoSize = new Size((int)(autoFitHeight * minAspectRatio), autoFitHeight);

    //    Size videoSize = Player.VideoSize;

    //    int height = videoSize.Height;
    //    int width = videoSize.Width;

    //    if (App.StartSize == "previous")
    //        App.StartSize = "height-session";

    //    Debug.WriteLine($"CLientSize={ClientSize},App.Settings.WindowSize={App.Settings.WindowSize}");

    //    if (Player.WasInitialSizeSet)
    //    {
    //        if (KeepSize())
    //        {
    //            width = ClientSize.Width;
    //            height = ClientSize.Height;
    //        }
    //        else if (App.StartSize == "height-always" || App.StartSize == "height-session")
    //        {
    //            height = ClientSize.Height;
    //            width = (int)Math.Ceiling(height * videoSize.Width / (double)videoSize.Height);
    //        }
    //        else if (App.StartSize == "width-always" || App.StartSize == "width-session")
    //        {
    //            width = ClientSize.Width;
    //            height = (int)Math.Floor(width * videoSize.Height / (double)videoSize.Width);
    //        }
    //    }
    //    else
    //    {
    //        Size windowSize = App.Settings.WindowSize;

    //        if (App.StartSize == "height-always" && windowSize.Height != 0)
    //        {
    //            height = windowSize.Height;
    //            width = (int)Math.Ceiling(height * videoSize.Width / (double)videoSize.Height);
    //        }
    //        else if (App.StartSize == "height-session" || App.StartSize == "session")
    //        {
    //            height = autoFitHeight;
    //            width = (int)Math.Ceiling(height * videoSize.Width / (double)videoSize.Height);
    //        }
    //        else if (App.StartSize == "width-always" && windowSize.Height != 0)
    //        {
    //            width = windowSize.Width;
    //            height = (int)Math.Floor(width * videoSize.Height / (double)videoSize.Width);
    //        }
    //        else if (App.StartSize == "width-session")
    //        {
    //            width = autoFitHeight / 9 * 16;
    //            height = (int)Math.Floor(width * videoSize.Height / (double)videoSize.Width);
    //        }
    //        else if (App.StartSize == "always" && windowSize.Height != 0)
    //        {
    //            height = windowSize.Height;
    //            width = windowSize.Width;
    //        }

    //        Player.WasInitialSizeSet = true;
    //    }

    //    SetSize(width, height, screen, checkAutofit, load);
    //}

    //void SetSize(int width, int height, Screen screen, bool checkAutofit = true, bool load = false)
    //{
    //    Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

    //    int maxHeight = workingArea.Height - (Height - ClientSize.Height) - 2;
    //    int maxWidth = workingArea.Width - (Width - ClientSize.Width);

    //    int startWidth = width;
    //    int startHeight = height;

    //    if (checkAutofit)
    //    {
    //        if (height < maxHeight * Player.AutofitSmaller)
    //        {
    //            height = (int)(maxHeight * Player.AutofitSmaller);
    //            width = (int)Math.Ceiling(height * startWidth / (double)startHeight);
    //        }

    //        if (height > maxHeight * Player.AutofitLarger)
    //        {
    //            height = (int)(maxHeight * Player.AutofitLarger);
    //            width = (int)Math.Ceiling(height * startWidth / (double)startHeight);
    //        }
    //    }

    //    if (width > maxWidth)
    //    {
    //        width = maxWidth;
    //        height = (int)Math.Floor(width * startHeight / (double)startWidth);
    //    }

    //    if (height > maxHeight)
    //    {
    //        height = maxHeight;
    //        width = (int)Math.Ceiling(height * startWidth / (double)startHeight);
    //    }

    //    if (height < maxHeight * 0.1)
    //    {
    //        height = (int)(maxHeight * 0.1);
    //        width = (int)Math.Ceiling(height * startWidth / (double)startHeight);
    //    }

    //    Point middlePos = new Point(Left + Width / 2, Top + Height / 2);
    //    var rect = new RECT(new Rectangle(screen.Bounds.X, screen.Bounds.Y, width, height));

    //    AddWindowBorders(Handle, ref rect, GetDpi(Handle), !Player.TitleBar);

    //    width = rect.Width;
    //    height = rect.Height;

    //    int left = Convert.ToInt32(middlePos.X - width / 2.0);
    //    int top = Convert.ToInt32(middlePos.Y - height / 2.0);

    //    if (!Player.TitleBar)
    //        top -= Convert.ToInt32(GetTitleBarHeight(Handle, GetDpi(Handle)) / 2.0);

    //    Rectangle currentRect = new Rectangle(Left, Top, Width, Height);

    //    if (GetHorizontalLocation(screen) == -1) left = Left;
    //    if (GetHorizontalLocation(screen) == 1) left = currentRect.Right - width;

    //    if (GetVerticalLocation(screen) == -1) top = Top;
    //    if (GetVerticalLocation(screen) == 1) top = currentRect.Bottom - height;

    //    Screen[] screens = Screen.AllScreens;

    //    int minLeft = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).X).Min();
    //    int maxRight = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Right).Max();
    //    int minTop = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Y).Min();
    //    int maxBottom = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Bottom).Max();

    //    if (load)
    //    {
    //        string geometryString = Player.GetPropertyString("geometry");

    //        if (!string.IsNullOrEmpty(geometryString))
    //        {
    //            var pos = ParseGeometry(geometryString, width, height);

    //            if (pos.X != int.MaxValue)
    //                left = pos.X;

    //            if (pos.Y != int.MaxValue)
    //                top = pos.Y;
    //        }
    //    }

    //    if (left < minLeft)
    //        left = minLeft;

    //    if (left + width > maxRight)
    //        left = maxRight - width;

    //    if (top < minTop)
    //        top = minTop;

    //    if (top + height > maxBottom)
    //        top = maxBottom - height;

    //    uint SWP_NOACTIVATE = 0x0010;
    //    SetWindowPos(Handle, IntPtr.Zero, left, top, width, height, SWP_NOACTIVATE);
    //}

    Point ParseGeometry(string input, int width, int height)
    {
        int x = int.MaxValue;
        int y = int.MaxValue;

        Match match = Regex.Match(input, @"^\+(\d+)%?\+(\d+)%?$");

        if (match.Success)
        {
            Rectangle workingArea = GetWorkingArea(Handle, Screen.FromHandle(Handle).WorkingArea);

            x = int.Parse(match.Groups[1].Value);
            y = int.Parse(match.Groups[2].Value);

            x = workingArea.Left + Convert.ToInt32((workingArea.Width - width) / 100.0 * x);
            y = workingArea.Top + Convert.ToInt32((workingArea.Height - height) / 100.0 * y);
        }

        return new Point(x, y);
    }

    //private void CycleFullscreen(bool enabled/*, bool forceToNornam = false*/)
    //{
    //    _lastCycleFullscreen = Environment.TickCount;
    //    Player.Fullscreen = enabled;

    //    if (enabled)
    //    {
    //        if (WindowState != FormWindowState.Maximized || FormBorderStyle != FormBorderStyle.None)
    //        {
    //            FormBorderStyle = FormBorderStyle.None;
    //            WindowState = FormWindowState.Maximized;
    //            if (_wasMaximized)
    //            {
    //                Rectangle bounds = Screen.FromControl(this).Bounds;
    //                uint SWP_SHOWWINDOW = 0x0040;
    //                IntPtr HWND_TOP = IntPtr.Zero;
    //                SetWindowPos(Handle, HWND_TOP, bounds.X, bounds.Y, bounds.Width, bounds.Height, SWP_SHOWWINDOW);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if ((WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None) /*|| forceToNornam*/)
    //        {
    //            if (_wasMaximized)
    //                WindowState = FormWindowState.Maximized;
    //            else
    //            {
    //                WindowState = FormWindowState.Normal;

    //                if (!Player.WasInitialSizeSet)
    //                    SetFormPosAndSize();
    //            }

    //            FormBorderStyle = Player.Border ? FormBorderStyle.Sizable : FormBorderStyle.None;

    //            if (!KeepSize() /*|| forceToNornam*/)
    //                SetFormPosAndSize();
    //        }
    //    }
    //}

    Rectangle prebounds;
    private bool bPlayerinited;
    private System.Windows.Forms.Panel overlayPanel;

    private void CycleFullScreenFor3D(bool enable3DMode)
    {
        //Player.Fullscreen = enable3DSubtitle;
        if (enable3DMode)
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            bounds.Width = bounds.Width * 2;
            if (FormBorderStyle != FormBorderStyle.None) FormBorderStyle = FormBorderStyle.None;
            if (WindowState != FormWindowState.Normal) WindowState = FormWindowState.Normal;
            this.Bounds = bounds;
            //uint SWP_SHOWWINDOW = 0x0040;
            //IntPtr HWND_TOP = IntPtr.Zero;
            //SetWindowPos(Handle, HWND_TOP, bounds.X, bounds.Y, bounds.Width, bounds.Height, SWP_SHOWWINDOW);
            if (Player.Duration.TotalMicroseconds > 0)
            {
                var vw = Player.GetPropertyInt("width");
                var vh = Player.GetPropertyInt("height");
                if (vw / vh <= 2.35) // half-SBS
                {
                    Player.SetPropertyString("video-aspect-override", (vw * 2).ToString() + ":" + vh.ToString());
                }
                else // full-SBS
                {
                    Player.SetPropertyString("video-aspect-override", vw.ToString() + ":" + vh.ToString());
                }
            }
            btnBackRight.Left = Screen.PrimaryScreen.Bounds.Width + btnBackLeft.Left;
            btn3DSubtitleModeRight.Left = Screen.PrimaryScreen.Bounds.Width + btn3DSubtitleModeLeft.Left;
            btn3DRight.Left = Screen.PrimaryScreen.Bounds.Width + btn3DLeft.Left;
            progressBarRight.Left = Screen.PrimaryScreen.Bounds.Width + progressBarLeft.Left;
            progressBarLeft.Width = (int)(0.8397 * Width / 2);
            progressBarRight.Width = progressBarLeft.Width;
        }
        else
        {
            if (Player.Duration.TotalMicroseconds > 0)
            {
                var vw = Player.GetPropertyInt("width");
                var vh = Player.GetPropertyInt("height");
                Player.SetPropertyString("video-aspect-override", vw.ToString() + ":" + vh.ToString());
            }

            Bounds = new Rectangle(App.Settings.WindowLocation.X, App.Settings.WindowLocation.Y,
                                   App.Settings.WindowSize.Width, App.Settings.WindowSize.Height);
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.Sizable;

            progressBarLeft.Width = (int)(0.8397 * Width);
            progressBarRight.Width = progressBarLeftWidth;
        }
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

    //public void InitAndBuildContextMenu()
    //{
    //    ContextMenu.Closed += ContextMenu_Closed;
    //    ContextMenu.UseLayoutRounding = true;

    //    var (menuBindings, confBindings) = App.InputConf.GetBindings();
    //    _confBindings = confBindings;
    //    var activeBindings = InputHelp.GetActiveBindings(menuBindings);

    //    foreach (Binding binding in menuBindings)
    //    {
    //        Binding tempBinding = binding;

    //        if (!binding.IsMenu)
    //            continue;

    //        var menuItem = MenuHelp.Add(ContextMenu.Items, tempBinding.Comment);

    //        if (menuItem != null)
    //        {
    //            menuItem.Click += (sender, args) =>
    //            {
    //                try
    //                {
    //                    TaskHelp.Run(() =>
    //                    {
    //                        MenuAutoResetEvent.WaitOne();
    //                        System.Windows.Application.Current.Dispatcher.Invoke(
    //                            DispatcherPriority.Background, new Action(delegate { }));
    //                        if (!string.IsNullOrEmpty(tempBinding.Command))
    //                        {
    //                            Player.Command(tempBinding.Command);          //通过向libmpv发送“script-message-to mpvnet open-files”来触发打开播放文件           
    //                            //Player.LoadFiles(new string[] { @"D:\Movies\3D\Avatar.The.Way.Of.Water.2022.2160p.3D.Half-SBS.Ai-Upscaled.HEVC.DDP7.1-90fps-Chs-Cht-Eng-HUHUAZHI.mkv" }, true, false); //通过调用LoadFiles函数来触发打开播放文件                                
    //                        }
    //                    });
    //                }
    //                catch (Exception ex)
    //                {
    //                    Msg.ShowException(ex);
    //                }
    //            };

    //            menuItem.InputGestureText = InputHelp.GetBindingsForCommand(activeBindings, tempBinding.Command);
    //        }
    //    }

    //    _contextMenuIsReady = true;
    //}

    /// <summary>
    /// 生成右眼偏移字幕
    /// </summary>
    static void MakeOffsetSubtitle(string input, string output, int videoWidth)
    {
        var lines = File.ReadAllLines(input, Encoding.UTF8);
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            if (line.StartsWith("Dialogue:"))
            {
                string modified = Regex.Replace(line, @"\\pos\((\d+),(\d+)\)", m =>
                {
                    int x = int.Parse(m.Groups[1].Value) + videoWidth / 2;
                    int y = int.Parse(m.Groups[2].Value);
                    return $"\\pos({x},{y})";
                });

                sb.AppendLine(modified);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        File.WriteAllText(output, sb.ToString(), Encoding.UTF8);
    }

    void SetTitle() => BeginInvoke(SetTitleInternal);

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
            if (WindowState == FormWindowState.Normal && WasShown)
            {
                progressBarLeftWidth = progressBarLeft.Width;
                App.Settings.WindowPosition = new Point(Left, Top);
                App.Settings.WindowLocation = new Point(Left, Top);
                App.Settings.WindowSize = new Size(Width, Height);
            }
        }
    }

    void SavePosition()
    {
        //Point pos = new Point(Left + Width / 2, Top + Height / 2);
        //Screen screen = Screen.FromControl(this);

        //int x = GetHorizontalLocation(screen);
        //int y = GetVerticalLocation(screen);

        //if (x == -1) pos.X = Left;
        //if (x == 1) pos.X = Left + Width;
        //if (y == -1) pos.Y = Top;
        //if (y == 1) pos.Y = Top + Height;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.Style |= 0x00020000 /* WS_MINIMIZEBOX */;
            return cp;
        }
    }

    //protected override void WndProc(ref Message m)
    //{
    //    switch (m.Msg)
    //    {
    //        case 0x0007: // WM_SETFOCUS
    //        case 0x0008: // WM_KILLFOCUS
    //        //case 0x0014: // WM_ERASEBKGND
    //        //    m.Result = IntPtr.Zero;
    //        //    break;
    //        case 0x0021: // WM_MOUSEACTIVATE
    //        case 0x0100: // WM_KEYDOWN
    //        case 0x0101: // WM_KEYUP
    //        case 0x0104: // WM_SYSKEYDOWN
    //        case 0x0105: // WM_SYSKEYUP
    //        case 0x0201: // WM_LBUTTONDOWN
    //        case 0x0202: // WM_LBUTTONUP
    //        case 0x0204: // WM_RBUTTONDOWN
    //        case 0x0205: // WM_RBUTTONUP
    //        case 0x0206: // WM_RBUTTONDBLCLK
    //        case 0x0207: // WM_MBUTTONDOWN
    //        case 0x0208: // WM_MBUTTONUP
    //        case 0x0209: // WM_MBUTTONDBLCLK
    //        case 0x020a: // WM_MOUSEWHEEL
    //        case 0x020b: // WM_XBUTTONDOWN
    //        case 0x020c: // WM_XBUTTONUP
    //        case 0x020e: // WM_MOUSEHWHEEL
    //        case 0x0280: // WM_IME_REPORT
    //        case 0x0281: // WM_IME_SETCONTEXT
    //        case 0x0282: // WM_IME_NOTIFY
    //        case 0x0283: // WM_IME_CONTROL
    //        case 0x0284: // WM_IME_COMPOSITIONFULL
    //        case 0x0285: // WM_IME_SELECT
    //        case 0x0286: // WM_IME_CHAR
    //        case 0x0288: // WM_IME_REQUEST
    //        case 0x0290: // WM_IME_KEYDOWN
    //        case 0x0291: // WM_IME_KEYUP
    //        case 0x02a3: // WM_MOUSELEAVE
    //            {
    //                bool ignore = false;

    //                if (m.Msg == 0x0100) // WM_KEYDOWN
    //                {
    //                    Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;

    //                    if (keyCode == Keys.Escape && _contextMenuIsReady && ContextMenu.IsOpen)
    //                    {
    //                        ignore = true;
    //                        ContextMenu.IsOpen = false;
    //                    }
    //                }

    //                if (MpvWindowHandle == IntPtr.Zero)
    //                    MpvWindowHandle = FindWindowEx(Handle, IntPtr.Zero, "mpv", null);

    //                if (MpvWindowHandle != IntPtr.Zero && !ignore)
    //                    m.Result = SendMessage(MpvWindowHandle, m.Msg, m.WParam, m.LParam);
    //            }
    //            break;
    //        case 0x001A: // WM_SETTINGCHANGE
    //            UpdateDarkMode();
    //            break;
    //        case 0x51: // WM_INPUTLANGCHANGE
    //            ActivateKeyboardLayout(m.LParam, 0x00000100u /*KLF_SETFORPROCESS*/);
    //            break;
    //        case 0x319: // WM_APPCOMMAND
    //            {
    //                string? key = MpvHelp.WM_APPCOMMAND_to_mpv_key((int)(m.LParam.ToInt64() >> 16 & ~0xf000));
    //                bool inputMediaKeys = Player.GetPropertyBool("input-media-keys");

    //                if (key != null && inputMediaKeys)
    //                {
    //                    Player.Command("keypress " + key);
    //                    m.Result = new IntPtr(1);
    //                    return;
    //                }
    //            }
    //            break;
    //        case 0x312: // WM_HOTKEY
    //            GlobalHotkey.Execute(m.WParam.ToInt32());
    //            break;
    //        case 0x200: // WM_MOUSEMOVE
    //            if (Environment.TickCount - _lastCycleFullscreen > 500)
    //            {
    //                Point pos = PointToClient(Cursor.Position);
    //                Player.Command($"mouse {pos.X} {pos.Y}");
    //            }
    //            if (IsCursorPosDifferent(_lastCursorPosition))
    //                ShowCursor();
    //            break;
    //        case 0x203: // WM_LBUTTONDBLCLK
    //            if (!App.Settings.Enable3DMode)
    //            {
    //                Point pos = PointToClient(Cursor.Position);
    //                Player.Command($"mouse {pos.X} {pos.Y} 0 double");
    //            }
    //            break;
    //        case 0x2E0: // WM_DPICHANGED
    //            {
    //                if (!WasShown)
    //                    break;

    //                RECT rect = Marshal.PtrToStructure<RECT>(m.LParam);
    //                SetWindowPos(Handle, IntPtr.Zero, rect.Left, rect.Top, rect.Width, rect.Height, 0);
    //            }
    //            break;
    //        case 0x0112: // WM_SYSCOMMAND
    //            {
    //                // with title-bar=no when the window is restored from minimizing the height is too high  
    //                if (!Player.TitleBar)
    //                {
    //                    int SC_MINIMIZE = 0xF020;

    //                    if (m.WParam == (nint)SC_MINIMIZE)
    //                    {
    //                        MaximumSize = Size;
    //                        _maxSizeSet = true;
    //                    }
    //                }
    //            }
    //            break;
    //        case 0x0083: // WM_NCCALCSIZE
    //            if ((int)m.WParam == 1 && !Player.TitleBar && !IsFullscreen)
    //            {
    //                var nccalcsize_params = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);
    //                RECT[] rects = nccalcsize_params.rgrc;
    //                int h = GetTitleBarHeight(Handle, GetDpi(Handle));
    //                rects[0].Top = rects[0].Top - h;
    //                Marshal.StructureToPtr(nccalcsize_params, m.LParam, false);
    //            }
    //            break;
    //        case 0x231: // WM_ENTERSIZEMOVE
    //        case 0x005: // WM_SIZE
    //            if (Player.SnapWindow)
    //                SnapManager.OnSizeAndEnterSizeMove(this);
    //            break;
    //        case 0x214: // WM_SIZING
    //            if (Player.KeepaspectWindow)//解除窗体的尺寸比例限制
    //            {
    //                RECT rc = Marshal.PtrToStructure<RECT>(m.LParam);
    //                RECT r = rc;
    //                SubtractWindowBorders(Handle, ref r, GetDpi(Handle), !Player.TitleBar);

    //                int c_w = r.Right - r.Left, c_h = r.Bottom - r.Top;
    //                Size videoSize = Player.VideoSize;

    //                if (videoSize == Size.Empty)
    //                    videoSize = new Size(16, 9);

    //                double aspect = videoSize.Width / (double)videoSize.Height;
    //                int d_w = (int)Math.Ceiling(c_h * aspect - c_w);
    //                int d_h = (int)Math.Floor(c_w / aspect - c_h);

    //                int[] d_corners = { d_w, d_h, -d_w, -d_h };
    //                int[] corners = { rc.Left, rc.Top, rc.Right, rc.Bottom };
    //                int corner = GetResizeBorder(m.WParam.ToInt32());

    //                if (corner >= 0)
    //                    corners[corner] -= d_corners[corner];

    //                Marshal.StructureToPtr(new RECT(corners[0], corners[1], corners[2], corners[3]), m.LParam, false);
    //                m.Result = new IntPtr(1);
    //            }
    //            return;
    //        case 0x84: // WM_NCHITTEST
    //            // resize borderless window
    //            if ((!Player.Border || !Player.TitleBar) && !Player.Fullscreen)
    //            {
    //                const int HTCLIENT = 1;
    //                const int HTLEFT = 10;
    //                const int HTRIGHT = 11;
    //                const int HTTOP = 12;
    //                const int HTTOPLEFT = 13;
    //                const int HTTOPRIGHT = 14;
    //                const int HTBOTTOM = 15;
    //                const int HTBOTTOMLEFT = 16;
    //                const int HTBOTTOMRIGHT = 17;

    //                int x = (short)(m.LParam.ToInt32() & 0xFFFF); // LoWord
    //                int y = (short)(m.LParam.ToInt32() >> 16);    // HiWord

    //                Point pt = PointToClient(new Point(x, y));
    //                Size cs = ClientSize;
    //                m.Result = new IntPtr(HTCLIENT);
    //                int distance = FontHeight / 3;

    //                if (pt.X >= cs.Width - distance && pt.Y >= cs.Height - distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTBOTTOMRIGHT);
    //                else if (pt.X <= distance && pt.Y >= cs.Height - distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTBOTTOMLEFT);
    //                else if (pt.X <= distance && pt.Y <= distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTTOPLEFT);
    //                else if (pt.X >= cs.Width - distance && pt.Y <= distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTTOPRIGHT);
    //                else if (pt.Y <= distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTTOP);
    //                else if (pt.Y >= cs.Height - distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTBOTTOM);
    //                else if (pt.X <= distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTLEFT);
    //                else if (pt.X >= cs.Width - distance && cs.Height >= distance)
    //                    m.Result = new IntPtr(HTRIGHT);

    //                return;
    //            }
    //            break;
    //        case 0x4A: // WM_COPYDATA
    //            {
    //                var copyData = (CopyDataStruct)m.GetLParam(typeof(CopyDataStruct))!;
    //                string[] args = copyData.lpData.Split('\n');
    //                string mode = args[0];
    //                args = args.Skip(1).ToArray();

    //                switch (mode)
    //                {
    //                    case "single":
    //                        Player.LoadFiles(args, true, false);
    //                        break;
    //                    case "queue":
    //                        foreach (string file in args)
    //                            Player.CommandV("loadfile", file, "append");
    //                        break;
    //                    case "command":
    //                        Player.Command(args[0]);
    //                        break;
    //                }

    //                Activate();
    //            }
    //            return;
    //        case 0x216: // WM_MOVING
    //            if (Player.SnapWindow)
    //                SnapManager.OnMoving(ref m);
    //            break;
    //    }

    //    if (m.Msg == _taskbarButtonCreatedMessage && Player.TaskbarProgress)
    //    {
    //        _taskbar = new Taskbar(Handle);
    //        ProgressTimer.Start();
    //    }

    //    if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_MAXIMIZE)
    //    {
    //        // 拦截最大化命令
    //        DoCustomMaximize();
    //        return; // 不再传给基类，阻止系统默认最大化
    //    }

    //    // beep sound when closed using taskbar due to exception
    //    if (!IsDisposed)
    //        base.WndProc(ref m);
    //}

    //private void DoCustomMaximize()
    //{
    //    // 获取当前屏幕的大小
    //    Rectangle screen = Screen.FromControl(this).Bounds;

    //    // 宽度扩大 2 倍，高度保持屏幕高度
    //    int newWidth = screen.Width * 2;
    //    int newHeight = screen.Height;

    //    // 位置从左上角开始
    //    //this.WindowState = FormWindowState.Normal; // 先确保不是系统最大化
    //    this.Bounds = new Rectangle(screen.X, screen.Y, newWidth, newHeight);

    //    Debug.WriteLine($"DoCustomMaximize");
    //}

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        //if (_maxSizeSet)
        //{
        //    TaskHelp.Run(() =>
        //    {
        //        Thread.Sleep(200);
        //        BeginInvoke(() =>
        //        {
        //            if (!IsDisposed && !Disposing)
        //            {
        //                MaximumSize = new Size(int.MaxValue, int.MaxValue);
        //                _maxSizeSet = false;
        //            }
        //        });
        //    });
        //}
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
            HideCursor();
            Debug.Print($"{DateTime.Now.ToString()}-HideCursor");
        }
    }

    //void ProgressTimer_Tick(object sender, EventArgs e) => UpdateProgressBar();

    //void UpdateProgressBar()
    //{
    //    if (Player.TaskbarProgress && _taskbar != null)
    //        _taskbar.SetValue(Player.GetPropertyDouble("time-pos", false), Player.Duration.TotalSeconds);
    //}

    //void PropChangeWindowScale(double scale)
    //{
    //    if (!WasShown)
    //        return;

    //    BeginInvoke(() =>
    //    {
    //        if (!App.Settings.Enable3DMode && Player.VideoSize.Width != 0 && Player.VideoSize.Height != 0)
    //            SetSize(
    //                (int)(Player.VideoSize.Width * scale),
    //                (int)Math.Floor(Player.VideoSize.Height * scale),
    //                Screen.FromControl(this), false);
    //    });
    //}

    //void PropChangeOnTop(bool value) => BeginInvoke(() => TopMost = value);

    //void PropChangeAid(string value) => Player.AID = value;

    //void PropChangeSid(string value) => Player.SID = value;

    //void PropChangeVid(string value) => Player.VID = value;

    //void PropChangeTitle(string value) { Title = value; SetTitle(); }

    void PropChangeEdition(int value) => Player.Edition = value;

    void PropChangeWindowMaximized()
    {
        if (!WasShown)
            return;


        BeginInvoke(() =>
        {
            Player.WindowMaximized = Player.GetPropertyBool("window-maximized");

            //if (_rectBackupFor3DMode is Rectangle rectangle && Player.WindowMaximized != false)
            //{
            //    Debug.WriteLine($"rectBackupFor3DMode~,WindowState={WindowState},Player.WindowMaximized={Player.WindowMaximized}");
            //    WindowState = FormWindowState.Normal;
            //    Bounds = rectangle;
            //    _rectBackupFor3DMode = null;
            //    return;
            //}

            if (Player.WindowMaximized && WindowState != FormWindowState.Maximized)
            {
                //if (App.Enable3DSubtitle == true)
                //{
                //    Player.WindowMaximized = true;
                //    _rectBackupFor3DMode = Bounds;
                //    DoCustomMaximize();
                //}
                //else
                {
                    Debug.WriteLine($"FormWindowState.Maximized~");
                    WindowState = FormWindowState.Maximized;
                }
            }
            else if (!Player.WindowMaximized && WindowState == FormWindowState.Maximized)
            {
                Debug.WriteLine($"FormWindowState.Normal~");
                WindowState = FormWindowState.Normal;
            }
        });
    }

    void PropChangeWindowMinimized()
    {
        if (!WasShown)
            return;

        BeginInvoke(() =>
        {
            Player.WindowMinimized = Player.GetPropertyBool("window-minimized");

            if (Player.WindowMinimized && WindowState != FormWindowState.Minimized)
                WindowState = FormWindowState.Minimized;
            else if (!Player.WindowMinimized && WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        });
    }

    //void PropChangeCursorAutohide()
    //{
    //    string strValue = Player.GetPropertyString("cursor-autohide");

    //    if (strValue == "no")
    //        _cursorAutohide = 0;
    //    else if (strValue == "always")
    //        _cursorAutohide = -1;
    //    else if (int.TryParse(strValue, out var intValue))
    //        _cursorAutohide = intValue;
    //}

    //void PropChangeBorder(bool enabled)
    //{
    //    Player.Border = enabled;

    //    BeginInvoke(() =>
    //    {
    //        if (!IsFullscreen && !App.Settings.Enable3DMode)
    //        {
    //            if (Player.Border && FormBorderStyle == FormBorderStyle.None)
    //                FormBorderStyle = FormBorderStyle.Sizable;

    //            if (!Player.Border && FormBorderStyle == FormBorderStyle.Sizable)
    //                FormBorderStyle = FormBorderStyle.None;
    //        }
    //    });
    //}

    //void PropChangeTitleBar(bool enabled)
    //{
    //    if (enabled == Player.TitleBar)
    //        return;

    //    Player.TitleBar = enabled;

    //    BeginInvoke(() =>
    //    {
    //        SetSize(ClientSize.Width, ClientSize.Height, Screen.FromControl(this), false);
    //        Height += 1;
    //        Height -= 1;
    //    });
    //}

    //private void PropChangeSubStereoOn(bool obj)
    //{
    //    if (_s3DModeSwitchMenuItem != null)
    //    {
    //        Invoke(() => _s3DModeSwitchMenuItem.IsChecked = obj);
    //    }
    //}

    //void PropChangeFullscreen(bool value) => BeginInvoke(() => CycleFullscreen(value));
    //void Player_ClientMessage(string[] args)
    //{
    //    if (Command.Current.Commands.ContainsKey(args[0]))
    //        Command.Current.Commands[args[0]].Invoke(new ArraySegment<string>(args, 1, args.Length - 1));
    //    else if (GuiCommand.Current.Commands.ContainsKey(args[0]))
    //        BeginInvoke(() => GuiCommand.Current.Commands[args[0]].Invoke(new ArraySegment<string>(args, 1, args.Length - 1)));
    //}

    //void Player_PlaylistPosChanged(int pos)
    //{
    //    if (pos == -1)
    //        SetTitle();
    //}

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
                    float w = Player.GetPropertyInt("width");
                    float h = Player.GetPropertyInt("height");
                    if (w / h <= 2.35) // half-SBS
                    {
                        Player.SetPropertyString("video-aspect-override", (w * 2).ToString() + ":" + h.ToString());
                    }
                    else // full-SBS
                    {
                        Player.SetPropertyString("video-aspect-override", w.ToString() + ":" + h.ToString());
                    }
                }
                //Player.SetPropertyString("video-aspect-override", "7680:2072");
                SbsSubAutoMenuItem_Click(null, null);
                SetTitleInternal();

                int interval = (int)(Player.Duration.TotalMilliseconds / 100);

                if (interval < 100)
                    interval = 100;

                if (interval > 1000)
                    interval = 1000;

                ProgressTimer.Interval = interval;
                
                btnBackRight.Visible = true;
                btn3DSubtitleModeRight.Visible = true;
                btn3DRight.Visible = true;
                progressBarRight.Visible = true;
                progressBarRight.Visible = true;
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

    void Player_Pause()
    {
        if (_taskbar != null && Player.TaskbarProgress)
            _taskbar.SetState(Player.Paused ? TaskbarStates.Paused : TaskbarStates.Normal);
    }

    //void Player_Shutdown() => BeginInvoke(Close);

    //void Player_VideoSizeChanged(Size value) => BeginInvoke(() =>
    //{
    //    if (!KeepSize())
    //        SetFormPosAndSize();
    //});
    //protected override void OnLoad(EventArgs e)
    //{
    //    base.OnLoad(e);
    //    _lastCycleFullscreen = Environment.TickCount;
    //    //SetFormPosAndSize(false, true, true);

    //    if (_3DModeMenuItem == null)
    //    {
    //        _3DModeMenuItem = new MenuItem
    //        {
    //            Header = "3D 立体模式",
    //            IsCheckable = false,
    //        };
    //    }
    //if (_fullScreenUIMemuItem == null)
    //{
    //    _fullScreenUIMemuItem = new WpfControls.MenuItem
    //    {
    //        Header = "Fullscreen UI",
    //        IsCheckable = true,
    //    };
    //    _fullScreenUIMemuItem.Click += FullScreenUI_Click;
    //}

    //if (_s3DModeSwitchMenuItem == null)
    //{
    //    _s3DModeSwitchMenuItem = new MenuItem
    //    {
    //        Header = "开/关",
    //        IsCheckable = true,
    //    };
    //    _s3DModeSwitchMenuItem.Click += Enable3DMode_Click;
    //}

    //if (_SbsSubMemuItem == null)
    //{
    //    _SbsSubMemuItem = new MenuItem
    //    {
    //        Header = "3D字幕",
    //        IsCheckable = false,
    //    };
    //}


    //if (_sSbsSubAutoMenuItem == null)
    //{
    //    _sSbsSubAutoMenuItem = new MenuItem
    //    {
    //        Header = "自动",
    //        IsChecked = true,
    //        IsCheckable = true,
    //    };
    //    _sSbsSubAutoMenuItem.Click += SbsSubAutoMenuItem_Click;
    //}

    //if (_sSbsSubOnMenuItem == null)
    //{
    //    _sSbsSubOnMenuItem = new MenuItem
    //    {
    //        Header = "开-双眼字幕",
    //        IsCheckable = true,
    //    };
    //    _sSbsSubOnMenuItem.Click += SbsSubOnMemuItem_Click;
    //}

    //if (_sSbsSubOffMenuItem == null)
    //{
    //    _sSbsSubOffMenuItem = new MenuItem
    //    {
    //        Header = "关-复制右眼",
    //        IsCheckable = true,
    //    };
    //    _sSbsSubOffMenuItem.Click += SbsSubOffMenuItem_Click;
    //}

    //App.Settings.Enable3DMode = true;
    //_s3DModeSwitchMenuItem.IsChecked = App.Settings.Enable3DMode;
    //BeginInvoke(() =>
    //{
    //    if (App.Settings.Enable3DMode)
    //    {
    //        Enable3DMode_Click(_s3DModeSwitchMenuItem, null);
    //    }
    //});
    //}

    //protected override void OnLostFocus(EventArgs e)
    //{
    //    base.OnLostFocus(e);
    //    ShowCursor();
    //}

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        //if (App.Settings.Enable3DMode)
        //{
        //    Player.SetPropertyString("osc", "no");
        //}
        //else
        //{
        //    Player.SetPropertyString("osc", "yes");
        //}
        //InitializePlayer();
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
                //App.Settings.WindowPosition = Location;
                //App.Settings.WindowLocation = Location;
                //App.Settings.WindowSize = Size;
            }

            //if (FormBorderStyle != FormBorderStyle.None)
            //{
            //    if (WindowState == FormWindowState.Maximized)
            //        _wasMaximized = true;
            //    else if (WindowState == FormWindowState.Normal)
            //        _wasMaximized = false;
            //}

            //if (WasShown)
            //{
            //    if (WindowState == FormWindowState.Minimized)
            //        Player.SetPropertyBool("window-minimized", true);
            //    else if (WindowState == FormWindowState.Normal)
            //    {
            //        Player.SetPropertyBool("window-maximized", false);
            //        Player.SetPropertyBool("window-minimized", false);
            //    }
            //    else if (WindowState == FormWindowState.Maximized)
            //        Player.SetPropertyBool("window-maximized", true);
            //}
            //Debug.WriteLine($"OnResize,ClientSize={ClientSize}");
        }
        //Player.SetPropertyString("osc", "no");
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // prevent annoying beep using alt key
        if (ModifierKeys == Keys.Alt)
            e.SuppressKeyPress = true;

        base.OnKeyDown(e);
    }

    void ShowVideoUI()
    {
        btnBackLeft.Visible = true;
        btn3DSubtitleModeLeft.Visible = true;
        btn3DLeft.Visible = true;        
        progressBarLeft.Visible = true;

        btnBackRight.Visible = true;
        btn3DSubtitleModeRight.Visible = true;
        btn3DRight.Visible = true;
        progressBarRight.Visible = true;

        btnBackLeft.BringToFront();
        btn3DSubtitleModeLeft.BringToFront();
        btn3DLeft.BringToFront();
        progressBarLeft.BringToFront();

        btnBackRight.BringToFront();
        btn3DSubtitleModeRight.BringToFront();
        btn3DRight.BringToFront();        
        progressBarRight.BringToFront();
    }

    void ShowCursor()
    {
        ShowVideoUI();
        overlayPanel.Cursor = Cursors.Default;
        Cursor.Show();
    }

    void HideVideoUI()
    {
        btnBackLeft.Visible = false;
        btn3DSubtitleModeLeft.Visible = false;
        btn3DLeft.Visible = false;
        progressBarLeft.Visible = false;

        btnBackRight.Visible = false;
        btn3DSubtitleModeRight.Visible = false;
        btn3DRight.Visible = false;
        progressBarRight.Visible = false;
        
        
        
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

    bool IsCursorPosDifferent(Point screenPos)
    {
        float len = 5 * (GetDpi(Handle) / 96f);
        return Math.Abs(screenPos.X - MousePosition.X) > len || Math.Abs(screenPos.Y - MousePosition.Y) > len;
    }

    public static int GetDpi(IntPtr hwnd)
    {
        if (Environment.OSVersion.Version >= WindowsTen1607 && hwnd != IntPtr.Zero)
            return GetDpiForWindow(hwnd);
        else
            using (Graphics gx = Graphics.FromHwnd(hwnd))
                return GetDeviceCaps(gx.GetHdc(), 88 /*LOGPIXELSX*/);
    }

    [DllImport("DwmApi")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

    /// <summary>
    /// 选择一个字幕轨道
    /// </summary>
    private void SetDefaultSubId()
    {
        // 读取所有轨道
        //var tracks = Player.GetPropertyString<List<Dictionary<string, object>>>("track-list");
        var tracks = Player.GetPropertyString("track-list");

        //// 例1：按语言优先级匹配（chi/zho/zh）
        //string[] langs = { "chi", "zho", "zh" };
        //int? PickByLang()
        //{
        //    foreach (var lang in langs)
        //    {
        //        var t = tracks.FirstOrDefault(x =>
        //            (string?)x.GetValueOrDefault("type") == "sub" &&
        //            string.Equals((string?)x.GetValueOrDefault("lang"), lang, StringComparison.OrdinalIgnoreCase));
        //        if (t != null) return Convert.ToInt32(t["id"]);
        //    }
        //    return null;
        //}

        //// 例2：按标题关键字兜底
        //string[] keywords = { "中文", "Chinese", "繁体", "简体" };
        //int? PickByTitle()
        //{
        //    return tracks.Where(x => (string?)x.GetValueOrDefault("type") == "sub")
        //                 .FirstOrDefault(x =>
        //                 {
        //                     var title = (string?)x.GetValueOrDefault("title") ?? "";
        //                     return keywords.Any(k => title.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        //                 }) is { } t ? Convert.ToInt32(t["id"]) : (int?)null;
        //}

        //// 选择逻辑：语言→标题→默认第一条字幕
        //int? id = PickByLang() ?? PickByTitle() ??
        //          tracks.Where(x => (string?)x.GetValueOrDefault("type") == "sub")
        //                .Select(x => (int?)Convert.ToInt32(x["id"]))
        //                .FirstOrDefault();

        //if (id.HasValue)
        //{
        //    player.SetPropertyInt("sid", id.Value);
        //    // 可选：显示提示
        //    player.Command("show-text", $"Subtitle: sid={id.Value}", "1500", "3");
        //}
        //else
        //{
        //    // 没有字幕轨道
        //    player.SetPropertyInt("sid", 0); // 关闭
        //}

    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        //SaveWindowProperties();
        App.Settings.Save();
    }

    public static class v3DSubtitleMode
    {
        public static void Auto(System.Windows.Forms.Button Lbutton, System.Windows.Forms.Button Rbutton)
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

        public static void On(System.Windows.Forms.Button Lbutton, System.Windows.Forms.Button Rbutton)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, true);
            Player.SetPropertyBool("sub-stereo-duplicate", false);
            Lbutton.Text = "3D字幕模式:双屏";
            Rbutton.Text = "3D字幕模式:双屏";
        }

        public static void Off(System.Windows.Forms.Button Lbutton, System.Windows.Forms.Button Rbutton)
        {
            Player.SetPropertyBool(CMD_sub_stereo_on, false);
            Lbutton.Text = "3D字幕模式:单屏";
            Rbutton.Text = "3D字幕模式:单屏";
        }
    }

    int isub = 0;
    private void btnSubtitle_Click(object sender, EventArgs e)
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
    }
}
