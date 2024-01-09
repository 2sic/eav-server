using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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