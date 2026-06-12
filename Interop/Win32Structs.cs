using System.Runtime.InteropServices;

namespace GenshinMacro.Interop;

public enum InputType : uint
{
    Mouse = 0,
    Keyboard = 1,
    Hardware = 2
}

[Flags]
public enum MouseEventFlags : uint
{
    MOUSEEVENTF_MOVE = 0x0001,
    MOUSEEVENTF_LEFTDOWN = 0x0002,
    MOUSEEVENTF_LEFTUP = 0x0004,
    MOUSEEVENTF_RIGHTDOWN = 0x0008,
    MOUSEEVENTF_RIGHTUP = 0x0010,
    MOUSEEVENTF_MIDDLEDOWN = 0x0020,
    MOUSEEVENTF_MIDDLEUP = 0x0040,
    MOUSEEVENTF_XDOWN = 0x0080,
    MOUSEEVENTF_XUP = 0x0100,
    MOUSEEVENTF_WHEEL = 0x0800,
    MOUSEEVENTF_HWHEEL = 0x1000,
    MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000,
    MOUSEEVENTF_VIRTUALDESK = 0x4000,
    MOUSEEVENTF_ABSOLUTE = 0x8000,
}

[StructLayout(LayoutKind.Sequential)]
public struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct KEYBDINPUT
{
    public ushort wVk;
    public ushort wScan;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Explicit)]
public struct MouseKeyboardHardwareUnion
{
    [FieldOffset(0)]
    public MOUSEINPUT mi;

    [FieldOffset(0)]
    public KEYBDINPUT ki;
}

[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public InputType type;
    public MouseKeyboardHardwareUnion union;
}

public enum VirtualKey : int
{
    XBUTTON1 = 0x05,
    XBUTTON2 = 0x06,
}
