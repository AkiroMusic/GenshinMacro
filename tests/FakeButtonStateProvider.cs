using GenshinMacro.Input;

namespace GenshinMacro.Tests;

public class FakeButtonStateProvider : IButtonStateProvider
{
    public bool X1Pressed { get; set; }
    public bool X2Pressed { get; set; }
    public bool IsXButton1Pressed() => X1Pressed;
    public bool IsXButton2Pressed() => X2Pressed;
}
