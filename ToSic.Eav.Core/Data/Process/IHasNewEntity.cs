using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Process
{
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
    public interface IHasNewEntity<out T> where T: INewEntity
    {
        T NewEntity { get; }
    }
}
