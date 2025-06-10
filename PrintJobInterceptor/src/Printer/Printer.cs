using System.ComponentModel;
using System.Diagnostics;
using System.Management;

namespace PrintJobInterceptor;

public partial class Printer : INotifyPropertyChanged
{
    public event Action<PrintJob> OnPrintJobAdded;
    
    public string Id { get; set; }
    public uint Status { get; set; }
    public uint State { get; set; }
    public string Location { get; set; }
    public string PortName { get; set; }
    public string DriverName { get; set; }

    public Dictionary<uint, PrintJob> PrintJobs = [];

    public Printer(PrinterData data)
    {
        Id = data.Id;
        DriverName = data.DriverName;
        PortName = data.PortName;
        Location = data.Location;
        Status = data.PrinterStatus;
        State = data.PrinterState;
    }
    
    public void UpdateStatus(PrinterData data)
    {
        Status = data.PrinterStatus;
        State = data.PrinterState;
    }

    public void RegisterPrintJob(PrintJob printJob)
    {
        printJob.FinishedInit += PrintJobOnFinishedInit;
    }

    private void PrintJobOnFinishedInit(PrintJob printJob)
    {
        PrintJobs.Add(printJob.JobId, printJob);
        OnPrintJobAdded?.Invoke(printJob);    
    }

    public void Pause()
    {
        try
        {
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{Id}'";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printer in searcher.Get())
            {
                object? result = printer.InvokeMethod("Pause", null);
            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to pause print jobs for printer {Id}");
        }

    }

    public void Resume()
    {
        try
        {
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{Id}'";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printer in searcher.Get())
            {
                object? result = printer.InvokeMethod("Resume", null);
                if (result is int returnCode && returnCode != 0)
                {
                    ServiceLogger.LogError($"PrintTestPage failed with error code: {returnCode}");
                }

            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to resume print job for printer {Id}");
        }

    }

    public void CancelAllJobs()
    {
        try
        {
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{Id}'";
            using ManagementObjectSearcher searcher = new(query);
            foreach (ManagementObject printer in searcher.Get())
            {
                object? result = printer.InvokeMethod("CancelAllJobs", null);
                if (result is int returnCode && returnCode != 0)
                {
                    ServiceLogger.LogError($"PrintTestPage failed with error code: {returnCode}");
                }

            }
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, $"Failed to cancel all print jobs for printer {Id}");
        }

    }

    
    public async Task PrintTestPage()
    {
        await Task.Run(() =>
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = "printui.dll,PrintUIEntry /k /n \"" + Id + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
        
            process.Start();
            process.WaitForExit();
        });

    }
}