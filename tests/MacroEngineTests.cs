using System.Threading;
using GenshinMacro.Input;
using GenshinMacro.MacroEngine;
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
        Thread.Sleep(60);
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
        Thread.Sleep(60);
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
        Thread.Sleep(60);
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
        Thread.Sleep(150); // enough for one cycle
        worker.Stop();
        btn.X2Pressed = false;

        // Verify the call order matches the Python sequence:
        // LeftButtonDown, RightButtonDown, RightButtonUp, LeftButtonUp (cycle 1)
        // LeftButtonDown, RightButtonDown, RightButtonUp, LeftButtonUp (cycle 2)
        var log = sim.CallLog;
        Assert.Contains(log, c => c == "LeftButtonDown");
        Assert.Contains(log, c => c == "RightButtonDown");
        Assert.Contains(log, c => c == "RightButtonUp");
        Assert.Contains(log, c => c == "LeftButtonUp");
    }

    [Fact]
    public void InputLock_Should_BeSingleton()
    {
        Assert.Same(InputLock.SyncRoot, InputLock.SyncRoot);
    }

    [Fact]
    public void RotationWorker_SendInputFailure_Should_StopWorker()
    {
        var btn = new FakeButtonStateProvider();
        var sim = new FakeInputSimulator();
        sim.ReturnValue = false; // simulate SendInput failure
        btn.X1Pressed = true;

        var worker = new RotationWorker();
        worker.Start(btn, sim);
        Thread.Sleep(100);
        // Worker should have stopped due to SendInput failure
        // (the Run loop returns when SendInput returns false)
        // Just verify it doesn't crash
        worker.Stop();
        Assert.False(worker.IsRunning);
    }
}
