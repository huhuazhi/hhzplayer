using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace MyApp
{
    [XmlRoot("AppSettings")]
    public class hhzFileSettings
    {
        private string subtitleMode = "2D字幕(自动3D)";
        [DefaultValue("2D字幕(自动3D)")]
        public string SubtitleMode
        {
            get => subtitleMode;
            set { subtitleMode = value; hhzSettingsManager.MarkDirty(); }
        }

        private int lastVideoTrackId = -1;
        [DefaultValue(-1)]
        public int LastVideoTrackId
        {
            get => lastVideoTrackId;
            set { lastVideoTrackId = value; hhzSettingsManager.MarkDirty(); }
        }

        private int lastAudioTrackId = -1;
        [DefaultValue(-1)]
        public int LastAudioTrackId
        {
            get => lastAudioTrackId;
            set { lastAudioTrackId = value; hhzSettingsManager.MarkDirty(); }
        }

        private int lastSubtitleTrackId = -1;
        [DefaultValue(-1)]
        public int LastSubtitleTrackId
        {
            get => lastSubtitleTrackId;
            set { lastSubtitleTrackId = value; hhzSettingsManager.MarkDirty(); }
        }

        private string renderText = "2D渲染器";
        [DefaultValue("2D渲染器")]
        public string RenderText
        {
            get => renderText;
            set { renderText = value; hhzSettingsManager.MarkDirty(); }
        }

        private string videoAspestW = "0";
        [DefaultValue("0")]
        public string VideoAspestW
        {
            get => videoAspestW;
            set { videoAspestW = value; hhzSettingsManager.MarkDirty(); }
        }

        private string videoAspestH = "0";
        [DefaultValue("0")]
        public string VideoAspestH
        {
            get => videoAspestH;
            set { videoAspestH = value; hhzSettingsManager.MarkDirty(); }
        }

        [XmlIgnore]
        public bool IsAllDefault =>
            SubtitleMode == "2D字幕(自动3D)" &&
            LastVideoTrackId == -1 &&
            LastAudioTrackId == -1 &&
            LastSubtitleTrackId == -1 &&
            RenderText == "2D渲染器" &&
            VideoAspestW == "0" &&
            VideoAspestH == "0";

        [XmlIgnore]
        public bool IsModify => !IsAllDefault;
    }

    public static class hhzSettingsManager
    {
        private static string filePath;
        private static hhzFileSettings currentSettings;
        internal static bool IsLoading { get; private set; }

        private static readonly object saveLock = new();
        private static bool IsDirty = false;
        private static DateTime lastSaveTime = DateTime.MinValue;

        private static Thread watcherThread;
        private static bool running = true;

        static hhzSettingsManager()
        {
            // 启动后台轮询线程
            watcherThread = new Thread(WatcherLoop)
            {
                IsBackground = true,
                Name = "hhzSettingsWatcher"
            };
            watcherThread.Start();
        }

        public static hhzFileSettings Current => currentSettings ??= new hhzFileSettings();

        public static hhzFileSettings Load(string FilePath = null)
        {
            filePath = FilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                currentSettings = new hhzFileSettings();
                return currentSettings;
            }

            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                currentSettings = new hhzFileSettings();
                return currentSettings;
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var serializer = new XmlSerializer(typeof(hhzFileSettings));

            IsLoading = true;
            try
            {
                currentSettings = (hhzFileSettings)serializer.Deserialize(stream);
            }
            catch
            {
                currentSettings = new hhzFileSettings();
            }
            finally
            {
                IsLoading = false;
            }

            return currentSettings;
        }

        public static void MarkDirty()
        {
            if (IsLoading) return;
            IsDirty = true;
        }

        private static void WatcherLoop()
        {
            while (running)
            {
                try
                {
                    Thread.Sleep(500); // 每500ms轮询一次

                    if (!IsDirty) continue;
                    if ((DateTime.Now - lastSaveTime).TotalMilliseconds < 800) continue;

                    Save();
                    IsDirty = false;
                    lastSaveTime = DateTime.Now;
                }
                catch
                {
                    // 防止线程崩溃
                }
            }
        }

        public static void Save()
        {
            if (IsLoading) return;
            currentSettings ??= new hhzFileSettings();
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            lock (saveLock)
            {
                if (currentSettings.IsAllDefault)
                {
                    if (File.Exists(filePath))
                    {
                        try { File.Delete(filePath); } catch { }
                    }
                    return;
                }

                var dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                try
                {
                    if (File.Exists(filePath))
                    {
                        var attrs = File.GetAttributes(filePath);
                        if ((attrs & FileAttributes.Hidden) != 0)
                            File.SetAttributes(filePath, attrs & ~FileAttributes.Hidden);
                    }

                    using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    var serializer = new XmlSerializer(typeof(hhzFileSettings));
                    serializer.Serialize(stream, currentSettings);

                    File.SetAttributes(filePath, FileAttributes.Hidden);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"保存设置失败: {ex.Message}");
#endif
                }
            }
        }

        public static void StopWatcher()
        {
            running = false;
        }
    }
}
