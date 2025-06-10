using System.Reactive.Disposables;
using System.Windows.Controls;
using PrintJobInterceptor.Desktop.Converter;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;
using ReactiveUI.Wpf;

namespace PrintJobInterceptor.Desktop.Views;

public partial class PrinterView : ReactiveUserControl<PrinterViewModel>
{

    private readonly StatusToTextConverter _statusConverter = new(); 
    
    public PrinterView()
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