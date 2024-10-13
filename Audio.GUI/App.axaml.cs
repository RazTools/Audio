using Audio.GUI.Services;
using Audio.GUI.ViewModels;
using Audio.GUI.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Audio.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        ServiceCollection services = new();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            services.AddSingleton<IPlatformServiceProvider>(new PlatformServiceProvider(desktop.MainWindow));
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
            IRenderRoot? visualRoot = singleViewPlatform.MainView.GetVisualRoot();
            if (visualRoot is TopLevel topLevel)
            {
                services.AddSingleton<IPlatformServiceProvider>(new PlatformServiceProvider(topLevel));
            }
        }

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());

        base.OnFrameworkInitializationCompleted();
    }
}
