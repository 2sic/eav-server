namespace ToSic.Lib.Memory;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog? log = default);
}