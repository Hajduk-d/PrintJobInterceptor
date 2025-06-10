
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows;
using PrintJobInterceptor.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;
public class HomeViewModel : ReactiveViewModel
{
    private readonly PrinterService _printerService;
    private readonly INavigationService _navigationService;

    [Reactive] public PrinterViewModel? SelectedPrinter { get; set; }
    [Reactive] public PrintJobViewModel? SelectedPrintJob { get; set; }
    [Reactive] public DocumentViewModel? SelectedDocument { get; set; }

    public ObservableCollection<PrinterViewModel> PrinterVms { get; set; } = [];
    public ObservableCollection<PrintJobViewModel> PrintJobVms { get; set; } = [];
    public ObservableCollection<DocumentViewModel> DocumentVms { get; set; } = [];
    public ReactiveCommand<IRoutableViewModel, Unit> ItemDoubleClickedCommand { get; }

    public HomeViewModel()
    {
        _navigationService = Locator.Current.GetService<INavigationService>()!;
        _printerService = Locator.Current.GetService<PrinterService>()!;
        
        _printerService.NewDocumentCreated += DocumentServiceNewDocumentCreated;
        
        _printerService.StartService();
        _printerService.OnPrintJobAdded += NewPrintJobArrived;

        ItemDoubleClickedCommand = ReactiveCommand.Create<IRoutableViewModel>(HandleItemDoubleClick);
        
        InitPrinters();
    }

    private void DocumentServiceNewDocumentCreated(Document document)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            DocumentVms.Add(AppBootstrapper.GetDocumentViewModel(document));
        });
    }
    
    private void NewPrintJobArrived(PrintJob printJob)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            PrintJobVms.Add(AppBootstrapper.GetPrintJobViewModel(printJob));
        });
    }

    private void HandleItemDoubleClick(IRoutableViewModel viewModel)
    {
        _navigationService.NavigateTo(viewModel);
    }



    private void InitPrinters()
    {
        foreach (Printer printer in _printerService.Printers.Values)
        {
            PrinterVms.Add(AppBootstrapper.GetPrinterViewModel(printer));
        }
    }
}