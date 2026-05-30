using Serilog;

namespace SGPST.Infrastructure.Data;

public static class SerilogConfig
{
    public static void Configure(string appName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logDir = Path.Combine(appData, "SGPST", "logs", appName);
        
        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logDir, "log-.txt"), 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Logging inicializado para {AppName}. Logs em {LogDir}", appName, logDir);
    }
}
