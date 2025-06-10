using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;

namespace PrintJobInterceptor.Desktop.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext)
                .DisposeWith(disposables);
        });
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);    }
}