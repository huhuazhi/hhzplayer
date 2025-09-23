﻿
using NGettext.Wpf;

using System.Globalization;

namespace MpvNet.Windows.WPF;

public class WpfTranslator : ITranslator
{
    string _localizerLangauge = "";

    static Language[] Languages { get; } = new Language[] {
        new("bulgarian", "bg", "bg"),
        new("chinese-china", "zh-CN", "zh"),  // Chinese (Simplified)
        new("english", "en", "en"),
        new("french", "fr", "fr"),
        new("german", "de", "de"),
        new("japanese", "ja", "ja"),
        new("korean", "ko", "ko"),
        new("polish", "pl", "pl"),
        new("russian", "ru", "ru"),
        new("turkish", "tr", "tr"),
    };

    public string Gettext(string msgId)
    {
        InitNGettextWpf();
        return Translation._(msgId);
    }

    public string GetParticularString(string context, string text)
    {
        InitNGettextWpf();
        return Translation.GetParticularString(context, text);
    }

    void InitNGettextWpf()
    {
        if (Translation.Localizer == null || _localizerLangauge != App.Language)
        {
            CompositionRoot.Compose("mpvnet", GetCulture(App.Language), Folder.Startup + "Locale");
            _localizerLangauge = App.Language;
        }
    }

    string GetSystemLanguage()
    {
        string twoLetterName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        if (twoLetterName == "zh")
            return "chinese-china";  // Chinese (Simplified)

        return new CultureInfo(twoLetterName).EnglishName.ToLowerInvariant();
    }

    CultureInfo GetCulture(string name)
    {
        if (name == "system")
            name = GetSystemLanguage();

        foreach (Language lang in Languages)
            if (lang.MpvNetName == name)
                return new CultureInfo(lang.CultureInfoName);

        return new CultureInfo("en");
    }

    class Language
    {
        public string MpvNetName { get; }
        public string CultureInfoName { get; }
        public string TwoLetterName { get; }

        public Language(string mpvNetName, string cultureInfoName, string twoLetterName)
        {
            MpvNetName = mpvNetName;
            CultureInfoName = cultureInfoName;
            TwoLetterName = twoLetterName;
        }
    }
}
