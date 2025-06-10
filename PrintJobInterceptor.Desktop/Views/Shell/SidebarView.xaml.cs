using System.Reactive.Disposables;
using PrintJobInterceptor.Desktop.ViewModels;
using ReactiveUI;

namespace PrintJobInterceptor.Desktop.Views;

public partial class SidebarView : ReactiveUserControl<SidebarViewModel>
{
    public SidebarView()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {

        });
    }
}