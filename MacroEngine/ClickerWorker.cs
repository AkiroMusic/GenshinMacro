using AkiMacro.Input;
using AkiMacro.Interop;

namespace AkiMacro.MacroEngine;

/// <summary>
/// 鼠标连点器 Worker
/// 热键:
/// - F9 开始连点
/// - F10 停止连点
/// - F11 切换状态
/// - 默认频率 700ms，点击次数 100
/// </summary>
public class ClickerWorker : MacroWorkerBase
{
    private const int DefaultPollIntervalMs = 16; // ~60fps 检测按键
    private const int DebounceMs = 300; // 热键防抖

    // 配置参数
    private int _clickIntervalMs = 700;
    private int _maxClicks = 100; // 0 = 无限
    private bool _clickLeft = true;
    private bool _clickRight = false;

    // 热键虚拟键码
    private const int VK_F9 = 0x78;   // 开始
    private const int VK_F10 = 0x79;  // 停止
    private const int VK_F11 = 0x7A;  // 切换

    // 点击状态（独立于 Worker 线程生命周期）
    private volatile bool _isClicking;
    private volatile int _clicksPerformed;
    private DateTime _lastClickTime = DateTime.MinValue;

    /// <summary>
    /// 点击状态变化事件。参数为 true=开始点击, false=停止点击。
    /// </summary>
    public event Action<bool>? OnClickingStateChanged;

    /// <summary>
    /// 点击计数变化事件。
    /// </summary>
    public event Action<int>? OnClicksChanged;

    public int ClickIntervalMs
    {
        get => _clickIntervalMs;
        set => _clickIntervalMs = Math.Max(1, value);
    }

    public int MaxClicks
    {
        get => _maxClicks;
        set => _maxClicks = Math.Max(0, value);
    }

    public bool ClickLeft
    {
        get => _clickLeft;
        set => _clickLeft = value;
    }

    public bool ClickRight
    {
        get => _clickRight;
        set => _clickRight = value;
    }

    /// <summary>
    /// 是否正在执行连点（独立于 Worker 线程状态）。
    /// </summary>
    public bool IsClicking => _isClicking;

    public int ClicksPerformed => _clicksPerformed;
    public bool IsLimitReached => _maxClicks > 0 && _clicksPerformed >= _maxClicks;

    protected override void Run(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        _clicksPerformed = 0;
        _lastClickTime = DateTime.MinValue;

        bool prevF9 = false;
        bool prevF10 = false;
        bool prevF11 = false;

        while (!_cts.IsCancellationRequested)
        {
            // 边沿检测（按下瞬间触发，非持续）
            bool f9 = IsKeyPressed(VK_F9);
            bool f10 = IsKeyPressed(VK_F10);
            bool f11 = IsKeyPressed(VK_F11);

            // F9: 开始连点（仅在未点击时触发）
            if (f9 && !prevF9 && !_isClicking)
            {
                BeginClicking();
            }

            // F10: 停止连点
            if (f10 && !prevF10 && _isClicking)
            {
                StopClicking();
            }

            // F11: 切换状态
            if (f11 && !prevF11)
            {
                if (_isClicking)
                    StopClicking();
                else
                    BeginClicking();
                Thread.Sleep(DebounceMs);
            }

            prevF9 = f9;
            prevF10 = f10;
            prevF11 = f11;

            // 执行连点逻辑
            if (_isClicking && !IsLimitReached)
            {
                var now = DateTime.Now;
                var elapsed = (now - _lastClickTime).TotalMilliseconds;

                if (_lastClickTime == DateTime.MinValue || elapsed >= _clickIntervalMs)
                {
                    if (Monitor.TryEnter(InputLock.SyncRoot, TimeSpan.FromMilliseconds(5)))
                    {
                        try
                        {
                            bool success = true;

                            if (_clickLeft)
                                success &= inputSim.LeftButtonDown() && inputSim.LeftButtonUp();

                            if (_clickRight)
                                success &= inputSim.RightButtonDown() && inputSim.RightButtonUp();

                            if (success)
                            {
                                _clicksPerformed++;
                                _lastClickTime = now;
                                OnClicksChanged?.Invoke(_clicksPerformed);
                            }
                            else
                            {
                                StopClicking();
                                ReportError("连点器：输入模拟失败，请检查是否以管理员权限运行");
                                return;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(InputLock.SyncRoot);
                        }
                    }
                }
            }

            // 检查是否达到点击上限
            if (_isClicking && IsLimitReached)
            {
                StopClicking();
            }

            Thread.Sleep(DefaultPollIntervalMs);
        }
    }

    /// <summary>
    /// 开始连点。
    /// </summary>
    private void BeginClicking()
    {
        _clicksPerformed = 0;
        _lastClickTime = DateTime.MinValue;
        _isClicking = true;
        OnClickingStateChanged?.Invoke(true);
        OnClicksChanged?.Invoke(0);
    }

    /// <summary>
    /// 停止连点。
    /// </summary>
    private void StopClicking()
    {
        _isClicking = false;
        OnClickingStateChanged?.Invoke(false);
    }

    /// <summary>
    /// 手动启动连点（从 UI 按钮触发）。
    /// </summary>
    public void StartClicking()
    {
        if (_isClicking) return;
        BeginClicking();
    }

    /// <summary>
    /// 手动停止连点（从 UI 按钮触发）。
    /// </summary>
    public void StopClickingManually()
    {
        if (!_isClicking) return;
        StopClicking();
    }

    private static bool IsKeyPressed(int virtualKey)
    {
        short state = NativeMethods.GetAsyncKeyState(virtualKey);
        return (state & 0x8000) != 0;
    }

    public override void Stop()
    {
        _isClicking = false;
        _clicksPerformed = 0;
        _lastClickTime = DateTime.MinValue;
        base.Stop();
    }
}