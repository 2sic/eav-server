namespace ToSic.Sys.Boot;

/// <summary>
/// Boot process base class, mainly to save a bit of code to create more bootloaders.
/// </summary>
/// <param name="logName"></param>
/// <param name="connect"></param>
/// <param name="bootPhase"></param>
/// <param name="priority"></param>
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