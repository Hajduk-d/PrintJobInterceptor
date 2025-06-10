using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;
using Splat;

namespace PrintJobInterceptor.Desktop;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = (MainWindowViewModel)Locator.Current.GetService<IScreen>()!;

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext)
                .DisposeWith(disposables);
        });
    }
}