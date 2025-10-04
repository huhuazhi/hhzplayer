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
            set { subtitleMode = value; SettingsManager.TrySave(); }
        }

        private int lastVideoTrackId = -1;
        [DefaultValue(-1)]
        public int LastVideoTrackId
        {
            get => lastVideoTrackId;
            set { lastVideoTrackId = value; SettingsManager.TrySave(); }
        }

        private int lastAudioTrackId = -1;
        [DefaultValue(-1)]
        public int LastAudioTrackId
        {
            get => lastAudioTrackId;
            set { lastAudioTrackId = value; SettingsManager.TrySave(); }
        }

        private int lastSubtitleTrackId = -1;
        [DefaultValue(-1)]
        public int LastSubtitleTrackId
        {
            get => lastSubtitleTrackId;
            set { lastSubtitleTrackId = value; SettingsManager.TrySave(); }
        }

        private string renderText = "2D渲染器";
        [DefaultValue("2D渲染器")]
        public string RenderText
        {
            get => renderText;
            set { renderText = value; SettingsManager.TrySave(); }
        }

        private string videoAspestW = "0";
        [DefaultValue("0")]
        public string VideoAspestW
        {
            get => videoAspestW;
            set { videoAspestW = value; SettingsManager.TrySave(); }
        }

        private string videoAspestH = "0";
        [DefaultValue("0")]
        public string VideoAspestH
        {
            get => videoAspestH;
            set { videoAspestH = value; SettingsManager.TrySave(); }
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

    public static class SettingsManager
    {
        private static string filePath;
        private static hhzFileSettings currentSettings;
        internal static bool IsLoading { get; private set; }

        private static readonly object saveLock = new();
        private static DateTime lastSaveTime = DateTime.MinValue;

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

        /// <summary>
        /// 为避免多属性连续触发保存，这个函数带有简单防抖逻辑。
        /// </summary>
        internal static void TrySave()
        {
            if (IsLoading) return;
            if ((DateTime.Now - lastSaveTime).TotalMilliseconds < 200) return; // 200ms防抖
            lastSaveTime = DateTime.Now;
            Save();
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
                    // 如果文件存在并是隐藏的，先去掉隐藏属性
                    if (File.Exists(filePath))
                    {
                        var attrs = File.GetAttributes(filePath);
                        if ((attrs & FileAttributes.Hidden) != 0)
                            File.SetAttributes(filePath, attrs & ~FileAttributes.Hidden);
                    }

                    using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    var serializer = new XmlSerializer(typeof(hhzFileSettings));
                    serializer.Serialize(stream, currentSettings);

                    // 写完再设回隐藏属性
                    File.SetAttributes(filePath, FileAttributes.Hidden);
                }
                catch (Exception ex)
                {
                    DebugLog($"保存设置失败: {ex.Message}");
                }
            }
        }

        public static void ResetToDefaults() => currentSettings = new hhzFileSettings();

        private static void DebugLog(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }
    }
}
