namespace SGPST.Infrastructure.Data;

public static class FileLogger
{
    private static readonly string LogDir;

    static FileLogger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        LogDir = Path.Combine(appData, "SGPST", "logs");
        if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);
    }

    public static void Log(string appName, string message)
    {
        try
        {
            var logFile = Path.Combine(LogDir, $"{appName}_{DateTime.Now:yyyyMMdd}.log");
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(logFile, logEntry);
            Console.WriteLine(message);
        }
        catch { /* Nao trava se o log falhar */ }
    }

    public static void LogError(string appName, string message, Exception? ex = null)
    {
        var fullMessage = $"[ERROR] {message}";
        if (ex != null)
        {
            fullMessage += $" | Exception: {ex.Message}";
            if (ex.InnerException != null) fullMessage += $" | Inner: {ex.InnerException.Message}";
            fullMessage += $" | StackTrace: {ex.StackTrace}";
        }
        Log(appName, fullMessage);
    }
}
