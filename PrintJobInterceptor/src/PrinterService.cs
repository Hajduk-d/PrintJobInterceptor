using System.Collections.Concurrent;
using System.Management;

namespace PrintJobInterceptor;

public class PrinterService
{
    internal PrinterMonitor PrinterMonitor = null!;
    internal PrintJobMonitor PrintJobMonitor = null!;
    
    public ConcurrentDictionary<string, Printer> Printers = [];
    public ConcurrentDictionary<uint, PrintJob> PrintJobs = [];
    public List<Document> Documents { get; set; } = [];
    
    public event Action<Document>? NewDocumentCreated; 
    public event Action<PrintJob>? OnPrintJobAdded;
    public event Action<PrintJob>? OnPrintJobModified;

    public int RelatedPrintJobTime { get; set; } = 10;
    
    public void StartService()
    {
        PrinterMonitor = new PrinterMonitor();
        
        PrinterMonitor.OnPrinterAdded += PrinterAdded;
        PrinterMonitor.OnPrinterRemoved += PrinterRemoved;
        PrinterMonitor.OnPrinterStatusChanged += PrinterStatusChanged;
        
        PrinterMonitor.StartPrinterMonitoring();

        PrintJobMonitor = new PrintJobMonitor();
        
        PrintJobMonitor.OnPrintJobAdded += PrintJobAdded;
        PrintJobMonitor.OnPrintJobDeleted += PrintJobDeleted;
        PrintJobMonitor.OnPrintJobModified += PrintJobModified;

        PrintJobMonitor.StartJobMonitoring();
    }
    
    #region Printer
    private void PrinterStatusChanged(PrinterData data)
    {
        if (!Printers.TryGetValue(data.Id, out Printer? printer)) return;

        ServiceLogger.LogInfo($"Printer {printer.Id} updated");
        
        printer.UpdateStatus(data);
    }

    private void PrinterRemoved(string printerId)
    {

        
        Printers.TryRemove(printerId, out Printer? _);
        
        ServiceLogger.LogInfo($"Printer {printerId} removed");
        
    }

    private void PrinterAdded(Printer printer)
    {
        Printers.TryAdd(printer.Id, printer);
        
        ServiceLogger.LogInfo($"New Printer {printer.Id} added");
    }
    
    #endregion

    #region PrintJob

    private void PrintJobAdded(PrintJob printJob)
    {
        if (!PrintJobs.TryAdd(printJob.JobId, printJob))
        {
            ServiceLogger.LogWarn($"Could not add printJob {printJob.JobId} to collection");
            return;
        }

        int documentCount = Documents.Count;
        Document doc = GetPrintJobDocument(printJob);
        
        if (Printers.TryGetValue(printJob.PrinterName, out Printer? printer))
        {
            printer.RegisterPrintJob(printJob);
            
        }
        else
        {
            ServiceLogger.LogWarn($"Printer with id {printJob.PrinterName} was not found for printJob {printJob.JobId}");
            return;
        }

        printJob.InitReferences(doc, printer);

        if (documentCount < Documents.Count)
        {
            NewDocumentCreated?.Invoke(doc);
            ServiceLogger.LogInfo($"New print document registered {doc.Name}");
        }
        
        OnPrintJobAdded?.Invoke(printJob);
        
        ServiceLogger.LogInfo($"New Printjob {printJob.JobId} added");
    }
    
    private void PrintJobDeleted(uint id)
    {
        if (!PrintJobs.TryGetValue(id, out PrintJob? printJob)) 
        {
            ServiceLogger.LogWarn($"Failed to find printjob {id} to delete");
            return;
        }

        PrintJobData printJobData = new() { Status = "Canceled" };
        printJob.UpdateStatus(printJobData);
        
        ServiceLogger.LogInfo($"Printjob {printJob.JobId} has been canceled");
        
        OnPrintJobModified?.Invoke(printJob);
    }

    private void PrintJobModified(PrintJobData printJobData)
    {
        if (!PrintJobs.TryGetValue(printJobData.JobId, out PrintJob? printJob))
        {
            ServiceLogger.LogWarn($"Failed to find printjob {printJobData.JobId} to update");
            return;
        }
        printJob.UpdateStatus(printJobData);
        
        ServiceLogger.LogInfo($"Printjob {printJob.JobId} changed");
        
        OnPrintJobModified?.Invoke(printJob);
    }

    #endregion

    #region Document

    /// <summary>
    /// Assigns a print job to an existing or new document.
    /// First attempts to find documents that were printed within the specified time frame (RelatedPrintJobTime)
    /// and have the same file path as the current print job. For each matching document, tries to associate
    /// the print job based on additional validation criteria. If no existing document can accept the print job,
    /// create a new document instance.
    /// </summary>
    /// <param name="printJob">The print job to be associated with a document</param>
    /// <returns>Either an existing document that accepted the print job or a newly created document</returns>
    private Document GetPrintJobDocument(PrintJob printJob)
    { 
        Document? matchingDocument = Documents
            .Where(x => x.Name == printJob.DocumentName && //this should be the path not the document name
                Math.Abs((x.PrintJobStarted - printJob.StartTime).TotalSeconds) <= RelatedPrintJobTime)
            .FirstOrDefault(doc => doc.IsPrintJobRelated(printJob));

        return matchingDocument ?? CreateNewDocument(printJob);
    }

    private Document CreateNewDocument(PrintJob printJob)
    {
        Document newDoc = new(printJob);
        Documents.Add(newDoc);
        return newDoc;
    }

    #endregion
}