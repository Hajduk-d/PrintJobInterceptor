using System;
using System.Runtime.InteropServices;

namespace PrintJobInterceptor;

public static class WinSpoolJobApi
{
    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumJobs(IntPtr hPrinter, uint FirstJob, uint NoJobs, uint Level,
        IntPtr pJob, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindFirstPrinterChangeNotification(IntPtr hPrinter, uint fdwFlags,
        uint fdwOptions, IntPtr pPrinterNotifyOptions);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool FindNextPrinterChangeNotification(IntPtr hChange, ref uint pdwChange,
        IntPtr pPrinterNotifyOptions, out IntPtr ppPrinterNotifyInfo);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool FindClosePrinterChangeNotification(IntPtr hChange);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool FreePrinterNotifyInfo(IntPtr pPrinterNotifyInfo);

    public const uint JOB_STATUS_PAUSED = 0x00000001;
    public const uint JOB_STATUS_ERROR = 0x00000002;
    public const uint JOB_STATUS_DELETING = 0x00000004;
    public const uint JOB_STATUS_SPOOLING = 0x00000008;
    public const uint JOB_STATUS_PRINTING = 0x00000010;
    public const uint JOB_STATUS_OFFLINE = 0x00000020;
    public const uint JOB_STATUS_PAPEROUT = 0x00000040;
    public const uint JOB_STATUS_PRINTED = 0x00000080;
    public const uint JOB_STATUS_DELETED = 0x00000100;
    public const uint JOB_STATUS_BLOCKED_DEVQ = 0x00000200;
    public const uint JOB_STATUS_USER_INTERVENTION = 0x00000400;
    public const uint JOB_STATUS_RESTART = 0x00000800;

    public const uint PRINTER_CHANGE_ADD_JOB = 0x00000100;
    public const uint PRINTER_CHANGE_SET_JOB = 0x00000200;
    public const uint PRINTER_CHANGE_DELETE_JOB = 0x00000400;
    public const uint PRINTER_CHANGE_WRITE_JOB = 0x00000800;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct JOB_INFO_2
    {
        public uint JobId;
        public string pPrinterName;
        public string pMachineName;
        public string pUserName;
        public string pDocument;
        public string pNotifyName;
        public string pDatatype;
        public string pPrintProcessor;
        public string pParameters;
        public string pDriverName;
        public IntPtr pDevMode;
        public string pStatus;
        public IntPtr pSecurityDescriptor;
        public uint Status;
        public uint Priority;
        public uint Position;
        public uint StartTime;
        public uint UntilTime;
        public uint TotalPages;
        public uint Size;
        public SYSTEMTIME Submitted;
        public uint Time;
        public uint PagesPrinted;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;

        public DateTime ToDateTime()
        {
            try
            {
                return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_NOTIFY_OPTIONS
    {
        public uint Version;
        public uint Flags;
        public uint Count;
        public IntPtr pTypes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_NOTIFY_OPTIONS_TYPE
    {
        public ushort Type;
        public ushort Reserved0;
        public uint Reserved1;
        public uint Reserved2;
        public uint Count;
        public IntPtr pFields;
    }

    public const ushort PRINTER_NOTIFY_TYPE_JOB = 0x01;
    public const uint PRINTER_NOTIFY_FIELD_JOB_ID = 0x00;
    public const uint PRINTER_NOTIFY_FIELD_STATUS = 0x12;
}