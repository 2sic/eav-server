using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.New;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public interface ITreeMapper
    {
        IImmutableList<IEntity> AddParentChild<TKey>(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default
        );

        IList<EntityPair<TNewEntity>> AddOneRelationship<TNewEntity, TKey>(
            string fieldName,
            List<(EntityPair<TNewEntity> Set, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup
        );
    }
}