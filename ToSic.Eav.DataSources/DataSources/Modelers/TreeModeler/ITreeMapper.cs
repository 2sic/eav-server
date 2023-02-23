using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
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

        IDictionary<TRaw, IEntity> AddOneRelationship<TRaw, TKey>(
            string fieldName,
            List<(TRaw Raw, IEntity Entity, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup,
            bool cloneFirst = true
        );
    }
}