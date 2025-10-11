
namespace HHZPlayer.Windows.WPF;

interface ISettingControl
{
    bool Contains(string searchString);
    Setting Setting { get; }
}
