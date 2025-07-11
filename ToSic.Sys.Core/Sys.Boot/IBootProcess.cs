﻿namespace ToSic.Sys.Boot;

/// <summary>
/// Defines a service (which must be added using AddTransient (not TryAddTransient).
/// Can then do more registrations at startup, like register features
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IBootProcess: IHasLog, IHasIdentityNameId
{
    BootPhase Phase { get; }

    int Priority { get; }

    void Run();
}