using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using PrintJobInterceptor.Desktop.Services;
using PrintJobInterceptor.Desktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class SidebarViewModel : ReactiveViewModel
{
    private INavigationService _navigationService;

    [Reactive] public ObservableCollection<SidebarItemViewModel> SidebarItemViewModels { get; set; } = [];
    [Reactive] public SidebarItemViewModel? SelectedItem { get; set; }
    
    public SidebarViewModel()
    {
        _navigationService = Locator.Current.GetService<INavigationService>()!;

        InitSidebarItems();

        SelectedItem = SidebarItemViewModels.First();

        this.WhenAnyValue(x => x.SelectedItem)
            .WhereNotNull()
            .Subscribe(_ => _navigationService.NavigateTo(SelectedItem.ViewModel));

        _navigationService.Router.CurrentViewModel
            .WhereNotNull()
            .Subscribe(current =>
        {
            SidebarItemViewModel? matchingSidebarItem = SidebarItemViewModels
                .FirstOrDefault(item => item.ViewModel.GetType() == current.GetType());
            SelectedItem = matchingSidebarItem ;
        });
    }

    private void InitSidebarItems()
    {
        SidebarItemViewModel homeVm = new()
        {
            Title = "Home",
            Icon = "\uE80F",
            ViewModel = Locator.Current.GetService<HomeViewModel>()!
        };
        SidebarItemViewModel settingsVm = new()
        {
            Title = "Settings",
            Icon = "\uE713",
            ViewModel = Locator.Current.GetService<SettingsViewModel>()!
        };
         
        SidebarItemViewModels.Add(homeVm);
        SidebarItemViewModels.Add(settingsVm);
    }
    
    
}