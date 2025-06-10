using System.Windows;
using PrintJobInterceptor.Desktop.Services;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;
using Splat;
using Velopack;

namespace PrintJobInterceptor.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        VelopackApp.Build().Run();

        base.OnStartup(e);

        AppBootstrapper bootstrapper = new ();
        
        Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);
    }
}