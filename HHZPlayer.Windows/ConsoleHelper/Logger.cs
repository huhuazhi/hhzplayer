using System;
using System.Text;

public static class Logger
{
    static Logger()
    {
        // 防止中文乱码（控制台在 PowerShell 里）
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
    }

    public static void Info(string msg)
    {
        Console.WriteLine(msg);
    }

    public static void Error(string msg)
    {
        Console.Error.WriteLine(msg);
    }
}