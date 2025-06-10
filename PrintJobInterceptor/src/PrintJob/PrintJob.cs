using System.ComponentModel;
using System.Management;

namespace PrintJobInterceptor;

public partial class PrintJob : INotifyPropertyChanged
{
    public event Action<PrintJob>? FinishedInit; 
    public uint JobId { get; private set; }
    public string JobName { get; private set; }
    public string DocumentName { get; private set; }
    public string DataType { get; private set; }
    public string Status { get; private set; }
    public string Owner { get; private set; }
    public string PrintProcessor { get; private set; }
    public string PrinterName { get; private set; }
    public DateTime StartTime { get; private set; }
    public Printer Printer { get; set; }
    public Document Document { get; set; }

    public PrintJob(PrintJobData data)
    {
        JobId = data.JobId;
        JobName = data.JobName;
        DocumentName = data.Document;
        DataType = data.DataType;
        Status = data.Status;
        Owner = data.Owner;
        PrintProcessor = data.PrintProcessor;
        PrinterName = data.PrinterName;
        StartTime = data.StartTime;
    }

    public void InitReferences(Document document, Printer printer)
    {
        
        Document = document;
        Printer = printer;
        FinishedInit?.Invoke(this);
    }

    public void UpdateStatus(PrintJobData data)
    {
        Status = data.Status;
    }

    public void Pause()
    {
        try
        {
            string query = $"SELECT * FROM Win32_PrintJob WHERE JobId = {JobId}";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printJob in searcher.Get())
            {
                object? result = printJob.InvokeMethod("Pause", null);
            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to pause print job {JobId}");
        }

    }
    
    public void Resume()
    {
        try
        {
            string query = $"SELECT * FROM Win32_PrintJob WHERE JobId = {JobId}";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printJob in searcher.Get())
            {
                object? result = printJob.InvokeMethod("Resume", null);
            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to resume print job {JobId}");
        }

    }

    public void Cancel()
    {
        try
        {
            string query = $"SELECT * FROM Win32_PrintJob WHERE JobId = {JobId}";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printJob in searcher.Get())
            {
                object? result = printJob.InvokeMethod("Cancel", null);
            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to cancel print job {JobId}");
        }

    }
    
}