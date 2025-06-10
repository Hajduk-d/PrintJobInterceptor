using NLog;
using NLog.Config;

namespace PrintJobInterceptor;

public static class ServiceLogger
{
    private static readonly Logger Logger;

    static ServiceLogger()
    {
        Logger = LogManager.GetCurrentClassLogger();
        LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
    }
    
    public static void LogError(string message) => Logger.Error(message);
    public static void LogError(Exception ex, string message) => Logger.Error(ex, message);
    public static void LogInfo(string message) => Logger.Info(message);
    public static void LogWarn(string message) => Logger.Warn(message);}