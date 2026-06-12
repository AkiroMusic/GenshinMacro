using System.Runtime.InteropServices;
using GenshinMacro.Interop;

namespace GenshinMacro.Input;

public class Win32InputSimulator : IInputSimulator
{
    private static INPUT CreateMouseInput(MouseEventFlags flags, int dx = 0, int dy = 0)
    {
        return new INPUT
        {
            type = Interop.InputType.Mouse,
            union = new MouseKeyboardHardwareUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    dwFlags = (uint)flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                }
            }
        };
    }

    private bool Send(INPUT input)
    {
        var result = NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        if (result == 0)
        {
            var error = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine($"SendInput failed. Win32 error: {error}");
            return false;
        }
        return true;
    }

    public bool MoveMouseBy(int deltaX, int deltaY)
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_MOVE, deltaX, deltaY));
    }

    public bool RightClick()
    {
        if (!RightButtonDown()) return false;
        return RightButtonUp();
    }

    public bool LeftButtonDown()
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_LEFTDOWN));
    }

    public bool LeftButtonUp()
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_LEFTUP));
    }

    public bool RightButtonDown()
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTDOWN));
    }

    public bool RightButtonUp()
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTUP));
    }
}
