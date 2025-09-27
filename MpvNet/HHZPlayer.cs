
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using MpvNet.ExtensionMethod;
using MpvNet.Help;
using MpvNet.Native;

using static MpvNet.Native.LibMpv;

namespace MpvNet;
  
public class HHZPlayer : MpvClient
{
    public nint MainHandle { get; set; }

    public void MainEventLoop()
    {
        while (true)
        {
            mpv_wait_event(MainHandle, -1);
        }
    }

    string? _configFolder;
    public string ConfigFolder
    {
        get
        {
            if (_configFolder == null)
            {
                string? mpvnet_home = Environment.GetEnvironmentVariable("MPVNET_HOME");

                if (Directory.Exists(mpvnet_home))
                    return _configFolder = mpvnet_home.AddSep();

                _configFolder = Folder.Startup + "portable_config";

                if (!Directory.Exists(_configFolder))
                    _configFolder = Folder.AppData + "mpv.net";

                if (!Directory.Exists(_configFolder))
                    Directory.CreateDirectory(_configFolder);

                _configFolder = _configFolder.AddSep();
            }

            return _configFolder;
        }
    }
    public string ConfPath { get => ConfigFolder + "mpv.conf"; }
    private readonly Regex ConfRegex = new Regex("^[\\w-]+$", RegexOptions.Compiled);

    Dictionary<string, string>? _Conf;
    public float Autofit { get; set; } = 0.6f;
    public float AutofitSmaller { get; set; } = 0.3f;
    public float AutofitLarger { get; set; } = 0.8f;
    public bool Border { get; set; } = true;
    public bool Fullscreen { get; set; }
    public string GPUAPI { get; set; } = "auto";
    public bool KeepaspectWindow { get; set; }
    public int Screen { get; set; } = -1;
    public bool SnapWindow { get; set; }
    public bool TaskbarProgress { get; set; } = true;
    public string VO { get; set; } = "gpu";
    public bool WindowMaximized { get; set; }
    public bool WindowMinimized { get; set; }
    public bool TitleBar { get; set; } = true;

