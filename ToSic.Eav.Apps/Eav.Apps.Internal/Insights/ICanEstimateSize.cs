namespace ToSic.Eav.Apps.Internal.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog log = default);
}