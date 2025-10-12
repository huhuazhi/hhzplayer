using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal static class ConsoleHelper
{
    private const int ATTACH_PARENT_PROCESS = -1;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(
        string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition,
        uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

    private const uint GENERIC_READ = 0x80000000;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint FILE_SHARE_READ = 0x1;
    private const uint FILE_SHARE_WRITE = 0x2;
    private const uint OPEN_EXISTING = 3;

    private const int STD_INPUT_HANDLE = -10;
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;

    public static bool EnsureConsole(bool needConsole, bool createNewIfNoParent = false)
    {
        if (!needConsole) return false;

        bool attached = AttachConsole(ATTACH_PARENT_PROCESS);
        if (!attached && createNewIfNoParent)
            attached = AllocConsole();
        if (!attached) return false;

        // 重新绑定标准输出/错误
        var outHandle = CreateFile("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE,
                                   IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        if (outHandle != IntPtr.Zero && outHandle.ToInt64() != -1)
        {
            SetStdHandle(STD_OUTPUT_HANDLE, outHandle);
            SetStdHandle(STD_ERROR_HANDLE, outHandle);
            var fsOut = new FileStream(new SafeFileHandle(outHandle, ownsHandle: false), FileAccess.Write);
            var writer = new StreamWriter(fsOut, new UTF8Encoding(false)) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
        }

        // ★ 关键修复：CONIN$ 需要读 + 写，并允许共享写
        var inHandle = CreateFile("CONIN$", GENERIC_READ | GENERIC_WRITE,
                                  FILE_SHARE_READ | FILE_SHARE_WRITE,
                                  IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        if (inHandle != IntPtr.Zero && inHandle.ToInt64() != -1)
        {
            SetStdHandle(STD_INPUT_HANDLE, inHandle);
            var fsIn = new FileStream(new SafeFileHandle(inHandle, ownsHandle: false), FileAccess.Read);
            var reader = new StreamReader(fsIn, new UTF8Encoding(false));
            Console.SetIn(reader);
        }

        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        return true;
    }
}