    public void ProcessProperty(string? name, string? value)
    {
        switch (name)
        {
            case "autofit":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        Autofit = result / 100f;
                }
                break;
            case "autofit-smaller":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        AutofitSmaller = result / 100f;
                }
                break;
            case "autofit-larger":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        AutofitLarger = result / 100f;
                }
                break;
            case "border": Border = value == "yes"; break;
            case "fs":
            case "fullscreen": Fullscreen = value == "yes"; break;
            case "gpu-api": GPUAPI = value!; break;
            case "keepaspect-window": KeepaspectWindow = value == "yes"; break;
            case "screen": Screen = Convert.ToInt32(value); break;
            case "snap-window": SnapWindow = value == "yes"; break;
            case "taskbar-progress": TaskbarProgress = value == "yes"; break;
            case "vo": VO = value!; break;
            case "window-maximized": WindowMaximized = value == "yes"; break;
            case "window-minimized": WindowMinimized = value == "yes"; break;
            case "title-bar": TitleBar = value == "yes"; break;
        }

        if (AutofitLarger > 1)
            AutofitLarger = 1;
    }
    public Dictionary<string, string> Conf
    {
        get
        {
            if (_Conf != null)
                return _Conf;

            App.ApplyInputDefaultBindingsFix();

            _Conf = [];

            if (File.Exists(ConfPath))
            {
                foreach (string? it in File.ReadAllLines(ConfPath))
                {
                    string line = it.TrimStart(' ', '-').TrimEnd();

                    if (line.StartsWith('#'))
                        continue;

                    if (!line.Contains('='))
                    {
                        if (ConfRegex.Match(line).Success)
                            line += "=yes";
                        else
                            continue;
                    }

                    string key = line[..line.IndexOf("=")].Trim();
                    string value = line[(line.IndexOf("=") + 1)..].Trim();

                    if (value.Contains('#') && !value.StartsWith("#") &&
                        !value.StartsWith("'#") && !value.StartsWith("\"#"))

                        value = value[..value.IndexOf("#")].Trim();

                    _Conf[key] = value;
                }
            }

            foreach (var i in _Conf)
            {
                ProcessProperty(i.Key, i.Value);
            }

            return _Conf;
        }
    }
    public string UsedInputConfContent { get; set; } = "";
    public bool Paused { get; set; }
    public event Action? Pause;
    public int VideoRotate { get; set; }
    public Size VideoSize { get; set; }
    public event Action<Size>? VideoSizeChanged;
    public string Path { get; set; } = "";

    void UpdateVideoSize(string w, string h)
    {
        if (string.IsNullOrEmpty(Path))
            return;

        Size size = new Size(GetPropertyInt(w), GetPropertyInt(h));

        if (VideoRotate == 90 || VideoRotate == 270)
            size = new Size(size.Height, size.Width);

        if (size != VideoSize && size != Size.Empty)
        {
            VideoSize = size;
            VideoSizeChanged?.Invoke(size);
        }
    }
    public int PlaylistPos { get; set; } = -1;
    public event Action<int>? PlaylistPosChanged;
    public bool FileEnded { get; set; }
    public event Action? Initialized;

    public List<MediaTrack> MediaTracks { get; set; } = new List<MediaTrack>();
    public object MediaTracksLock { get; } = new object();
    private readonly Regex TitleRegex = new Regex(@"^[\._\-]", RegexOptions.Compiled);
    static string GetLanguage(string id)
    {
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            if (ci.ThreeLetterISOLanguageName == id || Convert(ci.ThreeLetterISOLanguageName) == id)
                return ci.EnglishName;

        return id;

        static string Convert(string id2) => id2 switch
        {
            "bng" => "ben",
            "ces" => "cze",
            "deu" => "ger",
            "ell" => "gre",
            "eus" => "baq",
            "fra" => "fre",
            "hye" => "arm",
            "isl" => "ice",
            "kat" => "geo",
            "mya" => "bur",
            "nld" => "dut",
            "sqi" => "alb",
            "zho" => "chi",
            _ => id2,
        };
    }
    public List<MediaTrack> GetTracks(bool includeInternal = true, bool includeExternal = true)
    {
        List<MediaTrack> tracks = new List<MediaTrack>();

        int trackCount = GetPropertyInt("track-list/count");

        for (int i = 0; i < trackCount; i++)
        {
            bool external = GetPropertyBool($"track-list/{i}/external");

            if ((external && !includeExternal) || (!external && !includeInternal))
                continue;

            string type = GetPropertyString($"track-list/{i}/type");
            string filename = GetPropertyString($"filename/no-ext");
            string title = GetPropertyString($"track-list/{i}/title").Replace(filename, "");

            title = TitleRegex.Replace(title, "");

            if (type == "video")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec == "MPEG2VIDEO")
                    codec = "MPEG2";
                else if (codec == "DVVIDEO")
                    codec = "DV";
                MediaTrack track = new MediaTrack();
                Add(track, codec);
                Add(track, GetPropertyString($"track-list/{i}/demux-w") + "x" + GetPropertyString($"track-list/{i}/demux-h"));
                Add(track, GetPropertyString($"track-list/{i}/demux-fps").Replace(".000000", "") + " FPS");
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                track.Text = "V: " + track.Text.Trim(' ', ',');
                track.Type = "v";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                tracks.Add(track);
            }
            else if (type == "audio")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec.Contains("PCM"))
                    codec = "PCM";
                MediaTrack track = new MediaTrack();
                Add(track, GetLanguage(GetPropertyString($"track-list/{i}/lang")));
                Add(track, codec);
                Add(track, GetPropertyInt($"track-list/{i}/audio-channels") + " ch");
                Add(track, GetPropertyInt($"track-list/{i}/demux-samplerate") / 1000 + " kHz");
                Add(track, GetPropertyBool($"track-list/{i}/forced") ? "Forced" : null);
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                Add(track, GetPropertyBool($"track-list/{i}/external") ? "External" : null);
                Add(track, title);
                track.Text = "A: " + track.Text.Trim(' ', ',');
                track.Type = "a";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                track.External = external;
                tracks.Add(track);
            }
            else if (type == "sub")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec.Contains("PGS"))
                    codec = "PGS";
                else if (codec == "SUBRIP")
                    codec = "SRT";
                else if (codec == "WEBVTT")
                    codec = "VTT";
                else if (codec == "DVB_SUBTITLE")
                    codec = "DVB";
                else if (codec == "DVD_SUBTITLE")
                    codec = "VOB";
                MediaTrack track = new MediaTrack();
                Add(track, GetLanguage(GetPropertyString($"track-list/{i}/lang")));
                Add(track, codec);
                Add(track, GetPropertyBool($"track-list/{i}/forced") ? "Forced" : null);
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                Add(track, GetPropertyBool($"track-list/{i}/external") ? "External" : null);
                Add(track, title);
                track.Text = "S: " + track.Text.Trim(' ', ',');
                track.Type = "s";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                track.External = external;
                tracks.Add(track);
            }
        }

        if (includeInternal)
        {
            int editionCount = GetPropertyInt("edition-list/count");

            for (int i = 0; i < editionCount; i++)
            {
                string title = GetPropertyString($"edition-list/{i}/title");

                if (string.IsNullOrEmpty(title))
                    title = "Edition " + i;

                MediaTrack track = new MediaTrack
                {
                    Text = "E: " + title,
                    Type = "e",
                    ID = i
                };

                tracks.Add(track);
            }
        }

        return tracks;

        static void Add(MediaTrack track, object? value)
        {
            string str = (value + "").Trim();

            if (str != "" && !track.Text.Contains(str))
                track.Text += " " + str + ",";
        }
    }
    public void UpdateExternalTracks()
    {
        int trackListTrackCount = GetPropertyInt("track-list/count");
        int editionCount = GetPropertyInt("edition-list/count");
        int count = MediaTracks.Where(i => i.Type != "g").Count();

        lock (MediaTracksLock)
        {
            if (count != (trackListTrackCount + editionCount))
            {
                MediaTracks = MediaTracks.Where(i => !i.External).ToList();
                MediaTracks.AddRange(GetTracks(false));
            }
        }
    }
    public string VID { get; set; } = "";
    public string AID { get; set; } = "";
    public string SID { get; set; } = "";
    public int Edition { get; set; }
    public List<Chapter> GetChapters()
    {
        List<Chapter> chapters = new List<Chapter>();
        int count = GetPropertyInt("chapter-list/count");

        for (int x = 0; x < count; x++)
        {
            string title = GetPropertyString($"chapter-list/{x}/title");
            double time = GetPropertyDouble($"chapter-list/{x}/time");

            if (string.IsNullOrEmpty(title) ||
                (title.Length == 12 && title.Contains(':') && title.Contains('.')))

                title = "Chapter " + (x + 1);

            chapters.Add(new Chapter() { Title = title, Time = time });
        }

        return chapters;
    }
    public List<TimeSpan> BluRayTitles { get; } = new List<TimeSpan>();

    string[]? _profileNames;
    public string[] ProfileNames
    {
        get
        {
            if (_profileNames != null)
                return _profileNames;

            string[] ignore = ["builtin-pseudo-gui", "encoding", "libmpv", "pseudo-gui", "default"];
            string json = GetPropertyString("profile-list");
            return _profileNames = JsonDocument.Parse(json).RootElement.EnumerateArray()
                .Select(it => it.GetProperty("name").GetString())
                .Where(it => !ignore.Contains(it)).ToArray()!;
        }
    }
    public void SetBluRayTitle(int id) => LoadFiles(new[] { @"bd://" + id }, false, false);

    List<StringPair>? _audioDevices;
    public List<StringPair> AudioDevices
    {
        get
        {
            if (_audioDevices != null)
                return _audioDevices;

            _audioDevices = [];
            string json = GetPropertyString("audio-device-list");
            var enumerator = JsonDocument.Parse(json).RootElement.EnumerateArray();

            foreach (var element in enumerator)
            {
                string name = element.GetProperty("name").GetString()!;
                string description = element.GetProperty("description").GetString()!;
                _audioDevices.Add(new StringPair(name, description));
            }

            return _audioDevices;
        }
    }
    public bool WasInitialSizeSet;
    public TimeSpan Duration;
    public bool IsQuitNeeded { set; get; } = true;
    public AutoResetEvent ShutdownAutoResetEvent { get; } = new AutoResetEvent(false);

    public void Init(IntPtr formHandle, bool processCommandLine)
    {
        App.ApplyShowMenuFix();

        MainHandle = mpv_create();
        Handle = MainHandle;

        var events = Enum.GetValues<mpv_event_id>().Cast<mpv_event_id>();

        foreach (mpv_event_id i in events)
        {
            mpv_request_event(MainHandle, i, 0);
        }

        mpv_request_log_messages(MainHandle, "no");

        if (formHandle != IntPtr.Zero)
            TaskHelp.Run(MainEventLoop);

        if (MainHandle == IntPtr.Zero)
            throw new Exception("error mpv_create");

        if (App.IsTerminalAttached)
        {
            SetPropertyString("terminal", "yes");
            SetPropertyString("input-terminal", "yes");
        }

        if (formHandle != IntPtr.Zero)
        {
            SetPropertyString("force-window", "yes");
            SetPropertyLong("wid", formHandle.ToInt64());
        }

        SetPropertyInt("osd-duration", 2000);

        SetPropertyBool("input-default-bindings", true);
        SetPropertyBool("input-builtin-bindings", false);
        SetPropertyBool("input-media-keys", true);

        SetPropertyString("autocreate-playlist", "filter");
        SetPropertyString("media-controls", "yes");
        SetPropertyString("idle", "yes");
        SetPropertyString("screenshot-directory", "~~desktop/");
        SetPropertyString("osd-playing-msg", "${media-title}");
        SetPropertyString("osc", "yes");
        SetPropertyString("config-dir", ConfigFolder);
        SetPropertyString("config", "yes");

        ////特外
        SetPropertyString("sub-stereo-duplicate", "yes");
        SetPropertyString("sub-stereo-layout", "sbs");
        SetPropertyLong("sub-stereo-offset-px", 0);


        UsedInputConfContent = App.InputConf.GetContent();

        if (!string.IsNullOrEmpty(UsedInputConfContent))
            SetPropertyString("input-conf", @"memory://" + UsedInputConfContent);

        if (processCommandLine)
            CommandLine.ProcessCommandLineArgsPreInit();

        if (CommandLine.Contains("config-dir"))
        {
            string configDir = CommandLine.GetValue("config-dir");
            string fullPath = System.IO.Path.GetFullPath(configDir);
            App.InputConf.Path = fullPath.AddSep() + "input.conf";
            string content = App.InputConf.GetContent();

            if (!string.IsNullOrEmpty(content))
                SetPropertyString("input-conf", @"memory://" + content);
        }

        Environment.SetEnvironmentVariable("MPVNET_VERSION", AppInfo.Version.ToString());  // deprecated

        mpv_error err = mpv_initialize(MainHandle);

        if (err < 0)
            throw new Exception("mpv_initialize error" + BR2 + GetError(err) + BR);

        string idle = GetPropertyString("idle");
        App.Exit = idle == "no" || idle == "once";

        Handle = mpv_create_client(MainHandle, "mpvnet");

        if (Handle == IntPtr.Zero)
            throw new Exception("mpv_create_client error");

        mpv_request_log_messages(Handle, "info");

        if (formHandle != IntPtr.Zero)
            TaskHelp.Run(EventLoop);

        // otherwise shutdown is raised before media files are loaded,
        // this means Lua scripts that use idle might not work correctly
        SetPropertyString("idle", "yes");

        SetPropertyString("user-data/frontend/name", "mpv.net");
        SetPropertyString("user-data/frontend/version", AppInfo.Version.ToString());
        SetPropertyString("user-data/frontend/process-path", Environment.ProcessPath!);

        ObservePropertyBool("pause", value =>
        {
            Paused = value;
            Pause?.Invoke();
        });

        VideoRotate = GetPropertyInt("video-rotate");

        ObservePropertyInt("video-rotate", value =>
        {
            if (VideoRotate != value)
            {
                VideoRotate = value;
                UpdateVideoSize("dwidth", "dheight");
            }
        });

        ObservePropertyInt("playlist-pos", value =>
        {
            PlaylistPos = value;
            PlaylistPosChanged?.Invoke(value);

            if (FileEnded && value == -1)
                if (GetPropertyString("keep-open") == "no" && App.Exit)
                    CommandV("quit");
        });

        Initialized?.Invoke();
    }
    public DateTime LastLoad;
    public static string ConvertFilePath(string path)
    {
        if ((path.Contains(":/") && !path.Contains("://")) || (path.Contains(":\\") && path.Contains('/')))
            path = path.Replace("/", "\\");

        if (!path.Contains(':') && !path.StartsWith("\\\\") && File.Exists(path))
            path = System.IO.Path.GetFullPath(path);

        return path;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibrary(string path);

    [SupportedOSPlatform("windows")]
    public static string GetShortcutTarget(string path)
    {
        Type? t = Type.GetTypeFromProgID("WScript.Shell");
        dynamic? sh = Activator.CreateInstance(t!);
        return sh?.CreateShortcut(path).TargetPath!;
    }

    bool _wasAviSynthLoaded;
    [SupportedOSPlatform("windows")]
    void LoadAviSynth()
    {
        if (!_wasAviSynthLoaded)
        {
            string? dll = Environment.GetEnvironmentVariable("AviSynthDLL");  // StaxRip sets it in portable mode
            LoadLibrary(File.Exists(dll) ? dll : "AviSynth.dll");
            _wasAviSynthLoaded = true;
        }
    }
    public void LoadISO(string path)
    {
        using var mi = new MediaInfo(path);

        if (mi.GetGeneral("Format") == "ISO 9660 / DVD Video")
        {
            Command("stop");
            Thread.Sleep(500);
            SetPropertyString("dvd-device", path);
            LoadFiles([@"dvd://"], false, false);
        }
        else
        {
            Command("stop");
            Thread.Sleep(500);
            SetPropertyString("bluray-device", path);
            LoadFiles([@"bd://"], false, false);
        }
    }

    public void LoadFiles(string[]? files, bool loadFolder, bool append)
    {
        if (files == null || files.Length == 0)
            return;

        if ((DateTime.Now - LastLoad).TotalMilliseconds < 1000)
            append = true;

        LastLoad = DateTime.Now;

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];

            if (string.IsNullOrEmpty(file))
                continue;

            if (file.Contains('|'))
                file = file[..file.IndexOf("|")];

            file = ConvertFilePath(file);

            string ext = file.Ext();

            if (OperatingSystem.IsWindows())
            {
                switch (ext)
                {
                    case "avs": LoadAviSynth(); break;
                    case "lnk": file = GetShortcutTarget(file); break;
                }
            }

            if (ext == "iso")
                LoadISO(file);
            else if (FileTypes.Subtitle.Contains(ext))
                CommandV("sub-add", file);
            else
            {
                if (i == 0 && !append)
                    CommandV("loadfile", file);
                else
                    CommandV("loadfile", file, "append");
            }
        }

        if (string.IsNullOrEmpty(GetPropertyString("path")))
            SetPropertyInt("playlist-pos", 0);
    }
    public List<MpvClient> Clients { get; } = new List<MpvClient>();
    public void Destroy()
    {
        mpv_destroy(MainHandle);
        mpv_destroy(Handle);

        foreach (var client in Clients)
        {
            mpv_destroy(client.Handle);
        }
    }
    public string GetProfiles()
    {
        string json = GetPropertyString("profile-list");
        StringBuilder sb = new StringBuilder();

        foreach (var profile in JsonDocument.Parse(json).RootElement.EnumerateArray())
        {
            sb.Append(profile.GetProperty("name").GetString() + BR2);

            foreach (var it in profile.GetProperty("options").EnumerateArray())
                sb.AppendLine($"    {it.GetProperty("key").GetString()} = {it.GetProperty("value").GetString()}");

            sb.Append(BR);
        }

        return sb.ToString();
    }
    public void LoadDiskFolder(string path)
    {
        Command("stop");
        Thread.Sleep(500);

        if (Directory.Exists(path + "\\BDMV"))
        {
            SetPropertyString("bluray-device", path);
            LoadFiles([@"bd://"], false, false);
        }
        else
        {
            SetPropertyString("dvd-device", path);
            LoadFiles([@"dvd://"], false, false);
        }
    }
    public string GetDecoders()
    {
        var list = JsonDocument.Parse(GetPropertyString("decoder-list")).RootElement.EnumerateArray()
            .Select(it => $"{it.GetProperty("codec").GetString()} - {it.GetProperty("description").GetString()}")
            .OrderBy(it => it);

        return string.Join(BR, list);
    }
}

