using System.Collections.Generic;
using GenshinMacro.Input;

namespace GenshinMacro.Tests;

public class FakeInputSimulator : IInputSimulator
{
    public List<string> CallLog { get; } = new();
    public bool ReturnValue { get; set; } = true;

    public bool MoveMouseBy(int deltaX, int deltaY)
    {
        CallLog.Add($"MoveMouseBy({deltaX},{deltaY})");
        return ReturnValue;
    }

    public bool RightClick()
    {
        CallLog.Add("RightClick");
        return ReturnValue;
    }

    public bool LeftButtonDown()
    {
        CallLog.Add("LeftButtonDown");
        return ReturnValue;
    }

    public bool LeftButtonUp()
    {
        CallLog.Add("LeftButtonUp");
        return ReturnValue;
    }

    public bool RightButtonDown()
    {
        CallLog.Add("RightButtonDown");
        return ReturnValue;
    }

    public bool RightButtonUp()
    {
        CallLog.Add("RightButtonUp");
        return ReturnValue;
    }

    public void Clear() => CallLog.Clear();
}
