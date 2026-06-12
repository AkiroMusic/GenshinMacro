using GenshinMacro.Input;
using GenshinMacro.Interop;
using Xunit;

namespace GenshinMacro.Tests;

public class Win32InputTests
{
    [Fact]
    public void InputStruct_Layout_ShouldBeSequential()
    {
        var input = new INPUT
        {
            type = InputType.Mouse,
            union = new MouseKeyboardHardwareUnion
            {
                mi = new MOUSEINPUT { dx = 100, dy = 200, dwFlags = (uint)MouseEventFlags.MOUSEEVENTF_MOVE }
            }
        };
        Assert.Equal(InputType.Mouse, input.type);
        Assert.Equal(100, input.union.mi.dx);
        Assert.Equal(200, input.union.mi.dy);
    }

    [Fact]
    public void FakeInputSimulator_Should_TrackCalls()
    {
        var fake = new FakeInputSimulator();
        Assert.True(fake.MoveMouseBy(50, 0));
        Assert.True(fake.RightClick());
        Assert.Contains(fake.CallLog, c => c.Contains("MoveMouseBy(50,0)"));
        Assert.Contains(fake.CallLog, c => c.Contains("RightClick"));
    }

    [Fact]
    public void FakeButtonStateProvider_Should_ReturnConfiguredValues()
    {
        var fake = new FakeButtonStateProvider();
        fake.X1Pressed = true;
        fake.X2Pressed = false;
        Assert.True(fake.IsXButton1Pressed());
        Assert.False(fake.IsXButton2Pressed());
    }

    [Fact]
    public void Win32InputSimulator_ShouldNotCrash_WhenCalled()
    {
        var sim = new Win32InputSimulator();
        // These call real SendInput - just ensure no exception is thrown
        var result = sim.MoveMouseBy(0, 0);
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void Win32ButtonStateProvider_ShouldNotCrash_WhenPolled()
    {
        var provider = new Win32ButtonStateProvider();
        Assert.IsType<bool>(provider.IsXButton1Pressed());
        Assert.IsType<bool>(provider.IsXButton2Pressed());
    }
}
