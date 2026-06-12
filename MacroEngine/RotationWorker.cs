using GenshinMacro.Input;

namespace GenshinMacro.MacroEngine;

public class RotationWorker : MacroWorkerBase
{
    // Python: ROTATION_SPEED = 0.02 (20ms poll interval)
    private const int PollIntervalMs = 20;

    // Python: pydirectinput.move(SCREEN_WIDTH, 0, duration=0.02, relative=True)
    // We need to replicate the smooth movement. pydirectinput's duration=0.02
    // means it spreads the movement over 20ms by sending sub-steps.
    // Replicate this by splitting into ~20 sub-steps of 96px each over 20ms,
    // totaling SCREEN_WIDTH relative movement to the right.
    private const int ScreenWidth = 1920;  // Will be configurable later
    private const int SubSteps = 20;       // Number of micro-moves per tick
    private const int PixelsPerSubStep = ScreenWidth / SubSteps; // 96 each
    private const int SubStepDelayMs = PollIntervalMs / SubSteps; // 1ms

    protected override void Run(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        while (!_cts.IsCancellationRequested)
        {
            if (buttonState.IsXButton1Pressed())
            {
                lock (InputLock.SyncRoot)
                {
                    for (int i = 0; i < SubSteps; i++)
                    {
                        if (_cts.IsCancellationRequested) return;
                        if (!inputSim.MoveMouseBy(PixelsPerSubStep, 0))
                        {
                            ReportError("旋转宏：输入模拟失败，请检查是否以管理员权限运行");
                            return;
                        }
                        Thread.Sleep(SubStepDelayMs);
                    }
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
}
