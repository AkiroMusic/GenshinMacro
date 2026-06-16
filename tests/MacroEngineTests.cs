using System.Threading;
using GenshinMacro.Input;
using GenshinMacro.MacroEngine;
using GenshinMacro.ViewModels;
using Xunit;

namespace GenshinMacro.Tests;

public class MacroEngineTests
{
    [Fact]
    public void RotationWorker_StartStop_Should_Lifecycle()
    {
        var worker = new RotationWorker();
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();

        Assert.False(worker.IsRunning);
        worker.Start(btn, sim);
        Assert.True(worker.IsRunning);
        Assert.True(SpinWait.SpinUntil(() => worker.IsRunning, 500));
        worker.Stop();
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void DoubleClickWorker_StartStop_Should_Lifecycle()
    {
        var worker = new DoubleClickWorker();
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();

        Assert.False(worker.IsRunning);
        worker.Start(btn, sim);
        Assert.True(worker.IsRunning);
        Assert.True(SpinWait.SpinUntil(() => worker.IsRunning, 500));
        worker.Stop();
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void MacroCoordinator_Should_StartAndStopBoth()
    {
        var coord = new MacroCoordinator(new FakeInputSimulator(), new FakeButtonStateProvider());
        Assert.False(coord.AnyRunning);
        coord.StartAll();
        Assert.True(coord.AnyRunning);
        Assert.True(SpinWait.SpinUntil(() => coord.AnyRunning, 500));
        coord.StopAll();
        Assert.False(coord.AnyRunning);
    }

    [Fact]
    public void DoubleClickWorker_Should_EmitCorrectSequence()
    {
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();
        var worker = new DoubleClickWorker();

        btn.X2Pressed = true;
        worker.Start(btn, sim);
        Thread.Sleep(150);
        worker.Stop();
        btn.X2Pressed = false;

        var log = sim.CallLog;
        Assert.Contains(log, c => c == "LeftButtonDown");
        Assert.Contains(log, c => c == "RightButtonDown");
        Assert.Contains(log, c => c == "RightButtonUp");
        Assert.Contains(log, c => c == "LeftButtonUp");

        int ld = log.FindIndex(c => c == "LeftButtonDown");
        int rd = log.FindIndex(c => c == "RightButtonDown");
        int ru = log.FindIndex(c => c == "RightButtonUp");
        int lu = log.FindIndex(c => c == "LeftButtonUp");
        Assert.True(ld < rd, "LeftButtonDown should precede RightButtonDown");
        Assert.True(rd < ru, "RightButtonDown should precede RightButtonUp");
        Assert.True(ru < lu, "RightButtonUp should precede LeftButtonUp");
    }

    [Fact]
    public void RotationWorker_SendInputFailure_Should_StopWorker()
    {
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();
        sim.ReturnValue = false;
        sim.FailAfterCallCount = 0;
        btn.X1Pressed = true;

        var worker = new RotationWorker();
        worker.Start(btn, sim);

        var stopped = SpinWait.SpinUntil(() => !worker.IsRunning, 1000);
        worker.Stop();

        Assert.True(stopped, "Worker should have stopped due to SendInput failure (not via manual Stop)");
        Assert.NotNull(worker.LastError);
        Assert.Contains("输入模拟失败", worker.LastError);
    }

    [Fact]
    public void DoubleClickWorker_ShouldReleaseButtons_OnFailure()
    {
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();
        sim.FailAfterCallCount = 2;
        btn.X2Pressed = true;

        var worker = new DoubleClickWorker();
        worker.Start(btn, sim);
        Thread.Sleep(300);
        worker.Stop();
        btn.X2Pressed = false;

        var log = sim.CallLog;
        Assert.Contains(log, c => c == "LeftButtonDown");

        int ldIdx = log.FindIndex(c => c == "LeftButtonDown");
        int luIdx = log.FindLastIndex(c => c == "LeftButtonUp");
        Assert.True(luIdx >= 0, "Should have called LeftButtonUp to release held button");
    }

    [Fact]
    public void RotationWorker_ShouldSkipTick_WhenLockHeld()
    {
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();
        var worker = new RotationWorker();

        var holdEvent = new ManualResetEventSlim(false);
        var holdThread = new Thread(() =>
        {
            lock (InputLock.SyncRoot)
            {
                holdEvent.Set();
                Thread.Sleep(200);
            }
        });
        holdThread.Start();
        holdEvent.Wait();

        btn.X1Pressed = true;
        worker.Start(btn, sim);
        Thread.Sleep(100);
        Assert.Null(worker.LastError);
        Assert.True(worker.IsRunning, "Worker should still be running after lock contention");
        worker.Stop();
        btn.X1Pressed = false;

        holdThread.Join();
    }

    [Fact]
    public void MacroCoordinator_StartAll_ShouldBeIdempotent()
    {
        var coord = new MacroCoordinator(new FakeInputSimulator(), new FakeButtonStateProvider());
        string? error = null;
        coord.OnWorkerError += (msg) => error = msg;

        coord.StartAll();
        Assert.True(coord.AnyRunning);

        coord.StartAll();
        Assert.True(coord.AnyRunning);

        coord.StopAll();
        Assert.False(coord.AnyRunning);
    }

    [Fact]
    public void MainWindowViewModel_Should_AcceptFakeDependencies()
    {
        var sim = new FakeInputSimulator();
        var btn = new FakeButtonStateProvider();
        var vm = new MainWindowViewModel(sim, btn);

        Assert.False(vm.IsRunning);
        Assert.False(vm.ShowError);

        vm.ToggleCommand.Execute(null);
        Assert.True(vm.IsRunning);

        vm.ToggleCommand.Execute(null);
        Assert.False(vm.IsRunning);
    }

    [Fact]
    public void MainWindowViewModel_DefaultConstructor_Should_Work()
    {
        var vm = new MainWindowViewModel();
        Assert.NotNull(vm);
        Assert.False(vm.IsRunning);
    }
}
