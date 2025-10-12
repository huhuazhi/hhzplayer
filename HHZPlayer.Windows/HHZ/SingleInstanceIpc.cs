using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HHZPlayer.SingleInstance
{
    public static class SingleInstanceIpc
    {
        private static Mutex? _mutex;
        private static string? _pipeName;
        private static string[]? _initialArgs;
        private static CancellationTokenSource? _cts;

        public static bool TryBecomePrimary(string appId, string[]? args)
        {
            var sid = WindowsIdentity.GetCurrent()?.User?.Value ?? Environment.UserName;
            _pipeName = $"HHZPlayer.Open.{appId}.{sid}"; // 每用户隔离
            bool createdNew;
            _mutex = new Mutex(true, $@"Local\{_pipeName}.mutex", out createdNew);

            if (createdNew)
            {
                _initialArgs = args;
                return true; // 我是主实例
            }
            else
            {
                // 把参数转发给主实例再退出
                if (args is { Length: > 0 })
                {
                    try { SendToPrimaryAsync(args).GetAwaiter().GetResult(); }
                    catch { /* 忽略异常，反正当前实例要退出 */ }
                }
                return false;
            }
        }

        public static void StartServer(Action<string[]> onFiles, CancellationToken externalToken = default)
        {
            if (_pipeName is null) throw new InvalidOperationException("Call TryBecomePrimary first.");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

            Task.Run(async () =>
            {
                var token = _cts!.Token;
                while (!token.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(
                        _pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    try
                    {
                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);
                        using var reader = new StreamReader(server, new UTF8Encoding(false));
                        var line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var files = JsonSerializer.Deserialize<string[]>(line!) ?? Array.Empty<string>();
                            onFiles(files);
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch { /* 忽略继续下一轮 */ }
                }
            }, _cts.Token);
        }

        public static void DeliverInitialArgs(Action<string[]> onFiles)
        {
            if (_initialArgs is { Length: > 0 })
            {
                onFiles(_initialArgs);
                _initialArgs = null;
            }
        }

        public static void Shutdown()
        {
            try { _cts?.Cancel(); } catch { }
            try { _mutex?.ReleaseMutex(); } catch { }
        }

        private static async Task<bool> SendToPrimaryAsync(string[] files)
        {
            if (_pipeName is null) return false;
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
                await client.ConnectAsync(1500).ConfigureAwait(false);
                using var writer = new StreamWriter(client, new UTF8Encoding(false)) { AutoFlush = true };
                var json = JsonSerializer.Serialize(files);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
                return true;
            }
            catch { return false; }
        }
    }
}
