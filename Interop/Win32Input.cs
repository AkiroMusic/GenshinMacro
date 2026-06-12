using System.Runtime.InteropServices;

namespace GenshinMacro.Interop;

internal static class NativeMethods
{
    private const string User32 = "user32.dll";

    [DllImport(User32, SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport(User32)]
    public static extern short GetAsyncKeyState(int vKey);
}
