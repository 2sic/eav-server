namespace ToSic.Sys.Memory;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]

public interface IOptimizeMemory
{
    public bool UseCompression { get; }
}
