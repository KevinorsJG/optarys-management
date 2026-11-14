using CommunityToolkit.Mvvm.ComponentModel;
using Management.App.UI.ViewModels;

namespace Optarys.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
