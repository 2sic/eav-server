using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Generics;
using ToSic.Lib.Data;
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
            return Values.TryGetValue(specs.Field, out var result)
                ? new PropReqResult(result: result, fieldType: Attributes.FieldIsDynamic /* I believe this would only be used for certain follow up work */, path: path)
                {
                    Value = null,
                    Source = this,
                }
                : PropReqResult.Null(path);
        }

        public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path) 
            => new List<PropertyDumpItem> { new PropertyDumpItem { Path = $"Not supported on {nameof(PropertyLookupDictionary)}" } };
    }
}
