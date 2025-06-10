using System.Runtime.InteropServices;

namespace PrintJobInterceptor;

public class WinSpoolPrinterMonitor : IDisposable
{
    private bool _disposed;
    private CancellationTokenSource _cancellationTokenSource;
    private Task? _monitoringTask;
    private Dictionary<string, PrinterData> _lastKnownPrinters;

    internal event Action<Printer>? OnPrinterAdded;
    internal event Action<string>? OnPrinterRemoved;
    internal event Action<PrinterData>? OnPrinterStatusChanged;

    public WinSpoolPrinterMonitor()
    {
        _lastKnownPrinters = new Dictionary<string, PrinterData>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartPrinterMonitoring()
    {
        try
        {
            // Initial detection
            DetectPrinters();

            // Start monitoring task
            _monitoringTask = Task.Run(MonitorPrintersAsync, _cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to start printer monitoring");
        }
    }

    private async Task MonitorPrintersAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(2000, _cancellationTokenSource.Token);
                CheckForPrinterChanges();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                ServiceLogger.LogError(e, $"Error during printer monitoring");
            }
        }
    }

    private void CheckForPrinterChanges()
    {
        Dictionary<string, PrinterData> currentPrinters = GetAllPrinters();
        HashSet<string> currentPrinterNames = new (currentPrinters.Keys);
        HashSet<string> lastKnownNames = new(_lastKnownPrinters.Keys);

        // Check for new printers
        foreach (string printerName in currentPrinterNames.Except(lastKnownNames))
        {
            Printer printer = new Printer(currentPrinters[printerName]);
            OnPrinterAdded?.Invoke(printer);
        }

        // Check for removed printers
        foreach (string printerName in lastKnownNames.Except(currentPrinterNames))
        {
            OnPrinterRemoved?.Invoke(printerName);
        }

        // Check for modified printers
        foreach (string printerName in currentPrinterNames.Intersect(lastKnownNames))
        {
            PrinterData current = currentPrinters[printerName];
            PrinterData last = _lastKnownPrinters[printerName];

            if (!PrinterDataEquals(current, last))
            {
                OnPrinterStatusChanged?.Invoke(current);
            }
        }

        _lastKnownPrinters = currentPrinters;
    }

    private bool PrinterDataEquals(PrinterData a, PrinterData b)
    {
        return a.Id == b.Id &&
               a.ShareName == b.ShareName &&
               a.PortName == b.PortName &&
               a.DriverName == b.DriverName &&
               a.Location == b.Location &&
               a.Comment == b.Comment &&
               a.PrinterStatus == b.PrinterStatus &&
               a.PrinterState == b.PrinterState;
    }

    private void DetectPrinters()
    {
        _lastKnownPrinters = GetAllPrinters();
        
        foreach (PrinterData printerData in _lastKnownPrinters.Values)
        {
            Printer printer = new(printerData);
            OnPrinterAdded?.Invoke(printer);
        }
    }

    private Dictionary<string, PrinterData> GetAllPrinters()
    {
        Dictionary<string, PrinterData> printers = new();
        
        uint cbNeeded = 0;
        uint cReturned = 0;
        
        // First call to get the size needed
        WinSpoolApi.EnumPrinters(
            WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_LOCAL | WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS,
            null, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);

        if (cbNeeded == 0)
            return printers;

        IntPtr pPrinterEnum = Marshal.AllocHGlobal((int)cbNeeded);
        try
        {
            if (WinSpoolApi.EnumPrinters(
                WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_LOCAL | WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS,
                null, 2, pPrinterEnum, cbNeeded, ref cbNeeded, ref cReturned))
            {
                IntPtr currentPrinter = pPrinterEnum;
                int printerInfoSize = Marshal.SizeOf<WinSpoolApi.PRINTER_INFO_2>();

                for (int i = 0; i < cReturned; i++)
                {
                    WinSpoolApi.PRINTER_INFO_2 printerInfo = Marshal.PtrToStructure<WinSpoolApi.PRINTER_INFO_2>(currentPrinter);
                    PrinterData printerData = CreatePrinterDataFromWinSpool(printerInfo);
                    
                    if (!string.IsNullOrEmpty(printerData.Id))
                    {
                        printers[printerData.Id] = printerData;
                    }

                    currentPrinter = IntPtr.Add(currentPrinter, printerInfoSize);
                }
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(error, "Failed to enumerate printers");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pPrinterEnum);
        }

        return printers;
    }

    private PrinterData CreatePrinterDataFromWinSpool(WinSpoolApi.PRINTER_INFO_2 printerInfo)
    {
        try
        {
            return new PrinterData
            {
                Id = printerInfo.pPrinterName ?? string.Empty,
                ShareName = printerInfo.pShareName ?? string.Empty,
                PortName = printerInfo.pPortName ?? string.Empty,
                DriverName = printerInfo.pDriverName ?? string.Empty,
                Location = printerInfo.pLocation ?? string.Empty,
                Comment = printerInfo.pComment ?? string.Empty,
                PrinterStatus = printerInfo.Status,
                PrinterState = printerInfo.Status // WinSpool uses Status field for both
            };
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to extract printer data from WinSpool structure");
            throw;
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
            _cancellationTokenSource.Cancel();
            _monitoringTask?.Wait(5000);
            _cancellationTokenSource.Dispose();
            _monitoringTask?.Dispose();
        }

        _disposed = true;
    }
}
