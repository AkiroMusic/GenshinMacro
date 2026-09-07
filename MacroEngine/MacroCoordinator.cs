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
    public bool IsClickerClicking => _clicker.IsClicking;
    public bool AnyRunning => _rotation.IsRunning || _doubleClick.IsRunning || _clicker.IsRunning;

    public ClickerWorker Clicker => _clicker;

    /// <summary>
    /// 点击状态变化事件（连点器开始/停止点击时触发）。
    /// </summary>
    public event Action<bool>? OnClickerClickingChanged;

    /// <summary>
    /// 点击计数变化事件。
    /// </summary>
    public event Action<int>? OnClickerClicksChanged;

    public void StartAll()
    {
        if (_started) return;
        _started = true;
        _rotation.OnError += OnWorkerError;
        _doubleClick.OnError += OnWorkerError;
        _clicker.OnError += OnWorkerError;
        _clicker.OnClickingStateChanged += OnClickerClickingChangedForward;
        _clicker.OnClicksChanged += OnClickerClicksChangedForward;
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
        _clicker.OnClickingStateChanged -= OnClickerClickingChangedForward;
        _clicker.OnClicksChanged -= OnClickerClicksChangedForward;
        _rotation.Stop();
        _doubleClick.Stop();
        _clicker.Stop();
    }

    /// <summary>
    /// 手动启动连点器点击（从 UI 按钮触发）。
    /// </summary>
    public void StartClickerClicking()
    {
        _clicker.StartClicking();
    }

    /// <summary>
    /// 手动停止连点器点击（从 UI 按钮触发）。
    /// </summary>
    public void StopClickerClicking()
    {
        _clicker.StopClickingManually();
    }

    private void OnClickerClickingChangedForward(bool isClicking)
    {
        OnClickerClickingChanged?.Invoke(isClicking);
    }

    private void OnClickerClicksChangedForward(int count)
    {
        OnClickerClicksChanged?.Invoke(count);
    }
}
