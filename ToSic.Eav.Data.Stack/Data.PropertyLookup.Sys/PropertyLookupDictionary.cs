using ToSic.Eav.Data.Debug;
using ToSic.Lib.Data;

namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Internal class to do a PropertyLookup using dictionary values.
/// Probably just use for tests ATM
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PropertyLookupDictionary(string nameId, IDictionary<string, object> values)
    : IPropertyLookup, IHasIdentityNameId
{
    public const string SourceTypeId = "Dictionary";
    public string NameId { get; } = nameId;

    public IDictionary<string, object> Values { get; } = values.ToInvariant();

    public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
    {
        path = path?.Add(SourceTypeId, NameId, specs.Field);
        return Values.TryGetValue(specs.Field, out var result)
            ? new(result: result /* I believe this would only be used for certain follow up work */, valueType: ValueTypesWithState.Dynamic, path: path)
            {
                Value = null,
                Source = this,
            }
            : PropReqResult.Null(path);
    }

    // #DropUseOfDumpProperties
    //public List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path) 
    //    => [new() { Path = $"Not supported on {nameof(PropertyLookupDictionary)}" }];
}