using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
    internal interface ITreeMapper
    {
        IImmutableList<IEntity> GetEntitiesWithRelationships(IEnumerable<IEntity> originals, string parentIdentifierAttribute, string childParentAttribute, string targetChildrenAttribute, string targetParentAttribute);
    }
}