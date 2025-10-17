using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Drawing;
using static HHZPlayer.AppSettings;

namespace HHZPlayer;

[Serializable()]
public class AppSettings
{
    public bool InputDefaultBindingsFixApplied;
    public bool ShowMenuFixApplied;
    public int MenuUpdateVersion;
    public int Volume = 70;
    public List<string> RecentFiles = new List<string>();
    public Point WindowLocation;
    public Point WindowPosition;
    public Size WindowSize;
    public string AudioDevice = "";
    public string ConfigEditorSearch = "Video:";
    public string Mute = "no";
    public string StartupFolder = "";
    public bool Enable3DMode;
    public string LastOpenedFolder = "";
    public bool FromLastPosPlay = false;

    // 控制是否以单进程模式运行：
    // true = 单进程（新启动的实例会把文件通过 IPC 发送到已有实例并退出）
    // false = 多进程（每个实例独立打开文件）
    public bool IsSingleProcess { get; set; } = false;

    //public System.Windows.Forms.FormWindowState WindowStatus { get; set; }
    public enum enumWindowsStatus
    {
        Normal,
        Minimized,
        Maximized,
        Other
    }

    public enumWindowsStatus WindowsStatus;

    public enum enumFormBorderStyle
    {
        None,
        Sizable,
        other
    }

    public enumSubtitleMode SubtitleMode;
    public enum enumSubtitleMode
    {
        Auto,
        On,
        Off,
        other
    }


    public enumFormBorderStyle FormBorderStyle;
    public string RenderText;

    public void Save()
    {
        SettingsManager.Save(this);
    }
}

class SettingsManager
{
    public static string SettingsFile => Player.ConfigFolder + "settings.xml";

    public static AppSettings Load()
    {
        if (!File.Exists(SettingsFile))
            return new AppSettings();

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using FileStream fs = new FileStream(SettingsFile, FileMode.Open);
            return (AppSettings)serializer.Deserialize(fs)!;
        }
        catch (Exception ex)
        {
            Terminal.WriteError(ex.ToString());
            return new AppSettings();
        }
    }

    public static void Save(object obj)
    {
        try
        {
            using XmlTextWriter writer = new XmlTextWriter(SettingsFile, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(writer, obj);
        }
        catch (Exception ex)
        {
            Terminal.WriteError(ex.ToString());
        }
    }
}
