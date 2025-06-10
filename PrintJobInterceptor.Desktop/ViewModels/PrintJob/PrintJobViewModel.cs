using System.Reactive;
using PrintJobInterceptor.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class PrintJobViewModel : ReactiveViewModel
{
    private DocumentViewModel? _document;
    public DocumentViewModel Document => _document ??= AppBootstrapper.GetDocumentViewModel(PrintJob.Document);

    public IEnumerable<DocumentViewModel> DocumentAsEnumerable => [Document];
    public PrintJob PrintJob { get; }
    public string DocumentName { get; set; }
    public uint JobId { get; set; }
    public string Owner { get; set; }
    public string DataType { get; set; }
    public string JobName { get; set; }
    public string PrinterName => Printer.Id;
    public PrinterViewModel Printer { get; set; }
    public IEnumerable<PrinterViewModel> PrinterAsEnumerable => [Printer];
    [Reactive] public string Status { get; set; }

    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<IRoutableViewModel, Unit> RouteToViewModelCommand { get; }
    public PrintJobViewModel(PrintJob printJob)
    {
        
        PrintJob = printJob;

        DocumentName = PrintJob.DocumentName;
        JobId = PrintJob.JobId;
        Status = PrintJob.Status;
        Owner = PrintJob.Owner;
        Printer = AppBootstrapper.GetPrinterViewModel(PrintJob.Printer);
        DataType = PrintJob.DataType;
        JobName = PrintJob.JobName;
        
        RouteToViewModelCommand = ReactiveCommand.Create<IRoutableViewModel>(RouteToViewModel);
        PauseCommand = ReactiveCommand.Create(Pause);
        ResumeCommand = ReactiveCommand.Create(Resume);
        CancelCommand = ReactiveCommand.Create(Cancel);
        

        this.WhenAnyValue(x => x.PrintJob.Status)
            .Subscribe(_ => Status = PrintJob.Status);
    }

    private void Pause()
    {
        PrintJob.Pause();
    }

    private void Resume()
    {
        PrintJob.Resume();
    }

    private void Cancel()
    {
        PrintJob.Cancel();
    }

    private void RouteToViewModel(IRoutableViewModel viewModel)
    {
        Locator.Current.GetService<INavigationService>()!.NavigateTo(viewModel);
    }
}