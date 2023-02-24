using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.New
{
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
    public interface IHasNewEntity
    {
        INewEntity NewEntity { get; }
    }
}
