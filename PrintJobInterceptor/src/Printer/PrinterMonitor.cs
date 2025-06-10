using System.Management;

namespace PrintJobInterceptor;

public class PrinterMonitor : IDisposable
{
    private bool _disposed;

    private ManagementEventWatcher _addWatcher;
    private ManagementEventWatcher _deleteWatcher;
    private ManagementEventWatcher _modifiedWatcher;

    internal event Action<Printer> OnPrinterAdded;
    internal event Action<string> OnPrinterRemoved;
    internal event Action<PrinterData> OnPrinterStatusChanged;
    
    public void StartPrinterMonitoring()
    {
        try
        {
            DetectPrinters();

            InitAddWatcher();
            InitModifiedWatcher();
            InitDeleteWatcher();
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to start printer event watchers");
        }

    }
    
    private void InitModifiedWatcher()
    {
        WqlEventQuery statusQuery = new WqlEventQuery(
            $"SELECT * FROM __InstanceModificationEvent WITHIN 2 " +
            $"WHERE TargetInstance ISA 'Win32_Printer' "
        );

        _modifiedWatcher = new ManagementEventWatcher(statusQuery);
        _modifiedWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            PrinterData newPrinterData = GetPrinterDataFromWMI(targetInstance);
            OnPrinterStatusChanged?.Invoke(newPrinterData);
        };
        _modifiedWatcher.Start();
    }

    private void InitDeleteWatcher()
    {
        WqlEventQuery printerRemovedQuery = new(
            "SELECT * FROM __InstanceDeletionEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_Printer'"
        );

        _deleteWatcher = new ManagementEventWatcher(printerRemovedQuery);
        
        _deleteWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string? printerName = targetInstance["Name"]?.ToString();

            if (printerName is null) return;

            OnPrinterRemoved?.Invoke(printerName);
            
        };
        
        _deleteWatcher.Start();    }

    private void InitAddWatcher()
    {
        WqlEventQuery printerAddedQuery = new(
            "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_Printer'"
        );
        
        _addWatcher = new ManagementEventWatcher(printerAddedQuery);

        _addWatcher.EventArrived += (sender, e) =>
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            Printer? newPrinter = CreateNewPrinter(new ManagementObject(targetInstance.ClassPath));

            OnPrinterAdded?.Invoke(newPrinter);
        };
        
        _addWatcher.Start();
    }

    private void DetectPrinters()
    {
        SelectQuery query = new("Win32_Printer");
        using ManagementObjectSearcher searcher = new(query);
        using ManagementObjectCollection printerCollection = searcher.Get();
        foreach (ManagementObject printerWMI in printerCollection)
        {
            Printer printer = CreateNewPrinter(printerWMI);
            OnPrinterAdded?.Invoke(printer);
        }
    }

    private Printer CreateNewPrinter(ManagementObject printerWMI)
    {
        PrinterData newPrinterData = GetPrinterDataFromWMI(printerWMI);
        
        return new Printer(newPrinterData);
    }

    private PrinterData GetPrinterDataFromWMI(ManagementBaseObject printerWMI)
    {
        try
        {
            return new PrinterData
            {
                Id = printerWMI["Name"]?.ToString() ?? string.Empty,
                ShareName = printerWMI["ShareName"]?.ToString() ?? string.Empty,
                PortName = printerWMI["PortName"]?.ToString() ?? string.Empty,
                DriverName = printerWMI["DriverName"]?.ToString() ?? string.Empty,
                Location = printerWMI["Location"]?.ToString() ?? string.Empty,
                Comment = printerWMI["Comment"]?.ToString() ?? string.Empty,
                PrinterStatus = Convert.ToUInt32(printerWMI["PrinterStatus"] ?? 0),
                PrinterState = Convert.ToUInt32(printerWMI["PrinterState"] ?? 0)
            };
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to extract printer data from received WMI object");
            return new PrinterData();
        }
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
            _addWatcher?.Dispose();
            _deleteWatcher?.Dispose();
            _modifiedWatcher?.Dispose();
        }
        
        _disposed = true;
    }
}