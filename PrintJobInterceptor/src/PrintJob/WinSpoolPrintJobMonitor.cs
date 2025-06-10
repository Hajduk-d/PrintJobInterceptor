using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PrintJobInterceptor;

public class WinSpoolPrintJobMonitor : IDisposable
{
    private bool _disposed;
    private readonly Dictionary<string, IntPtr> _printerHandles;
    private readonly Dictionary<string, Dictionary<uint, PrintJobData>> _lastKnownJobs;
    private CancellationTokenSource _cancellationTokenSource;
    private Task? _monitoringTask;

    public event Action<PrintJob>? OnPrintJobAdded;
    public event Action<PrintJobData>? OnPrintJobModified;
    public event Action<uint>? OnPrintJobDeleted;

    public WinSpoolPrintJobMonitor()
    {
        _printerHandles = new Dictionary<string, IntPtr>();
        _lastKnownJobs = new Dictionary<string, Dictionary<uint, PrintJobData>>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartJobMonitoring()
    {
        try
        {
            InitializePrinterHandles();
            _monitoringTask = Task.Run(MonitorJobsAsync, _cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to start print job monitoring");
        }
    }

    private void InitializePrinterHandles()
    {
        List<string> printers = GetAllPrinterNames();
        
        foreach (string printerName in printers)
        {
            if (!WinSpoolApi.OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero)) continue;
            
            _printerHandles[printerName] = hPrinter;
            _lastKnownJobs[printerName] = new Dictionary<uint, PrintJobData>();
                
            DetectExistingJobs(printerName, hPrinter);
        }
    }

    private List<string> GetAllPrinterNames()
    {
        List<string> printerNames = new();
        uint cbNeeded = 0;
        uint cReturned = 0;

        WinSpoolApi.EnumPrinters(
            WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_LOCAL | WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS,
            null, 1, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);

        if (cbNeeded == 0) return printerNames;

        IntPtr pPrinterEnum = Marshal.AllocHGlobal((int)cbNeeded);
        try
        {
            if (WinSpoolApi.EnumPrinters(
                WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_LOCAL | WinSpoolApi.PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS,
                null, 1, pPrinterEnum, cbNeeded, ref cbNeeded, ref cReturned))
            {
                IntPtr current = pPrinterEnum;
                int size = Marshal.SizeOf<WinSpoolApi.PRINTER_INFO_1>();

                for (int i = 0; i < cReturned; i++)
                {
                    WinSpoolApi.PRINTER_INFO_1 info = Marshal.PtrToStructure<WinSpoolApi.PRINTER_INFO_1>(current);
                    if (!string.IsNullOrEmpty(info.pName))
                    {
                        printerNames.Add(info.pName);
                    }
                    current = IntPtr.Add(current, size);
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pPrinterEnum);
        }

        return printerNames;
    }

    private void DetectExistingJobs(string printerName, IntPtr hPrinter)
    {
        Dictionary<uint, PrintJobData> jobs = GetPrintJobs(hPrinter, printerName);
        foreach (PrintJobData job in jobs.Values)
        {
            _lastKnownJobs[printerName][job.JobId] = job;
            OnPrintJobAdded?.Invoke(new PrintJob(job));
        }
    }

    private async Task MonitorJobsAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token); // Poll every second
                CheckForJobChanges();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                ServiceLogger.LogError(e, "Error during print job monitoring");
            }
        }
    }

    private void CheckForJobChanges()
    {
        foreach (var printerName in _printerHandles.Keys.ToList())
        {
            IntPtr hPrinter = _printerHandles[printerName];
            Dictionary<uint, PrintJobData> currentJobs = GetPrintJobs(hPrinter, printerName);
            Dictionary<uint, PrintJobData> lastKnownJobs = _lastKnownJobs[printerName];

            // Check for new jobs
            foreach (uint jobId in currentJobs.Keys.Except(lastKnownJobs.Keys))
            {
                PrintJobData job = currentJobs[jobId];
                OnPrintJobAdded?.Invoke(new PrintJob(job));
            }

            // Check for deleted jobs
            foreach (uint jobId in lastKnownJobs.Keys.Except(currentJobs.Keys))
            {
                OnPrintJobDeleted?.Invoke(jobId);
            }

            // Check for modified jobs
            foreach (uint jobId in currentJobs.Keys.Intersect(lastKnownJobs.Keys))
            {
                PrintJobData current = currentJobs[jobId];
                PrintJobData last = lastKnownJobs[jobId];

                if (!JobDataEquals(current, last))
                {
                    OnPrintJobModified?.Invoke(current);
                }
            }

            _lastKnownJobs[printerName] = currentJobs;
        }
    }

    private bool JobDataEquals(PrintJobData a, PrintJobData b)
    {
        return a.JobId == b.JobId &&
               a.Status == b.Status &&
               a.Document == b.Document &&
               a.Owner == b.Owner &&
               a.DataType == b.DataType;
    }

    private Dictionary<uint, PrintJobData> GetPrintJobs(IntPtr hPrinter, string printerName)
    {
        Dictionary<uint, PrintJobData> jobs = new();
        uint cbNeeded = 0;
        uint cReturned = 0;

        WinSpoolJobApi.EnumJobs(hPrinter, 0, 1000, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);

        if (cbNeeded == 0) return jobs;

        IntPtr pJob = Marshal.AllocHGlobal((int)cbNeeded);
        try
        {
            if (WinSpoolJobApi.EnumJobs(hPrinter, 0, 1000, 2, pJob, cbNeeded, ref cbNeeded, ref cReturned))
            {
                IntPtr current = pJob;
                int jobInfoSize = Marshal.SizeOf<WinSpoolJobApi.JOB_INFO_2>();

                for (int i = 0; i < cReturned; i++)
                {
                    WinSpoolJobApi.JOB_INFO_2 jobInfo = Marshal.PtrToStructure<WinSpoolJobApi.JOB_INFO_2>(current);
                    PrintJobData jobData = CreatePrintJobDataFromWinSpool(jobInfo, printerName);
                    jobs[jobData.JobId] = jobData;
                    current = IntPtr.Add(current, jobInfoSize);
                }
            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed loading print jobs via winspool.drv");
        }
        finally
        {
            Marshal.FreeHGlobal(pJob);
        }

        return jobs;
    }

    private PrintJobData CreatePrintJobDataFromWinSpool(WinSpoolJobApi.JOB_INFO_2 jobInfo, string printerName)
    {
        return new PrintJobData
        {
            JobId = jobInfo.JobId,
            JobName = $"{printerName},{jobInfo.JobId}",
            Document = jobInfo.pDocument ?? string.Empty,
            DataType = jobInfo.pDatatype ?? string.Empty,
            Status = jobInfo.Status.ToString(),
            Owner = jobInfo.pUserName ?? string.Empty,
            PrintProcessor = jobInfo.pPrintProcessor ?? string.Empty,
            PrinterName = printerName,
            StartTime = jobInfo.Submitted.ToDateTime()
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
            _cancellationTokenSource.Cancel();
            _monitoringTask?.Wait(5000);

            foreach (IntPtr handle in _printerHandles.Values)
            {
                WinSpoolApi.ClosePrinter(handle);
            }

            _cancellationTokenSource.Dispose();
            _monitoringTask?.Dispose();
        }

        _disposed = true;
    }
}