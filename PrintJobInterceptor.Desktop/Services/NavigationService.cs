using ReactiveUI;
using Splat;

namespace PrintJobInterceptor.Desktop.Services
{
    public interface INavigationService
    {
        RoutingState Router { get; }
        void NavigateTo<TViewModel>() where TViewModel : class, IRoutableViewModel;
        void NavigateTo(IRoutableViewModel? viewModel);
        void NavigateBack();
    }

    public class NavigationService : INavigationService
    {
        public RoutingState Router { get; }

        public NavigationService()
        {
            Router = new RoutingState();
        }

        public void NavigateTo<TViewModel>() where TViewModel : class, IRoutableViewModel
        {
            TViewModel? vm = Locator.Current.GetService<TViewModel>();
            if (vm == null)
            {
                ServiceLogger.LogWarn($"Could not find viewmodel {typeof(TViewModel)}");
                return;
            }

            Router.Navigate.Execute(vm);
        }

        public void NavigateTo(IRoutableViewModel? viewModel)
        {
            if (viewModel == null) return;
            
            try
            {
                Router.Navigate.Execute(viewModel);
            }
            catch (Exception e)
            {
                ServiceLogger.LogError($"Failed to change view {e}");
            }
        }

        public void NavigateBack()
        {
            Router.NavigateBack.Execute();
        }
    }
}