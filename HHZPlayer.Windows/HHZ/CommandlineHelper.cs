using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHZPlayer.Windows.HHZ
{    
    public static class CommandlineHelper
    {
        public static bool GetVideoInfo;
        public static bool InfoVerbose;
        public static List<string> files = [];
        public static List<string> originalArgs = [];
        public static bool OnlyConsole;
        public static bool ProcessCommandline(string[] Args)
        {
            GetVideoInfo = false;
            InfoVerbose = false;
            files = [];
            originalArgs = [];
            OnlyConsole = false;

            bool argCorrect = false;

            foreach (string arg in Args)
            {
                if (arg.StartsWith("--") || ((arg.StartsWith("-") || arg.StartsWith("/")) || arg == "-" || arg.Contains("://") ||
                    arg.Contains(":\\") || arg.StartsWith("\\\\") || arg.StartsWith('.') ||
                File.Exists(arg)))
                {
                    if (arg.ToLower() == "--get-video-info") { GetVideoInfo = true; OnlyConsole = true; argCorrect = true; continue; }
                    // 这是一个例子 if (arg.ToLower() == "-v") { InfoVerbose = true; argCorrect = true; continue; }

                    if (File.Exists(arg)) { files.Add(arg); argCorrect = true; }
                    originalArgs.Add(arg);
                }
            }

            if (argCorrect)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
