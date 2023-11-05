using Avalonia.ReactiveUI;
using Audio.ViewModels;

namespace Audio.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
    }
}
