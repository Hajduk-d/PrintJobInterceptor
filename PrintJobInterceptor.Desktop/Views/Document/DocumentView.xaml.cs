using System.Reactive.Disposables;
using System.Windows.Controls;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;

namespace PrintJobInterceptor.Desktop.Views;

public partial class DocumentView : ReactiveUserControl<DocumentViewModel>
{
    public DocumentView()
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