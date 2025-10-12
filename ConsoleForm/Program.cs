using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Timers;
using System.Threading;

namespace Console_Form
{
    internal class Program
    {
        // 用 AutoResetEvent 来优雅阻塞主线程，支持 Ctrl+C 退出
        private static readonly System.Threading.AutoResetEvent QuitEvent = new(initialState: false);
        private static System.Timers.Timer _timer;
        private static CancellationTokenSource? _cts;

        static int Main(string[] args)
        {
            // 防中文乱码
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("MyConsoleTimer 已启动。按 Ctrl+C 退出。");

            // 监听 Ctrl+C
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // 不让进程立刻被终止，先做清理
                Console.WriteLine("\n收到退出信号，正在停止计时器…");
                StopTimer();
                QuitEvent.Set();
            };

            // 启动定时器：每秒回显一次
            _timer = new System.Timers.Timer(1000);
            _timer.AutoReset = true;                 // 周期触发
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            // 2) 启动命名管道服务器（后台线程）
            _cts = new CancellationTokenSource();
            var pipeThread = new Thread(() => RunPipeServer("hhz.console.echo", _cts.Token))
            {
                IsBackground = true
            };
            pipeThread.Start();


            // 阻塞等待退出
            QuitEvent.WaitOne();

            Console.WriteLine("已退出。");
            return 0;
        }

        private static void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine($"TimerCallback: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                // 如需即时刷新：Console.Out.Flush();
            }
            catch (Exception ex)
            {
                // 把异常写到 stderr，不影响 stdout 的管道
                Console.Error.WriteLine($"[TimerError] {ex.Message}");
            }
        }

        private static void StopTimer()
        {
            if (_timer is null) return;
            try
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
            }
            catch { /* 忽略清理异常 */ }
        }

        /// <summary>
        /// 命名管道服务器：每次处理一个客户端连接；逐行读取，原样写到控制台。
        /// 协议：UTF-8，每行一条消息；发送端以 \n 结尾。
        /// </summary>
        private static void RunPipeServer(string pipeName, CancellationToken token)
        {
            Console.WriteLine($"[Pipe] 正在监听: \\\\.\\pipe\\{pipeName}");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        pipeName,
                        PipeDirection.In,
                        maxNumberOfServerInstances: NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    // 等待客户端连接
                    var waitHandle = server.BeginWaitForConnection(null, null).AsyncWaitHandle;
                    int signaled = WaitHandle.WaitAny(new[] { waitHandle, token.WaitHandle });
                    if (signaled == 1) return; // 取消

                    server.EndWaitForConnection(null);

                    using var sr = new StreamReader(server, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
                    string? line;
                    while (!token.IsCancellationRequested && (line = sr.ReadLine()) is not null)
                    {
                        // 简单的行协议，可按需扩展（例如前缀 [INFO]/[WARN]）
                        Console.WriteLine(line);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[PipeError] {ex.Message}");
                    // 小憩避免疯狂重试
                    Thread.Sleep(300);
                }
            }
        }
    }
}
