using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GenshinMacro.Input;
using GenshinMacro.MacroEngine;

namespace GenshinMacro.ViewModels;

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

    public ICommand ToggleCommand { get; }
    public ICommand DismissErrorCommand { get; }

    public MainWindowViewModel()
    {
        var inputSim = new Win32InputSimulator();
        var buttonState = new Win32ButtonStateProvider();
        _coordinator = new MacroCoordinator(inputSim, buttonState);
        _coordinator.OnWorkerError += OnWorkerError;
        ToggleCommand = new RelayCommand(Toggle);
        DismissErrorCommand = new RelayCommand(() => ErrorMessage = "");
    }

    private void OnWorkerError(string message)
    {
        ErrorMessage = message;
        IsRunning = false;
    }

    public void Toggle()
    {
        if (IsRunning)
        {
            _coordinator.StopAll();
            IsRunning = false;
            return;
        }

        ErrorMessage = "";
        _coordinator.StartAll();
        IsRunning = true;
    }

    public void Shutdown()
    {
        _coordinator.OnWorkerError -= OnWorkerError;

        if (_coordinator.AnyRunning)
            _coordinator.StopAll();

        IsRunning = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
