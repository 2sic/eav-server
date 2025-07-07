namespace ToSic.Sys.Memory;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog? log = default);
}