using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Navigation;
using PrintJobInterceptor.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly INavigationService _navigationService;

    public RoutingState Router => _navigationService.Router;

    [Reactive] public SidebarViewModel SidebarViewModel { get; set; }
    
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public MainWindowViewModel()
    {
        _navigationService = Locator.Current.GetService<INavigationService>()!;
        
        SidebarViewModel = new SidebarViewModel();
        
        GoBackCommand = ReactiveCommand.Create(TryNavigateBack);
        
        Router.NavigateAndReset.Execute(Locator.Current.GetService<HomeViewModel>()!);
    }

    private void TryNavigateBack()
    {
        if (_navigationService.Router.NavigationStack.Count <= 1) return;
        
        _navigationService.NavigateBack();
        // RxApp.MainThreadScheduler.Schedule(() => _navigationService.NavigateBack());
        
    }
}
