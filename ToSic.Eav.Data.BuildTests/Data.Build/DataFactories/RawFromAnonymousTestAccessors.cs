using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal static class RawFromAnonymousTestAccessors
{
    extension(RawFromAnonymousHelper helper)
    {
        public IRawEntity ConvertTac(object data)
            => helper.Convert(data);

        public (IDictionary<string, object?> values, IList<object> relationshipKeys)
            ExtractRelationshipKeysTac(int id, IDictionary<string, object?> dic)
            => helper.ExtractRelationshipKeys(id, dic);

        public IDictionary<string, RawRelationship> ExtractRelationshipsTac(IDictionary<string, object?> dic)
            => helper.ExtractRelationships(dic);

        public IDictionary<string, object?> StrongTypeRelationshipsTac(IDictionary<string, object?> dic)
            => helper.StrongTypeRelationships(dic);
        
    }
}
