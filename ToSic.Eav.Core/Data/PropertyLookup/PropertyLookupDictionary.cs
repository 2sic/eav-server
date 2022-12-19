using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Generics;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.PropertyLookup
{
    /// <summary>
    /// Internal class to do a PropertyLookup using dictionary values.
    /// Probably just use for tests ATM
    /// </summary>
    [PrivateApi]
    public class PropertyLookupDictionary: IPropertyLookup, IHasIdentityNameId
    {
        public const string SourceTypeId = "Dictionary";
        public string NameId { get; }

        public PropertyLookupDictionary(string nameId, IDictionary<string, object> values)
        {
            NameId = nameId;
            Values = values.ToInvariant();
        }

        public IDictionary<string, object> Values { get; }

        public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        {
            path = path?.Add(SourceTypeId, NameId, specs.Field);
            if (Values.TryGetValue(specs.Field, out var result))
            {
                return new PropReqResult(result, path)
                {
                    Value = null,
                    Source = this,
                    FieldType = Attributes.FieldIsVirtual,  // I believe this would only be used for certain follow up work
                };
            }

            return PropReqResult.Null(path);
        }

        public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path) 
            => new List<PropertyDumpItem> { new PropertyDumpItem { Path = $"Not supported on {nameof(PropertyLookupDictionary)}" } };
    }
}
