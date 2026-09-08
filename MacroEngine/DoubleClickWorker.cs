using AkiMacro.Input;

namespace AkiMacro.MacroEngine;

public class DoubleClickWorker : MacroWorkerBase
{
    private const int PollIntervalMs = 50;

    private const int LeftHoldMs = 200;
    private const int RightClickDelayMs = 140;
    private const int LeftUpDelayMs = 130;

    [Flags]
    private enum ButtonState
    {
        None = 0,
        Cycle1LeftDown = 1,
        Cycle1RightDown = 2,
        Cycle1RightUp = 4,
        Cycle1LeftUp = 8,
        Cycle2LeftDown = 16,
        Cycle2RightDown = 32,
        Cycle2RightUp = 64,
        Cycle2LeftUp = 128
    }

    protected override void Run(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        while (!_cts.IsCancellationRequested)
        {
            if (buttonState.IsXButton2Pressed())
            {
                lock (InputLock.SyncRoot)
                {
                    if (!ExecuteDoubleClickSequence(inputSim))
                        return;
                }
            }
            Thread.Sleep(PollIntervalMs);
        }
    }

    private bool ExecuteDoubleClickSequence(IInputSimulator inputSim)
    {
        if (_cts.IsCancellationRequested) return false;

        var state = ButtonState.None;

        // Cycle 1
        if (!inputSim.LeftButtonDown()) return Fail("LeftButtonDown", state, inputSim);
        state |= ButtonState.Cycle1LeftDown;
        Thread.Sleep(LeftHoldMs);
        if (!inputSim.RightButtonDown()) return Fail("RightButtonDown", state, inputSim);
        state |= ButtonState.Cycle1RightDown;
        if (!inputSim.RightButtonUp()) return Fail("RightButtonUp", state, inputSim);
        state |= ButtonState.Cycle1RightUp;
        Thread.Sleep(RightClickDelayMs);
        if (!inputSim.LeftButtonUp()) return Fail("LeftButtonUp", state, inputSim);
        state |= ButtonState.Cycle1LeftUp;
        Thread.Sleep(LeftUpDelayMs);

        // Cycle 2
        if (!inputSim.LeftButtonDown()) return Fail("LeftButtonDown", state, inputSim);
        state |= ButtonState.Cycle2LeftDown;
        Thread.Sleep(LeftHoldMs);
        if (!inputSim.RightButtonDown()) return Fail("RightButtonDown", state, inputSim);
        state |= ButtonState.Cycle2RightDown;
        if (!inputSim.RightButtonUp()) return Fail("RightButtonUp", state, inputSim);
        state |= ButtonState.Cycle2RightUp;
        Thread.Sleep(RightClickDelayMs);
        if (!inputSim.LeftButtonUp()) return Fail("LeftButtonUp", state, inputSim);
        state |= ButtonState.Cycle2LeftUp;
        Thread.Sleep(LeftUpDelayMs);

        return true;
    }

    private bool Fail(string action, ButtonState state, IInputSimulator inputSim)
    {
        if (state.HasFlag(ButtonState.Cycle2RightDown) && !state.HasFlag(ButtonState.Cycle2RightUp))
            inputSim.RightButtonUp();
        if (state.HasFlag(ButtonState.Cycle2LeftDown) && !state.HasFlag(ButtonState.Cycle2LeftUp))
            inputSim.LeftButtonUp();
        if (state.HasFlag(ButtonState.Cycle1RightDown) && !state.HasFlag(ButtonState.Cycle1RightUp))
            inputSim.RightButtonUp();
        if (state.HasFlag(ButtonState.Cycle1LeftDown) && !state.HasFlag(ButtonState.Cycle1LeftUp))
            inputSim.LeftButtonUp();

        ReportError($"双键宏：{action} 模拟失败，请检查是否以管理员权限运行");
        return false;
    }
}
