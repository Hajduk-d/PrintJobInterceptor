using System.IO;
using System.Text.Json;
using PrintJobInterceptor.Desktop.Models;
using PrintJobInterceptor.Desktop.Services;
using PrintJobInterceptor.Desktop.Views;
using ReactiveUI;
using Splat;

namespace PrintJobInterceptor.Desktop.ViewModels;

public class AppBootstrapper : IEnableLogger
{
    public AppBootstrapper()
    {
        Locator.CurrentMutable.RegisterConstant(new NavigationService(), typeof(INavigationService));

        Settings settings = LoadSettings();
        Locator.CurrentMutable.RegisterConstant(settings, typeof(Settings));
        
        PrinterService printerService = new (){RelatedPrintJobTime = settings.PrintJobRelationTime};
        Locator.CurrentMutable.RegisterConstant(printerService, typeof(PrinterService));
        
        RegisterViewModel(new HomeViewModel(), typeof(HomeViewModel));
        RegisterViewModel(new SettingsViewModel(), typeof(SettingsViewModel));

        MainWindowViewModel screen = new();
        Locator.CurrentMutable.RegisterConstant(screen, typeof(IScreen));
    }

    public Settings LoadSettings()
    {
        string filePath = Settings.FilePath;

        if (!File.Exists(filePath)) return new Settings();

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Settings>(json, Settings.DefaultOptions) ?? new Settings();
    }

     public static void RegisterViewModel(IRoutableViewModel routableViewModel, Type type)
    {
        Locator.CurrentMutable.RegisterConstant(routableViewModel, type);
    }

    public static PrinterViewModel GetPrinterViewModel(Printer printer)
    {
        PrinterViewModel? printerVm = Locator.Current.GetServices<PrinterViewModel>()
            .FirstOrDefault(x => x.Printer == printer);
        if (printerVm != null) return printerVm;

        printerVm = new PrinterViewModel(printer);
        Locator.CurrentMutable.RegisterConstant(printerVm, typeof(PrinterViewModel));
        return printerVm;
    }

    public static DocumentViewModel GetDocumentViewModel(Document document)
    {
        DocumentViewModel? documentVm = Locator.Current.GetServices<DocumentViewModel>()
            .FirstOrDefault(x => x.Document == document);
        if (documentVm != null) return documentVm;

        documentVm = new DocumentViewModel(document);
        Locator.CurrentMutable.RegisterConstant(documentVm, typeof(DocumentViewModel));
        return documentVm;
    }
    
    public static PrintJobViewModel GetPrintJobViewModel(PrintJob printJob)
    {
        PrintJobViewModel? printJobViewModel = Locator.Current.GetServices<PrintJobViewModel>()
            .FirstOrDefault(x => x.PrintJob == printJob);
        if (printJobViewModel != null) return printJobViewModel;
    
        printJobViewModel = new PrintJobViewModel(printJob);
        Locator.CurrentMutable.RegisterConstant(printJobViewModel, typeof(PrintJobViewModel));
        return printJobViewModel;
    }
}