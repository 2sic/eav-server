﻿using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Generics;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Internal class to do a PropertyLookup using dictionary values.
/// Probably just use for tests ATM
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
            ? new(result: result, fieldType: Attributes.FieldIsDynamic /* I believe this would only be used for certain follow up work */, path: path)
            {
                Value = null,
                Source = this,
            }
            : PropReqResult.Null(path);
    }

    public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path) 
        => new() { new() { Path = $"Not supported on {nameof(PropertyLookupDictionary)}" } };
}