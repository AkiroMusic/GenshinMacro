using System;
using System.Threading;
using AkiMacro.Input;

namespace AkiMacro.MacroEngine;

/// <summary>
/// 鼠标连点器 Worker
/// 基于 REF/鼠标连点器 配置：
/// - F9 (VK_F9=120) 开始
/// - F10 (VK_F10=121) 停止  
/// - F11 (VK_F11=122) 切换
/// - 默认频率 700ms，点击次数 100
/// </summary>
public class ClickerWorker : MacroWorkerBase
{
    private const int DefaultPollIntervalMs = 16; // ~60fps 检测按键
    
    // 配置参数
    private int _clickIntervalMs = 700;
    private int _maxClicks = 100; // 0 = 无限
    private bool _clickLeft = true;
    private bool _clickRight = false;
    
    // 热键虚拟键码
    private const int VK_F9 = 0x78;   // 开始
    private const int VK_F10 = 0x79;  // 停止
    private const int VK_F11 = 0x7A;  // 切换
    
    // 状态
    private volatile int _clicksPerformed = 0;
    private DateTime _lastClickTime = DateTime.MinValue;
    
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
    
    public int ClicksPerformed => _clicksPerformed;
    public bool IsLimitReached => _maxClicks > 0 && _clicksPerformed >= _maxClicks;

    protected override void Run(IButtonStateProvider buttonState, IInputSimulator inputSim)
    {
        _clicksPerformed = 0;
        _lastClickTime = DateTime.MinValue;
        
        while (!_cts.IsCancellationRequested)
        {
            // 检测热键
            bool startPressed = IsKeyPressed(VK_F9);
            bool stopPressed = IsKeyPressed(VK_F10);
            bool togglePressed = IsKeyPressed(VK_F11);
            
            if (startPressed)
            {
                // F9: 开始连点
                ReportError("连点器已启动 (F9)");
            }
            
            if (stopPressed)
            {
                // F10: 停止连点
                ReportError("连点器已停止 (F10)");
                return;
            }
            
            if (togglePressed)
            {
                // F11: 切换状态 - 这里简单处理为停止
                ReportError("连点器切换 (F11)");
                Thread.Sleep(300); // 防抖
            }
            
            // 执行连点逻辑（如果正在运行且未达到限制）
            if (IsRunning && !IsLimitReached)
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
                            }
                            else
                            {
                                ReportError("连点器：输入模拟失败");
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
            
            Thread.Sleep(DefaultPollIntervalMs);
        }
    }
    
    private static bool IsKeyPressed(int virtualKey)
    {
        // 使用 GetAsyncKeyState 检测按键状态（高位为1表示当前按下）
        short state = AkiMacro.Interop.NativeMethods.GetAsyncKeyState(virtualKey);
        return (state & 0x8000) != 0;
    }
    
    public new void Stop()
    {
        _clicksPerformed = 0;
        _lastClickTime = DateTime.MinValue;
        base.Stop();
    }
}