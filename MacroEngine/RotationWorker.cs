using AkiMacro.Input;

namespace AkiMacro.MacroEngine;

public class RotationWorker : MacroWorkerBase
{
    private const int PollIntervalMs = 20;
    private const int ScreenWidth = 1920;
    private const int SubSteps = 20;
    private const int PixelsPerSubStep = ScreenWidth / SubSteps;
    private const int SubStepDelayMs = PollIntervalMs / SubSteps;

    protected override void Run(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        while (!_cts.IsCancellationRequested)
        {
            if (buttonState.IsXButton1Pressed() && 
                Monitor.TryEnter(InputLock.SyncRoot, TimeSpan.FromMilliseconds(10)))
            {
                try
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
                finally
                {
                    Monitor.Exit(InputLock.SyncRoot);
                }
            }
            Thread.Sleep(PollIntervalMs);
        }
    }
}
