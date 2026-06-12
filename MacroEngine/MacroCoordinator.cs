using GenshinMacro.Input;

namespace GenshinMacro.MacroEngine;

public class MacroCoordinator
{
    public event Action<string>? OnWorkerError;

    private readonly RotationWorker _rotation = new();
    private readonly DoubleClickWorker _doubleClick = new();
    private readonly IInputSimulator _inputSim;
    private readonly IButtonStateProvider _buttonState;

    public MacroCoordinator(IInputSimulator inputSim, IButtonStateProvider buttonState)
    {
        _inputSim = inputSim;
        _buttonState = buttonState;
    }

    public bool IsRotationRunning => _rotation.IsRunning;
    public bool IsDoubleClickRunning => _doubleClick.IsRunning;
    public bool AnyRunning => _rotation.IsRunning || _doubleClick.IsRunning;

    public void StartAll()
    {
        _rotation.OnError += OnWorkerError;
        _doubleClick.OnError += OnWorkerError;
        _rotation.Start(_buttonState, _inputSim);
        _doubleClick.Start(_buttonState, _inputSim);
    }

    public void StopAll()
    {
        _rotation.OnError -= OnWorkerError;
        _doubleClick.OnError -= OnWorkerError;
        _rotation.Stop();
        _doubleClick.Stop();
    }
}
