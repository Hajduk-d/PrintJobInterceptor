using System.ComponentModel;

namespace PrintJobInterceptor;

public partial class Document : INotifyPropertyChanged
{
    public event Action<PrintJob>? OnPrintJobAdded; 
    public string Name { get; set; }
    public string Path { get; set; }
    public uint Size { get; set; }
    public DateTime TimeCreated { get; set; }
    public DateTime PrintJobStarted { get; set; }
    public string Owner { get; set; }
    public Printer Printer { get; set; }
    public List<PrintJob> RelatedPrintJobs { get; set; } = [];

    public Document(PrintJob printJob)
    {
        Name = printJob.DocumentName;
        PrintJobStarted = printJob.StartTime;
        Owner = printJob.Owner;
        printJob.FinishedInit += PrintJobOnFinishedInit;
    }

    public bool IsPrintJobRelated(PrintJob printJob)
    {
        if (printJob.Owner != Owner) return false;
        if (printJob.DocumentName != Name) return false;
        
        printJob.FinishedInit += PrintJobOnFinishedInit;

        return true;
    }

    private void PrintJobOnFinishedInit(PrintJob printJob)
    {
        RelatedPrintJobs.Add(printJob);
        Printer = printJob.Printer;
        printJob.FinishedInit -= PrintJobOnFinishedInit;
        OnPrintJobAdded?.Invoke(printJob);
    }

    public void Pause()
    {
        foreach (PrintJob relatedPrintJob in RelatedPrintJobs)
        {
            relatedPrintJob.Pause();
        }
    }

    public void Resume()
    {
        foreach (PrintJob relatedPrintJob in RelatedPrintJobs)
        {
            relatedPrintJob.Resume();
        }
    }

    public void Cancel()
    {
        foreach (PrintJob relatedPrintJob in RelatedPrintJobs)
        {
            relatedPrintJob.Cancel();
        }
    }
}