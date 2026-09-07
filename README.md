# AkiMacro

通用鼠标宏工具 | 作者：Akiro | 版本 1.3.0

## 功能

| 触发键 | 功能 | 说明 |
|------|------|------|
| **X1** (鼠标侧键1) | 自动旋转 | 按住时鼠标平滑向右移动，20ms 轮询，20 个微步 |
| **X2** (鼠标侧键2) | 双键宏 | 左键按住 → 右键点按 → 松开，循环两次 |
| **F9 / F10 / F11** | 鼠标连点器 | F9启动 / F10停止 / F11切换，可配置间隔、次数、左右键 |

## 系统要求

- Windows 10/11（64 位）
- [.NET 10.0 运行时](https://dotnet.microsoft.com/download/dotnet/10.0)
- 管理员权限（程序会自动请求 UAC 提权）

## 下载

从 [Releases](https://github.com/AkiroMusic/AkiMacro/releases/latest) 下载最新版本

## 从源码构建

```bash
dotnet restore
dotnet build -c Release
dotnet test
dotnet publish AkiMacro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

也可直接运行 `run.bat` 快速构建并启动。

## 项目结构

```
AkiMacro.sln
├── AkiMacro.csproj          # WPF 项目配置
├── App.xaml / App.xaml.cs       # 应用入口 + UAC 自动提权
├── MainWindow.xaml / .cs        # 主界面（无边框、自定义标题栏）
├── AboutWindow.xaml / .cs       # 关于窗口
├── SettingsWindow.xaml / .cs    # 设置窗口（含连点器配置）
├── app.manifest                 # 管理员权限清单
├── ViewModels/
│   └── MainWindowViewModel.cs   # MVVM ViewModel
├── Interop/                     # Win32 P/Invoke
│   ├── Win32Input.cs            # SendInput / GetAsyncKeyState
│   └── Win32Structs.cs          # INPUT / MOUSEINPUT 结构体
├── Input/                       # 输入抽象层
│   ├── IInputSimulator.cs       # 模拟接口
│   ├── IButtonStateProvider.cs  # 按键状态接口
│   ├── Win32InputSimulator.cs   # Win32 实现
│   └── Win32ButtonStateProvider.cs
├── MacroEngine/                 # 宏引擎
│   ├── MacroWorkerBase.cs       # Worker 基类
│   ├── RotationWorker.cs        # 自动旋转宏
│   ├── DoubleClickWorker.cs     # 双键宏
│   ├── ClickerWorker.cs         # 鼠标连点器 (F9/F10/F11)
│   ├── MacroCoordinator.cs      # 协调器
│   └── InputLock.cs             # 全局输入锁
├── Styles/Theme.xaml            # 深色主题资源
├── tests/                       # xUnit 测试
├── app.ico                      # 应用图标
├── logo.png                     # UI 图标
└── run.bat                      # 一键构建启动
```

## 技术架构

| 模块 | 说明 |
|------|------|
| **Interop** | 封装 Win32 API |
| **Input** | 输入抽象层，便于单元测试 |
| **MacroEngine** | 宏执行引擎，管理 Worker 线程生命周期 |
| **ViewModels** | MVVM 模式，UI 与业务逻辑分离 |

**设计要点：**
- **自动提权**：启动时检测管理员权限，非管理员触发 UAC
- **线程安全**：`InputLock.SyncRoot` 全局锁确保同一时间只有一个 Worker 模拟输入
- **错误处理**：`SendInput` 失败时自动停止并上报错误
- **接口抽象**：`IInputSimulator` / `IButtonStateProvider` 支持依赖注入和测试
- **热键驱动**：连点器使用 F9/F10/F11 全局热键，无需鼠标侧键占用

## 免责声明

本工具仅供学习和娱乐使用，请勿用于任何违反游戏规则或法律法规的行为。

---

---

## 更新日志

### v1.3.0 (2026-09-07)
- 新增鼠标连点器功能 (基于 REF/鼠标连点器 逆向重写)
- F9 启动 / F10 停止 / F11 切换
- 支持配置点击间隔 (10-2000ms)、最大点击次数、左右键选择
- 设置窗口新增连点器配置面板
- 主界面新增连点器状态显示与独立开关

### v1.2.0 (2026-06-16)
- 项目重命名：GenshinMacro → AkiMacro
- 通用化功能描述，移除游戏特定术语

---

**作者：Akiro**
