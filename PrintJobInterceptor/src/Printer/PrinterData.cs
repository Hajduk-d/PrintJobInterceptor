namespace PrintJobInterceptor;

public struct PrinterData
{
    public string Id { get; init; }
    public string ShareName { get; init; }
    public string PortName { get; init; }
    public string DriverName { get; init; }
    public string Location { get; init; }
    public string Comment { get; init; }
    
    public uint PrinterStatus { get; init; }
    public uint PrinterState { get; init; }
}