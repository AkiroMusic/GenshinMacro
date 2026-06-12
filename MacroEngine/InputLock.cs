namespace GenshinMacro.MacroEngine;

public static class InputLock
{
    public static readonly object SyncRoot = new();
}
