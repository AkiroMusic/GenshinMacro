using GenshinMacro.Input;

namespace GenshinMacro.MacroEngine;

public abstract class MacroWorkerBase
{
    public event Action<string>? OnError;

    protected readonly CancellationTokenSource _cts = new();
    protected Thread? _thread;
    protected bool _running;

    public bool IsRunning => _running;
    public string? LastError { get; protected set; }

    public void Start(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        if (_running) return;
        _running = true;
        _thread = new Thread(() => Run(buttonState, inputSim))
        {
            IsBackground = true,
            Name = GetType().Name
        };
        _thread.Start();
    }

    protected void ReportError(string message)
    {
        LastError = message;
        OnError?.Invoke(message);
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _cts.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(2));
        _thread = null;
    }

    protected abstract void Run(IButtonStateProvider buttonState, IInputSimulator inputSim);
}
