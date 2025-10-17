using HHZPlayer.SingleInstance; // ★ 新增：单实例/管道
using HHZPlayer.Windows.HHZ;
using HHZPlayer.Windows.UI;
using HHZPlayer.Windows.WinForms;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Console_Form
{
    internal class Program
    {
        private static Thread? _uiThread;

        // ★ 新增：退出事件与控制台标记
        private static readonly ManualResetEventSlim QuitEvent = new(false);
        private static bool _consoleOn = false;

        [STAThread]
        static int Main(string[] args)
        {
            // 只有“有参数”时才附着控制台；无参启动不显示 Console
            bool hasArgs = args != null && args.Length > 0;
            _consoleOn = ConsoleHelper.EnsureConsole(needConsole: hasArgs, createNewIfNoParent: false);

            if (hasArgs) CommandlineHelper.ProcessCommandline(args);

            if (_consoleOn)
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = UTF8Encoding.UTF8;
                
                // === 关键：OnlyConsole 直接走“无界面路径”，完全不创建 MainForm ===
                if (CommandlineHelper.OnlyConsole)
                    return RunHeadlessConsoleFlow(_consoleOn);

                Console.WriteLine("MyConsoleTimer 已启动。按 Ctrl+C 退出。");
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    if (_consoleOn) Console.WriteLine("\n收到退出信号，正在请求关闭 UI…");
                    try { Application.Exit(); } catch { }
                };
            }
            else if(CommandlineHelper.OnlyConsole)
            {
                return 0;
            }

            // 固定工作目录/环境变量（保留你的逻辑）
            string baseDir = AppDomain.CurrentDomain.BaseDirectory; 
            Directory.SetCurrentDirectory(baseDir);// 强制设置工作目录

            Environment.SetEnvironmentVariable("PYTHONHOME", baseDir, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH",
                Path.Combine(baseDir, "Lib") + ";" + Path.Combine(baseDir, "Lib", "site-packages"),
                EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH",
                baseDir + ";" + Environment.GetEnvironmentVariable("PATH"),
                EnvironmentVariableTarget.Process);

            // 初始化 App/Theme（用于读取 App.Settings.IsSingleProcess）
            App.Init();
            Theme.Init();

            bool singleProcessMode = true;
            try { singleProcessMode = App.Settings.IsSingleProcess; } catch { singleProcessMode = true; }

            // 单进程模式：非主实例把参数转发给已运行实例并退出
            if (singleProcessMode)
            {
                if (!SingleInstanceIpc.TryBecomePrimary("v1", args))
                    return 0;
            }

            //启动 WinForms UI 在线程中（单进程内或多进程独立）
            _uiThread = new Thread(() =>
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    MainForm mainForm = new MainForm();

                    // 如果启用单进程模式，则启动管道监听并处理初始参数；否则直接用自身参数加载
                    if (singleProcessMode)
                    {
                        // ★ 管道监听：别的实例启动时把新文件推过来
                        SingleInstanceIpc.StartServer(files =>
                        {
                            try
                            {
                                mainForm.BeginInvoke(new Action(() =>
                                {
                                    WindowActivator.BringToFront(mainForm);
                                    mainForm.OpenFromIpc(files); // ← 在 MainForm 里实现
                                }));
                            }
                            catch { /* ignore */ }
                        });

                        // ★ 首次启动也处理一次自身的启动参数
                        SingleInstanceIpc.DeliverInitialArgs(files =>
                        {
                            try
                            {
                                mainForm.BeginInvoke(new Action(() =>
                                {
                                    WindowActivator.BringToFront(mainForm);
                                    mainForm.OpenFromIpc(files);
                                }));
                            }
                            catch { /* ignore */ }
                        });
                    }
                    else
                    {
                        // 多进程模式：直接把命令行参数交给 MainForm 处理
                        try
                        {
                            var initialFiles = CommandlineHelper.files;
                            if (initialFiles is { Count: > 0 })
                            {
                                mainForm.BeginInvoke(new Action(() =>
                                {
                                    WindowActivator.BringToFront(mainForm);
                                    mainForm.OpenFromIpc([.. initialFiles]);
                                }));
                            }
                        }
                        catch { /* ignore */ }
                    }

                    // 窗口关闭 → 退出
                    mainForm.FormClosed += (s, e) =>
                    {
                        try { Application.ExitThread(); } catch { }
                        QuitEvent.Set();
                    };

                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    if (_consoleOn) Console.Error.WriteLine($"[UIThread] {ex}");
                    QuitEvent.Set();
                }
                finally
                {
                    QuitEvent.Set();
                }
            });
            _uiThread.SetApartmentState(ApartmentState.STA);
            //_uiThread.IsBackground = true;
            _uiThread.Start();

            // 阻塞等待 UI 线程退出
            if (_uiThread != null) _uiThread.Join();

            //QuitEvent.Wait();

            // ★ 清理单实例资源
            SingleInstanceIpc.Shutdown();

            if (_consoleOn) Console.WriteLine("已退出。");
            return 0;
        }

        // 你原来的定时器回调（如需可继续用）
        private static void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (_consoleOn)
                    Console.WriteLine($"TimerCallback: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
            catch (Exception ex)
            {
                if (_consoleOn) Console.Error.WriteLine($"[TimerError] {ex.Message}");
            }
        }
        private static int RunHeadlessConsoleFlow(bool consoleOn)
        {
            App.Init();
            Theme.Init();

            // 2) 创建“消息专用窗口”句柄，避免 UI 控件
            using var msgWin = new MessageOnlyWindow();   // 见下方类

            // 3) 初始化 Player（只取信息不渲染时可设 vo=null/ao=null）
            Player.Init(msgWin.Handle, false, CommandlineHelper.originalArgs.ToArray());
            Player.Command("set pause yes");
            try { Player.SetPropertyString("vo", "null"); } catch { }   // 可选：仅信息，不渲染
            try { Player.SetPropertyString("ao", "null"); } catch { }   // 可选：不初始化音频

            // 4) 订阅 FileLoaded：输出 → 退出消息循环
            var ctx = new ApplicationContext();
            Player.FileLoaded += () =>
            {
                try
                {
                    int vw = Player.GetPropertyInt("width");
                    int vh = Player.GetPropertyInt("height");
                    double fps = Player.GetPropertyDouble("container-fps");
                    string tracks = Player.GetPropertyString("track-list");
                    string path = Player.Path;

                    if (consoleOn)
                    {
                        Console.WriteLine(
                            $"文件路径:{path}\r\n" +
                            $"分辨率:{vw}x{vh}-{fps} fps {MainForm.GetHdrType(Player)}\r\n" +
                            $"时长:{Player.Duration}\r\n" +
                            $"{tracks}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    if (consoleOn) Console.Error.WriteLine($"[Headless] {ex}");
                }
                finally
                {
                    // 退出消息循环 → 进程结束
                    ctx.ExitThread();
                }
            };

            // 5) 开始加载文件（不触发任何 UI）
            var files = CommandlineHelper.files;
            if (files is { Count: > 0 })
                Player.LoadFiles([.. files], /*addToPlaylist*/ true, /*append*/ false);

            // 6) 跑一个“无窗”的消息循环，让 mpv/Player 正常派发事件
            Application.Run(ctx);
            return 0;
        }

        sealed class MessageOnlyWindow : NativeWindow, IDisposable
        {
            // HWND_MESSAGE = (IntPtr)(-3) → 消息专用窗口，永远不可见
            private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

            public MessageOnlyWindow()
            {
                var cp = new CreateParams
                {
                    Caption = string.Empty,
                    ClassName = null,
                    X = 0,
                    Y = 0,
                    Height = 0,
                    Width = 0,
                    Parent = HWND_MESSAGE
                };
                CreateHandle(cp);
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                    DestroyHandle();
            }
        }
    }
}
