using ToSic.Lib.Logging;

namespace ToSic.Lib.Memory;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanEstimateSize
{
    public SizeEstimate EstimateSize(ILog log = default);
}