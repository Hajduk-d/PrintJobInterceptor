using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using PrintJobInterceptor.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public partial class PrinterViewModel : ReactiveViewModel
{
    private readonly INavigationService _navigationService;

    public Printer Printer { get; private set; }
    public string DriverName { get; set; }
    public ObservableCollection<PrintJobViewModel> PrintJobs { get; set; } = [];
    public string Id { get; set; }
    public string Location { get; set; }
    public string PortName { get; set; }
    [Reactive] public uint State { get; set; }
    [Reactive] public uint Status { get; set; }
    [Reactive] public PrintJobViewModel SelectedPrintJob { get; set; }
    
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelAllJobsCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintTestPageCommand { get; }
    public ReactiveCommand<IRoutableViewModel, Unit> RoutToViewModelCommand { get; }


    public PrinterViewModel(Printer printer)
    {
        Printer = printer;
        _navigationService = Locator.Current.GetService<INavigationService>()!;

        Id = Printer.Id;
        State = Printer.State;
        Status = Printer.Status;
        DriverName = Printer.DriverName;
        Location = Printer.Location;
        PortName = Printer.PortName;

        foreach (PrintJob printJob in printer.PrintJobs.Values)
        {
            AddPrintJob(printJob);
        }
        
        Printer.OnPrintJobAdded += AddPrintJob;
        
        PrintTestPageCommand = ReactiveCommand.CreateFromObservable(() => 
            Observable.StartAsync(async () =>
            {
                await Printer.PrintTestPage();
            }));
        
        PauseCommand = ReactiveCommand.CreateRunInBackground(() => Printer.Pause());
        ResumeCommand = ReactiveCommand.CreateRunInBackground(() => Printer.Resume());
        CancelAllJobsCommand = ReactiveCommand.CreateRunInBackground(() => Printer.CancelAllJobs());
        RoutToViewModelCommand = ReactiveCommand.Create<IRoutableViewModel>(RouteToViewModel);

        

        this.WhenAnyValue(x => x.Printer.State)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => State = Printer.State);

        this.WhenAnyValue(x => x.Printer.Status)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Status = Printer.Status);
        
    }

    private void AddPrintJob(PrintJob printJob)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            PrintJobs.Add(AppBootstrapper.GetPrintJobViewModel(printJob));
        });
    }

    private void RouteToViewModel(IRoutableViewModel viewModel)
    {
        _navigationService.NavigateTo(viewModel);
    }
}