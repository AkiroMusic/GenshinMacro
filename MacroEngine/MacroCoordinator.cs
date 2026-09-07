using AkiMacro.Input;

namespace AkiMacro.MacroEngine;

public class MacroCoordinator
{
    public event Action<string>? OnWorkerError;

    private readonly RotationWorker _rotation = new();
    private readonly DoubleClickWorker _doubleClick = new();
    private readonly ClickerWorker _clicker = new();
    private readonly IInputSimulator _inputSim;
    private readonly IButtonStateProvider _buttonState;
    private bool _started;

    public MacroCoordinator(IInputSimulator inputSim, IButtonStateProvider buttonState)
    {
        _inputSim = inputSim;
        _buttonState = buttonState;
    }

    public bool IsRotationRunning => _rotation.IsRunning;
    public bool IsDoubleClickRunning => _doubleClick.IsRunning;
    public bool IsClickerRunning => _clicker.IsRunning;
    public bool AnyRunning => _rotation.IsRunning || _doubleClick.IsRunning || _clicker.IsRunning;

    public ClickerWorker Clicker => _clicker;

    public void StartAll()
    {
        if (_started) return;
        _started = true;
        _rotation.OnError += OnWorkerError;
        _doubleClick.OnError += OnWorkerError;
        _clicker.OnError += OnWorkerError;
        _rotation.Start(_buttonState, _inputSim);
        _doubleClick.Start(_buttonState, _inputSim);
        _clicker.Start(_buttonState, _inputSim);
    }

    public void StopAll()
    {
        if (!_started) return;
        _started = false;
        _rotation.OnError -= OnWorkerError;
        _doubleClick.OnError -= OnWorkerError;
        _clicker.OnError -= OnWorkerError;
        _rotation.Stop();
        _doubleClick.Stop();
        _clicker.Stop();
    }
}
