using GenshinMacro.Input;

namespace GenshinMacro.MacroEngine;

public class DoubleClickWorker : MacroWorkerBase
{
    // Python polling interval: 0.05 (50ms)
    private const int PollIntervalMs = 50;

    // Python timing (including pyautogui pause=0.1s):
    // pa.mouseDown()           → pause 0.1s
    // sleep(0.1)               → 0.1s
    // pa.click(button='right') → pause 0.1s
    // sleep(0.04)              → 0.04s
    // pa.mouseUp()             → pause 0.1s
    // sleep(0.03)              → 0.03s

    // Actual delays in ms:
    private const int LeftHoldMs = 200;      // 0.1s (pause) + 0.1s (sleep)
    private const int RightClickDelayMs = 140; // 0.1s (pause) + 0.04s (sleep)
    private const int LeftUpDelayMs = 130;   // 0.1s (pause) + 0.03s (sleep)

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
            try
            {
                Task.Delay(PollIntervalMs, _cts.Token).Wait(_cts.Token);
            }
            catch (OperationCanceledException) { break; }
            catch (AggregateException) { break; }
        }
    }

    private bool ExecuteDoubleClickSequence(IInputSimulator inputSim)
    {
        if (_cts.IsCancellationRequested) return false;

        // Cycle 1
        if (!inputSim.LeftButtonDown()) return Fail("LeftButtonDown");
        Thread.Sleep(LeftHoldMs);
        if (!inputSim.RightButtonDown()) return Fail("RightButtonDown");
        if (!inputSim.RightButtonUp()) return Fail("RightButtonUp");
        Thread.Sleep(RightClickDelayMs);
        if (!inputSim.LeftButtonUp()) return Fail("LeftButtonUp");
        Thread.Sleep(LeftUpDelayMs);

        // Cycle 2
        if (!inputSim.LeftButtonDown()) return Fail("LeftButtonDown");
        Thread.Sleep(LeftHoldMs);
        if (!inputSim.RightButtonDown()) return Fail("RightButtonDown");
        if (!inputSim.RightButtonUp()) return Fail("RightButtonUp");
        Thread.Sleep(RightClickDelayMs);
        if (!inputSim.LeftButtonUp()) return Fail("LeftButtonUp");
        Thread.Sleep(LeftUpDelayMs);

        return true;
    }

    private bool Fail(string action)
    {
        ReportError($"双马头宏：{action} 模拟失败，请检查是否以管理员权限运行");
        return false;
    }
}
