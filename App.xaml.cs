global using System.Windows;
global using System.Diagnostics;
global using System.Security.Principal;
global using System.IO;

namespace GenshinMacro;

public partial class App : Application
{
    private static readonly string LogFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GenshinMacro", "debug.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] FATAL: {args.ExceptionObject}\n");
        };
        DispatcherUnhandledException += (_, args) =>
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] UI ERROR: {args.Exception}\n");
            args.Handled = true;
        };

        File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] Start. Admin={IsAdministrator()}\n");

        if (!IsAdministrator())
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] Requesting elevation...\n");
            var proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Environment.ProcessPath,
                Verb = "runas",
                Arguments = string.Join(" ", e.Args)
            };
            try
            {
                Process.Start(proc);
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] Elevation failed: {ex.Message}\n");
            }
            Environment.Exit(0);
            return;
        }

        File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] Running as admin, starting UI...\n");
        base.OnStartup(e);
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
