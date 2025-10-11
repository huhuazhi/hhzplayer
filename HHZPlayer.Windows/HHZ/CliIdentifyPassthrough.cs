using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HHZPlayer.Windows
{
    /// <summary>
    /// 在检测到命令行需要“终端输出”时（--identify / --term- / --msg-level ...）
    /// 1) 临时挂载/创建控制台；
    /// 2) 把所有参数原样透传给 mpvnet.exe/mpv.exe；
    /// 3) 将退出码返回给外部。
    /// 双击正常启动时完全不影响 GUI。
    /// </summary>
    internal static class CliIdentifyPassthrough
    {
        // ============ Console attach ============
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")] private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")] private static extern bool AllocConsole();
        [DllImport("kernel32.dll")] private static extern bool FreeConsole();

        public static void EnsureConsole()
        {
            try
            {
                if (!AttachConsole(ATTACH_PARENT_PROCESS))
                    AllocConsole();
                try { Console.OutputEncoding = Encoding.UTF8; } catch { /* ignore */ }
                try { Console.InputEncoding = Encoding.UTF8; } catch { /* ignore */ }
            }
            catch { /* ignore */ }
        }

        /// <summary>是否请求了“在终端输出”的模式</summary>
        public static bool IsCliOutputRequested(string[] args)
        {
            if (args == null || args.Length == 0) return false;

            // 典型探测参数：--identify / --term-playing-msg / --term-status-msg / --msg-level / --help
            return args.Any(a =>
                   a.Equals("--identify", StringComparison.OrdinalIgnoreCase)
                || a.StartsWith("--term-", StringComparison.OrdinalIgnoreCase)
                || a.StartsWith("--msg-level", StringComparison.OrdinalIgnoreCase)
                || a.Equals("-h", StringComparison.OrdinalIgnoreCase)
                || a.Equals("--help", StringComparison.OrdinalIgnoreCase)
                || a.Equals("--audio-device=help", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 透传到 mpvnet.exe / mpv.exe（优先 mpvnet），在同目录或 Tools 下查找。
        /// 支持用环境变量 MPV_BIN 指定二进制路径。
        /// </summary>
        public static int RunMpvPassthrough(string[] args)
        {
            string baseDir = AppContext.BaseDirectory;

            // 在程序目录常见位置查找
            string[] candidates =
            {
                Path.Combine(baseDir, "hhzplayer.exe"),
            };
            string? exe = candidates.FirstOrDefault(File.Exists);

            if (exe == null)
                throw new FileNotFoundException("未找到 hhzplayer.exe，请将其放到程序目录或设置环境变量 MPV_BIN。");

            return Exec(exe, args);
        }

        private static int Exec(string exe, string[] args)
        {
            // 原样透传参数（遇空格加引号）
            static string Q(string s) => s.Contains(' ') ? $"\"{s}\"" : s;
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = string.Join(" ", args.Select(Q)),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            };
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("无法启动 hhzplayer 进程");
            p.WaitForExit();
            return p.ExitCode;
        }
    }
}
