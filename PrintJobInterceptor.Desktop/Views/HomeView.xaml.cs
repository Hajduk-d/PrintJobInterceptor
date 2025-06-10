using System.Reactive.Disposables;
using System.Windows.Controls;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;

namespace PrintJobInterceptor.Desktop.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
            .BindTo(this, x => x.DataContext)
            .DisposeWith(disposables);
        });
    }
}