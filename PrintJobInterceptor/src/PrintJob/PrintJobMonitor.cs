using System.Management;

namespace PrintJobInterceptor;

public class PrintJobMonitor : IDisposable
{
    private bool _disposed;
    
    private ManagementEventWatcher _jobAddedWatcher = null!;
    private ManagementEventWatcher _jobModifiedWatcher = null!;
    private ManagementEventWatcher _jobDeletedWatcher = null!;

    public event Action<PrintJob>? OnPrintJobAdded;
    public event Action<PrintJobData>? OnPrintJobModified;
    public event Action<uint>? OnPrintJobDeleted;
    
    public void StartJobMonitoring()
    {
        try
        {
            InitAddWatcher();
            InitModifiedWatcher();
            InitDeleteWatcher();
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to start print job event watchers");
        }
    }

    private void InitDeleteWatcher()
    {
        WqlEventQuery jobDeletedQuery = new (
            $"SELECT * FROM __InstanceDeletionEvent WITHIN 1 " +
            $"WHERE TargetInstance ISA 'Win32_PrintJob'"
        );
        
        _jobDeletedWatcher = new ManagementEventWatcher(jobDeletedQuery);

        _jobDeletedWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            OnPrintJobDeleted?.Invoke(Convert.ToUInt32(targetInstance["JobId"]));
        };
       
        _jobDeletedWatcher.Start();
    }

    private void InitModifiedWatcher()
    {
        WqlEventQuery jobModifiedQuery = new (
            "SELECT * FROM __InstanceModificationEvent WITHIN 1 " +
            "WHERE TargetInstance ISA 'Win32_PrintJob'"
        );
        
        _jobModifiedWatcher = new ManagementEventWatcher(jobModifiedQuery);

        _jobModifiedWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            PrintJobData data = GetPrintJobDataFromWMI(targetInstance);
            OnPrintJobModified?.Invoke(data);
        };
        
        _jobModifiedWatcher.Start();
    }

    private void InitAddWatcher()
    {
        WqlEventQuery jobAddedQuery = new (
            "SELECT * FROM __InstanceCreationEvent WITHIN 1 " +
            "WHERE TargetInstance ISA 'Win32_PrintJob'"
        );    
        
        _jobAddedWatcher = new ManagementEventWatcher(jobAddedQuery);

        _jobAddedWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            
            PrintJob printJob = new(GetPrintJobDataFromWMI(targetInstance));
            
            OnPrintJobAdded?.Invoke(printJob);
        };
        
        _jobAddedWatcher.Start();
    }

    private PrintJobData GetPrintJobDataFromWMI(ManagementBaseObject printJobWMI)
    {
        DateTime startTime = ManagementDateTimeConverter.ToDateTime(printJobWMI["TimeSubmitted"]?.ToString());
        return new PrintJobData
        {
            JobId = Convert.ToUInt32(printJobWMI["JobId"] ?? 0),
            JobName = printJobWMI["Name"]?.ToString() ?? string.Empty,
            Document = printJobWMI["Document"]?.ToString() ?? string.Empty,
            DataType = printJobWMI["DataType"]?.ToString() ?? string.Empty,
            Status = printJobWMI["Status"]?.ToString() ?? string.Empty,
            Owner = printJobWMI["Owner"]?.ToString() ?? string.Empty,
            PrintProcessor = printJobWMI["PrintProcessor"]?.ToString() ?? string.Empty,
            PrinterName = printJobWMI["Name"]?.ToString()?.Split(',').FirstOrDefault() ?? string.Empty,
            StartTime = startTime
        };
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _jobAddedWatcher?.Dispose();
            _jobDeletedWatcher?.Dispose();
            _jobModifiedWatcher?.Dispose();
        }
        
        _disposed = true;
    }
}