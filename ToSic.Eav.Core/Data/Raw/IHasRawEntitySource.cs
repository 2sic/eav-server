using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Raw
{
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
    public interface IHasRawEntitySource
    {
        IRawEntity Source { get; }
    }
}
