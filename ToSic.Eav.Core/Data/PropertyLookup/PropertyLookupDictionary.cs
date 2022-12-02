using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Documentation;
using ToSic.Eav.Generics;
using ToSic.Lib.Logging;

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
        public PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull, PropertyLookupPath path)
        {
            path = path?.Add(SourceTypeId, NameId, field);
            if (Values.TryGetValue(field, out var result))
            {
                return new PropertyRequest(result, path)
                {
                    Value = null,
                    //Result = result,
                    //Path = path,
                    Source = this,
                    FieldType = Attributes.FieldIsVirtual,  // I believe this would only be used for certain follow up work
                };
            }

            return new PropertyRequest(null, path); // {Path = path};
        }

        public List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull)
        {
            throw new System.NotImplementedException();
        }
    }
}
