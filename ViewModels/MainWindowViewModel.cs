using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using AkiMacro.Input;
using AkiMacro.MacroEngine;

namespace AkiMacro.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly MacroCoordinator _coordinator;
    private bool _isRunning;
    private string _errorMessage = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning == value) return;
            _isRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public bool IsRotationRunning => _coordinator.IsRotationRunning;
    public bool IsDoubleClickRunning => _coordinator.IsDoubleClickRunning;
    public bool IsClickerRunning => _coordinator.IsClickerRunning;
    public bool AnyRunning => _coordinator.AnyRunning;

    public string StatusText => IsRunning ? "运行中" : "已停止";

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowError));
        }
    }

    public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);

    // Clicker settings
    public int ClickIntervalMs
    {
        get => _coordinator.Clicker.ClickIntervalMs;
        set
        {
            _coordinator.Clicker.ClickIntervalMs = value;
            OnPropertyChanged();
        }
    }

    public int MaxClicks
    {
        get => _coordinator.Clicker.MaxClicks;
        set
        {
            _coordinator.Clicker.MaxClicks = value;
            OnPropertyChanged();
        }
    }

    public bool ClickLeft
    {
        get => _coordinator.Clicker.ClickLeft;
        set
        {
            _coordinator.Clicker.ClickLeft = value;
            OnPropertyChanged();
        }
    }

    public bool ClickRight
    {
        get => _coordinator.Clicker.ClickRight;
        set
        {
            _coordinator.Clicker.ClickRight = value;
            OnPropertyChanged();
        }
    }

    public int ClicksPerformed => _coordinator.Clicker.ClicksPerformed;

    public ICommand ToggleCommand { get; }
    public ICommand DismissErrorCommand { get; }
    public ICommand ToggleClickerCommand { get; }

    public MainWindowViewModel()
        : this(null, null)
    {
    }

    public MainWindowViewModel(IInputSimulator? inputSim, IButtonStateProvider? buttonState)
    {
        inputSim ??= new Win32InputSimulator();
        buttonState ??= new Win32ButtonStateProvider();
        _coordinator = new MacroCoordinator(inputSim, buttonState);
        _coordinator.OnWorkerError += OnWorkerError;
        ToggleCommand = new RelayCommand(Toggle);
        DismissErrorCommand = new RelayCommand(() => ErrorMessage = "");
        ToggleClickerCommand = new RelayCommand(ToggleClicker);
    }

    private void OnWorkerError(string message)
    {
        ErrorMessage = message;
        IsRunning = false;
        OnPropertyChanged(nameof(IsRotationRunning));
        OnPropertyChanged(nameof(IsDoubleClickRunning));
        OnPropertyChanged(nameof(IsClickerRunning));
        OnPropertyChanged(nameof(AnyRunning));
    }

    public void Toggle()
    {
        if (IsRunning)
        {
            _coordinator.StopAll();
            IsRunning = false;
            OnPropertyChanged(nameof(IsRotationRunning));
            OnPropertyChanged(nameof(IsDoubleClickRunning));
            OnPropertyChanged(nameof(IsClickerRunning));
            OnPropertyChanged(nameof(AnyRunning));
            return;
        }

        ErrorMessage = "";
        _coordinator.StartAll();
        IsRunning = true;
        OnPropertyChanged(nameof(IsRotationRunning));
        OnPropertyChanged(nameof(IsDoubleClickRunning));
        OnPropertyChanged(nameof(IsClickerRunning));
        OnPropertyChanged(nameof(AnyRunning));
    }

    public void ToggleClicker()
    {
        if (_coordinator.IsClickerRunning)
        {
            _coordinator.Clicker.Stop();
        }
        else
        {
            _coordinator.Clicker.Start(
                new Win32ButtonStateProvider(), 
                new Win32InputSimulator());
        }
        OnPropertyChanged(nameof(IsClickerRunning));
        OnPropertyChanged(nameof(AnyRunning));
        OnPropertyChanged(nameof(ClicksPerformed));
    }

    public void Shutdown()
    {
        _coordinator.OnWorkerError -= OnWorkerError;

        if (_coordinator.AnyRunning)
            _coordinator.StopAll();

        IsRunning = false;
        OnPropertyChanged(nameof(IsRotationRunning));
        OnPropertyChanged(nameof(IsDoubleClickRunning));
        OnPropertyChanged(nameof(IsClickerRunning));
        OnPropertyChanged(nameof(AnyRunning));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
