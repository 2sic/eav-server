using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Process
{
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
    public interface IHasRawEntity<out T> where T: IRawEntity
    {
        T RawEntity { get; }
    }
}
