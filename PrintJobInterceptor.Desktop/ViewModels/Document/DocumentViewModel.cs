using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using PrintJobInterceptor;
using PrintJobInterceptor.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class DocumentViewModel : ReactiveViewModel
{
    public Document Document { get; }
    public string Name { get; set; }
    public string Owner { get; set; }
    public DateTime TimeCreated { get; set; }
    public DateTime PrintJobStarted { get; set; }
    public PrinterViewModel Printer { get; set; }
    
    public IEnumerable<PrinterViewModel> PrinterAsEnumerable => [Printer];
    public ObservableCollection<PrintJobViewModel> PrintJobs { get; set; } = [];
    
    [Reactive] public PrintJobViewModel SelectedPrintJob { get; set; }
    
    public ReactiveCommand<IRoutableViewModel, Unit> RouteToViewModelCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    
    public DocumentViewModel(Document document)
    {
        Document = document;
        Name = document.Name;
        Owner = document.Owner;
        TimeCreated = document.TimeCreated;
        PrintJobStarted = document.PrintJobStarted;
        Printer = AppBootstrapper.GetPrinterViewModel(document.Printer);
        
        PauseCommand = ReactiveCommand.CreateRunInBackground(Pause);
        ResumeCommand = ReactiveCommand.CreateRunInBackground(Resume);
        CancelCommand = ReactiveCommand.CreateRunInBackground(Cancel);
        RouteToViewModelCommand = ReactiveCommand.Create<IRoutableViewModel>(RouteToViewModel);
        
        Document.OnPrintJobAdded += DocumentOnPrintJobAdded;
        InitPrintJobs();
    }
    
    private void RouteToViewModel(IRoutableViewModel viewModel)
    {
        Locator.Current.GetService<INavigationService>()!.NavigateTo(viewModel);
    }

    private void InitPrintJobs()
    {
        foreach (PrintJob printJob in Document.RelatedPrintJobs)
        {
            PrintJobs.Add(AppBootstrapper.GetPrintJobViewModel(printJob));
        }
    }

    private void DocumentOnPrintJobAdded(PrintJob printJob)
    {
        PrintJobs.Add(AppBootstrapper.GetPrintJobViewModel(printJob));
    }
    
    private void Pause()
    {
        Document.Pause();
    }

    private void Resume()
    {
        Document.Resume();
    }

    private void Cancel()
    {
        Document.Cancel();
    }

}