
using HHZPlayer.Windows.WPF.ViewModels;

namespace HHZPlayer.Windows.WPF.Views;

public partial class AboutWindow
{

    public AboutWindow()
    {
        InitializeComponent();
        var vm = new AboutViewModel();
        DataContext = vm;
        vm.CloseAction = Close;
    }
}
