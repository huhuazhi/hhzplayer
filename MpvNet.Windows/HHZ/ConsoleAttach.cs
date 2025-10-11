using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HHZPlayer.Windows // 用你工程的命名空间
{
    internal static class ConsoleAttach
    {
        const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")] static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")] static extern bool AllocConsole();
        [DllImport("kernel32.dll")] static extern bool FreeConsole();

        public static void EnsureConsole()
        {
            try
            {
                if (!AttachConsole(ATTACH_PARENT_PROCESS))
                    AllocConsole();
                try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            }
            catch { /* 忽略 */ }
        }

        public static bool IsCliOutputRequested(string[] args)
        {
            if (args == null || args.Length == 0) return false;
            return args.Any(a =>
                   string.Equals(a, "--identify", StringComparison.OrdinalIgnoreCase)
                || a.StartsWith("--term-", StringComparison.OrdinalIgnoreCase)      // --term-playing-msg / --term-status-msg
                || a.StartsWith("--msg-level", StringComparison.OrdinalIgnoreCase) // 想看日志
                || a.Equals("-h") || a.Equals("--help")
                || a.Equals("--audio-device=help", StringComparison.OrdinalIgnoreCase));
        }

        public static int RunMpvPassthrough(string[] args)
        {
            // 在程序目录找 mpvnet.exe / mpv.exe
            string baseDir = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "hhzplayer.exe"),
            };
            string exe = candidates.FirstOrDefault(File.Exists)
                      ?? throw new FileNotFoundException("未找到 hhzplayer.exe ，请把它放在程序目录。");

            string Quote(string a) => a.Contains(' ') ? $"\"{a}\"" : a;
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = string.Join(" ", args.Select(Quote)),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            };
            using var p = Process.Start(psi)!;
            p.WaitForExit();
            return p.ExitCode;
        }
    }
}
