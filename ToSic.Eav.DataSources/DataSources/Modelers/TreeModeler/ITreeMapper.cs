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
        IImmutableList<IEntity> AddRelationships<TKey>(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default
        );

        List<IEntity> AddSomeRelationshipsWIP<TKey>(
            string fieldName,
            List<(IEntity Entity, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup,
            bool cloneFirst = true
        );

        IList<NewEntitySet<TNewEntity>> AddOneRelationship<TNewEntity, TKey>(
            string fieldName,
            List<(NewEntitySet<TNewEntity> Set, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup,
            bool cloneFirst = true
        );
    }
}