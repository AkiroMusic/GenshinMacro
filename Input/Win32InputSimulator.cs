using System.Runtime.InteropServices;
using AkiMacro.Interop;

namespace AkiMacro.Input;

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
        return result != 0;
    }

    public bool MoveMouseBy(int deltaX, int deltaY)
    {
        return Send(CreateMouseInput(MouseEventFlags.MOUSEEVENTF_MOVE, deltaX, deltaY));
    }

    public bool RightClick()
    {
        var inputs = new INPUT[]
        {
            CreateMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTDOWN),
            CreateMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTUP)
        };
        var result = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        return result != 0;
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
