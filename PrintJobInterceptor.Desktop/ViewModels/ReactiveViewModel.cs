using ReactiveUI;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class ReactiveViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public IScreen HostScreen { get; } = Locator.Current.GetService<IScreen>()!;

}