
namespace MpvNet;

public static class Global
{
    public static readonly string BR = Environment.NewLine;
    public static readonly string BR2 = Environment.NewLine + Environment.NewLine;
    public static readonly HHZPlayer Player = new HHZPlayer();
    public static readonly HHZPlayer Core = Player; // deprecated
    public static readonly AppClass App = new AppClass();

    public static string _(string value) => Translator.Current!.Gettext(value);
    public static string _p(string context, string value) => Translator.Current!.GetParticularString(context, value);
}
