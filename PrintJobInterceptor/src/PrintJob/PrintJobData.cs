namespace PrintJobInterceptor;

public struct PrintJobData
{
    public uint JobId { get; set; }
    public string JobName { get; set; }
    public string Document { get; set; }
    public string DataType { get; set; }
    public string Status { get; set; }
    public string Owner { get; set; }
    public string PrintProcessor { get; set; }
    public string PrinterName { get; set; }
    public DateTime StartTime { get; set; }
}