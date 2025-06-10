using ReactiveUI;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class SidebarItemViewModel : ReactiveViewModel
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public IRoutableViewModel ViewModel { get; set; }
    
}