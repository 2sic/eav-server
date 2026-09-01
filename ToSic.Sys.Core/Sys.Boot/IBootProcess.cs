namespace ToSic.Sys.Boot;

/// <summary>
/// Defines a service, which must be added using `AddTransient` (not `TryAddTransient`).
/// 
/// Such a boot process can then do more work at startup, like register features.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IBootProcess: IHasLog, IHasIdentityNameId
{
    /// <summary>
    /// The phase during which to run this.
    /// </summary>
    BootPhase Phase { get; }

    /// <summary>
    /// The priority within the phase. Lower numbers run first. Default is usually 999.
    /// </summary>
    int Priority { get; }

    void Run();
}