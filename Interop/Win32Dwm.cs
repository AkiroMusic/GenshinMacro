using System.Runtime.InteropServices;

namespace GenshinMacro.Interop;

internal static class DwmApi
{
    private const string DwmDll = "dwmapi.dll";

    [DllImport(DwmDll, PreserveSig = false)]
    public static extern void DwmSetWindowAttribute(
        IntPtr hwnd,
        uint dwAttribute,
        ref int pvAttribute,
        uint cbAttribute);

    public const uint DWMWA_SYSTEMBACKDROP_TYPE = 38;
    public const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public const int DWMSBT_MAINWINDOW = 2;      // Mica
    public const int DWMSBT_TRANSIENTWINDOW = 3;  // Acrylic
    public const int DWMSBT_TABBEDWINDOW = 4;     // Tabbed Mica
}
