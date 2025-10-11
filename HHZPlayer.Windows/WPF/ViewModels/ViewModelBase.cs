
using CommunityToolkit.Mvvm.ComponentModel;

using HHZPlayer.Windows.UI;

namespace HHZPlayer.Windows.WPF.ViewModels;

public class ViewModelBase : ObservableObject
{
    public Theme Theme => Theme.Current!;
}
