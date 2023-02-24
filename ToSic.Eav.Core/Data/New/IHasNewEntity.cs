using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Create
{
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
    public interface IHasNewEntity
    {
        INewEntity NewEntity { get; }
    }
}
