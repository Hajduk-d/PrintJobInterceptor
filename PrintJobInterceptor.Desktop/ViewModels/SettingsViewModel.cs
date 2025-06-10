
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using PrintJobInterceptor.Desktop.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class SettingsViewModel : ReactiveViewModel
{
    private Settings _settings;
    [Reactive] public string PrintJobRelationTime { get; set; }
    
    [Reactive] public ThemeMode Theme { get; set; }

    public ObservableCollection<ThemeMode> ThemeOptions { get; set; } = [ThemeMode.Light, ThemeMode.Dark, ThemeMode.System];
    
    public SettingsViewModel()
    {
       _settings = Locator.Current.GetService<Settings>()!;
       
       Theme = _settings.Theme;
       PrintJobRelationTime = _settings.PrintJobRelationTime.ToString();
       
       this.WhenAnyValue(x => x.Theme)
           .Where(x => x != ThemeMode.None)
           .Subscribe(_ => ThemeChangeRequested(Theme));

       this.WhenAnyValue(x => x.PrintJobRelationTime)
           .WhereNotNull()
           .ObserveOn(RxApp.TaskpoolScheduler)
           .Subscribe(_ => PrintJobRelationTimeChanged());

       this.WhenAnyValue(x => x.Theme, x => x.PrintJobRelationTime)
           .Skip(1)
           .Throttle(TimeSpan.FromMilliseconds(500))
           .Subscribe(_ => _settings.Save());
       

    }

    public void ThemeChangeRequested(ThemeMode theme)
    {
        _settings.Theme = theme;
        Application.Current.ThemeMode = theme;
    }

    public void PrintJobRelationTimeChanged()
    {
        _settings.PrintJobRelationTime = Convert.ToInt32(PrintJobRelationTime);
        Locator.Current.GetService<PrinterService>()!.RelatedPrintJobTime = _settings.PrintJobRelationTime;
    }
}