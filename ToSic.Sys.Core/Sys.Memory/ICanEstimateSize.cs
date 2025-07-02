namespace ToSic.Sys.Memory;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog? log = default);
}