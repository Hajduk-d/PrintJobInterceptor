// See https://aka.ms/new-console-template for more information
using System.Management;
using PrintJobInterceptor;

Console.WriteLine("Hello, World!");


//@"C:\Windows\System32\winspool.drv"


PrinterService ps = new();
ps.StartService();


Console.WriteLine("Watching for print jobs");
Console.ReadKey();

