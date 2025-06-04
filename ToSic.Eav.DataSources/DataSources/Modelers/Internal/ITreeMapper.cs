using ToSic.Eav.Data.Entities.Sys.Sources;

namespace ToSic.Eav.DataSources.Internal;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ITreeMapper
{
    IImmutableList<IEntity> AddParentChild(
        IEnumerable<IEntity> originals,
        string parentIdField,
        string childToParentRefField,
        string newChildrenField = default,
        string newParentField = default,
        LazyLookup<object, IEntity> lookup = default);

}