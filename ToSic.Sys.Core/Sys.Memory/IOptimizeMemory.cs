namespace ToSic.Sys.Memory;

/// <summary>
/// Marks objects which can self-report if they want to use compression for memory optimization.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]

public interface IOptimizeMemory
{
    public bool UseCompression { get; }
}
