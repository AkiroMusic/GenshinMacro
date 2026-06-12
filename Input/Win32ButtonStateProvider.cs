using GenshinMacro.Interop;

namespace GenshinMacro.Input;

public class Win32ButtonStateProvider : IButtonStateProvider
{
    private static bool IsKeyPressed(int virtualKey)
    {
        short state = NativeMethods.GetAsyncKeyState(virtualKey);
        return (state & 0x8000) != 0;
    }

    public bool IsXButton1Pressed() => IsKeyPressed((int)VirtualKey.XBUTTON1);
    public bool IsXButton2Pressed() => IsKeyPressed((int)VirtualKey.XBUTTON2);
}
