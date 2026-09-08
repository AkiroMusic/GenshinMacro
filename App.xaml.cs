global using System.Windows;
global using System.Diagnostics;
global using System.Security.Principal;
global using System.IO;

namespace AkiMacro;

public partial class App : Application
{
    private static readonly string LogFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AkiMacro", "debug.log");

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

        if (!IsAdministrator())
        {
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

        base.OnStartup(e);
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
