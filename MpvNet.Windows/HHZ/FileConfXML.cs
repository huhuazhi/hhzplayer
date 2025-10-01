using System;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel; // << 新增

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
            set { subtitleMode = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private int lastVideoTrackId = -1;
        [DefaultValue(-1)]
        public int LastVideoTrackId
        {
            get => lastVideoTrackId;
            set { lastVideoTrackId = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private int lastAudioTrackId = -1;
        [DefaultValue(-1)]
        public int LastAudioTrackId
        {
            get => lastAudioTrackId;
            set { lastAudioTrackId = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private int lastSubtitleTrackId = -1;
        [DefaultValue(-1)]
        public int LastSubtitleTrackId
        {
            get => lastSubtitleTrackId;
            set { lastSubtitleTrackId = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private string renderText = "2D渲染器";
        [DefaultValue("2D渲染器")]
        public string RenderText
        {
            get => renderText;
            set { renderText = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private string videoAspestW = "0";
        [DefaultValue("0")]
        public string VideoAspestW
        {
            get => videoAspestW;
            set { videoAspestW = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
        }

        private string videoAspestH = "0";
        [DefaultValue("0")]
        public string VideoAspestH
        {
            get => videoAspestH;
            set { videoAspestH = value; if (!SettingsManager.IsLoading) SettingsManager.Save(); }
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
        public bool IsModify =>
            !IsAllDefault; // 也可以按你原来的口径只比对部分字段
    }

    public static class SettingsManager
    {
        private static string filePath = "settings.xml";
        private static hhzFileSettings currentSettings;
        internal static bool IsLoading { get; private set; }

        public static hhzFileSettings Current => currentSettings ??= new hhzFileSettings();

        public static hhzFileSettings Load(string FilePath = null)
        {
            if (!string.IsNullOrEmpty(FilePath)) filePath = FilePath;
            if (!File.Exists(filePath))
            {
                currentSettings = new hhzFileSettings();
                return null; // 按你原意：没有文件时返回 null
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var serializer = new XmlSerializer(typeof(hhzFileSettings));

            IsLoading = true;
            try { currentSettings = (hhzFileSettings)serializer.Deserialize(stream); }
            finally { IsLoading = false; }

            return currentSettings;
        }

        public static void Save(string FilePath = null)
        {
            if (!string.IsNullOrEmpty(FilePath)) filePath = FilePath;
            if (string.IsNullOrEmpty(filePath)) throw new InvalidOperationException("保存失败：文件路径为空。");

            currentSettings ??= new hhzFileSettings();

            // ✅ 如果全部是默认值：删除配置文件并返回
            if (currentSettings.IsAllDefault)
            {
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { /* 忽略删除失败 */ }
                }
                return;
            }

            var dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                var serializer = new XmlSerializer(typeof(hhzFileSettings));
                serializer.Serialize(stream, currentSettings);
            }
            catch { /* 忽略写入异常，保留你的风格 */ }
        }

        public static void ResetToDefaults() => currentSettings = new hhzFileSettings();
    }
}
