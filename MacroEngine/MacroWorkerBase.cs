using AkiMacro.Input;

namespace AkiMacro.MacroEngine;

public abstract class MacroWorkerBase : IDisposable
{
    public event Action<string>? OnError;

    protected CancellationTokenSource _cts = new();
    protected Thread? _thread;
    protected volatile bool _running;

    public bool IsRunning => _running;
    public string? LastError { get; protected set; }

    public void Start(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        if (_running) return;

        // Recreate CTS if already cancelled (enables restart after Stop)
        if (_cts.IsCancellationRequested)
        {
            _cts = new CancellationTokenSource();
        }

        _running = true;
        _thread = new Thread(() =>
        {
            try
            {
                Run(buttonState, inputSim);
            }
            finally
            {
                _running = false;
            }
        })
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

    public virtual void Stop()
    {
        if (!_running) return;
        _running = false;
        _cts.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(2));
        _thread = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_running) Stop();
        _cts.Dispose();
    }

    ~MacroWorkerBase() => Dispose(false);

    protected abstract void Run(IButtonStateProvider buttonState, IInputSimulator inputSim);
}
