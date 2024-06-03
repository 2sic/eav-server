using ToSic.Lib.Logging;

namespace ToSic.Lib.Data;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog log = default);
}