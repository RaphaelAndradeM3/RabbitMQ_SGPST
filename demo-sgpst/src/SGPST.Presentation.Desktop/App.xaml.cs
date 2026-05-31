using Serilog;
using SGPST.Infrastructure.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        SerilogConfig.Configure("Desktop");
        Log.Information("Desktop App iniciando...");
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Desktop App encerrando...");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

