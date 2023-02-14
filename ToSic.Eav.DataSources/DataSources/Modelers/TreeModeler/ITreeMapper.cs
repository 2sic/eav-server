using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public interface ITreeMapper
    {
        IImmutableList<IEntity> AddRelationships<TRel>(
            IEnumerable<IEntity> originals,
            string parentIdentifierAttribute,
            string childParentAttribute,
            string targetChildrenAttribute = default,
            string targetParentAttribute = default
        );
    }
}