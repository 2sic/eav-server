using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Data.Attributes;

namespace ToSic.Eav.Data;

partial class Entity
{

    // ReSharper disable once InheritdocInvalidUsage
    /// <inheritdoc />
    public object GetBestValue(string attributeName, string[] languages)
        => FindPropertyInternal(new PropReqSpecs(attributeName, languages), null).Result;


    // ReSharper disable once InheritdocInvalidUsage
    /// <inheritdoc />
    public TVal GetBestValue<TVal>(string name, string[] languages) => GetBestValue(name, languages).ConvertOrDefault<TVal>();


    [PrivateApi("Internal")]
    public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
    {
        path = path?.Add("Entity", EntityId.ToString(), specs.Field);
        var languages = ExtendDimsWithDefault(specs.Dimensions);
        var field = specs.Field.ToLowerInvariant();
        if (Attributes.ContainsKey(field))
        {
            var attribute = Attributes[field];
            var (valueField, result) = attribute.GetTypedValue(languages);
            return new PropReqResult(result: result, fieldType: attribute.Type.ToString(), path: path) { Value = valueField, Source = this };
        }
            
        if (field == EntityFieldTitle)
        {
            var attribute = Title;
            var valT = attribute?.GetTypedValue(languages);
            return new PropReqResult(result: valT?.Result, fieldType: attribute?.Type.ToString() ?? FieldIsNotFound, path: path) { Value = valT?.ValueField, Source = this };
        }

        // directly return internal properties, mark as virtual to not cause further Link resolution
        var valueFromInternalProperty = GetInternalPropertyByName(field);
        if (valueFromInternalProperty != null)
            return new PropReqResult(result: valueFromInternalProperty, fieldType: FieldIsVirtual, path: path) { Source = this };

        // New Feature in 12.03 - Sub-Item Navigation if the data contains information what the sub-entity identifiers are
        try
        {
            specs.LogOrNull.A("Nothing found in properties, will try Sub-Item navigation");
            var subItem = this.TryToNavigateToEntityInList(specs, this, path.Add("SubEntity", field));
            if (subItem != null) return subItem;
        } catch { /* ignore */ }

        return new PropReqResult(result: null, fieldType: FieldIsNotFound, path: path) { Source = this };
    }

    protected override object GetInternalPropertyByName(string attributeNameLowerInvariant)
    {
        // first check a field which doesn't exist on EntityLight
        if (attributeNameLowerInvariant == EntityFieldIsPublished) return IsPublished;

        // Now handle the ones that EntityLight has
        return base.GetInternalPropertyByName(attributeNameLowerInvariant);
    }

    /// <summary>
    /// Make sure the dimensions list also has a null-entry,
    /// for fallback to the first/only language (if any are provided and no match was made first)
    /// </summary>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public string[] ExtendDimsWithDefault(string[] dimensions)
    {
        // empty list - add the default dimension
        if (dimensions == null || dimensions.Length == 0) return new[] { null as string };

        // list already has a default at the end, don't change

        // we have dimensions but no default, add it
        if (dimensions.Last() == default) return dimensions;
        var newDims = dimensions.ToList();
        newDims.Add(default);
        return newDims.ToArray();
    }
}