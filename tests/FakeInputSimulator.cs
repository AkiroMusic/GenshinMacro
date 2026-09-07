using System.Collections.Generic;
using AkiMacro.Input;

namespace AkiMacro.Tests;

public class FakeInputSimulator : IInputSimulator
{
    public List<string> CallLog { get; } = new();
    public bool ReturnValue { get; set; } = true;
    public int? FailAfterCallCount { get; set; }
    private int _callCount;

    private bool TrackAndReturn(string call)
    {
        CallLog.Add(call);
        _callCount++;
        if (FailAfterCallCount.HasValue && _callCount > FailAfterCallCount.Value)
            return false;
        return ReturnValue;
    }

    public bool MoveMouseBy(int deltaX, int deltaY)
    {
        return TrackAndReturn($"MoveMouseBy({deltaX},{deltaY})");
    }

    public bool RightClick()
    {
        return TrackAndReturn("RightClick");
    }

    public bool LeftButtonDown()
    {
        return TrackAndReturn("LeftButtonDown");
    }

    public bool LeftButtonUp()
    {
        return TrackAndReturn("LeftButtonUp");
    }

    public bool RightButtonDown()
    {
        return TrackAndReturn("RightButtonDown");
    }

    public bool RightButtonUp()
    {
        return TrackAndReturn("RightButtonUp");
    }

    public void Clear()
    {
        CallLog.Clear();
        _callCount = 0;
    }
}
