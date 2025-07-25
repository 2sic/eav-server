﻿namespace ToSic.Sys.Boot;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class BootProcessBase(string logName, object[]? connect = default, BootPhase bootPhase = BootPhase.Registrations, int priority = 999)
    : ServiceBase($"B8T.{logName}", connect: connect), IBootProcess
{
    public string NameId => Log.NameId;

    public BootPhase Phase => bootPhase;

    public int Priority => priority;

    /// <summary>
    /// Register Dnn features before loading
    /// </summary>
    public abstract void Run();
}