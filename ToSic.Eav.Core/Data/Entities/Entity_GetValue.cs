using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Plumbing;
using static ToSic.Eav.Data.Attributes;

namespace ToSic.Eav.Data;

partial class Entity
{

    // ReSharper disable once InheritdocInvalidUsage
    /// <inheritdoc />
    public object GetBestValue(string attributeName, string[] languages)
        => FindPropertyInternal(new(attributeName, languages, false), null).Result;


    // ReSharper disable once InheritdocInvalidUsage
    /// <inheritdoc />
    [Obsolete("Should not be used anymore, use Get<T> instead. planned to keep till ca. v20")]
    public TVal GetBestValue<TVal>(string name, string[] languages)
        => GetBestValue(name, languages).ConvertOrDefault<TVal>();


    [PrivateApi("Internal")]
    public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
    {
        path = path?.Add("Entity", EntityId.ToString(), specs.Field);
        // the languages are "safe" - meaning they are already all lower-cased and have the optional null-fallback key
        var languages = specs.Dimensions; // PropReqSpecs.ExtendDimsWithDefault(specs);
        var field = specs.Field.ToLowerInvariant();
        if (Attributes.ContainsKey(field))
        {
            var attribute = Attributes[field];
            var (valueField, result) = attribute.GetTypedValue(languages, false);
            return new(result: result, fieldType: attribute.Type.ToString(), path: path) { Value = valueField, Source = this };
        }
            
        if (field == EntityFieldTitle)
        {
            var attribute = Title;
            var valT = attribute?.GetTypedValue(languages, false);
            return new(result: valT?.Result, fieldType: attribute?.Type.ToString() ?? FieldIsNotFound, path: path) { Value = valT?.ValueField, Source = this };
        }

        // directly return internal properties, mark as virtual to not cause further Link resolution
        var valueFromInternalProperty = GetInternalPropertyByName(field);
        if (valueFromInternalProperty != null)
            return new(result: valueFromInternalProperty, fieldType: FieldIsVirtual, path: path) { Source = this };

        // New Feature in 12.03 - Sub-Item Navigation if the data contains information what the sub-entity identifiers are
        try
        {
            specs.LogOrNull.A("Nothing found in properties, will try Sub-Item navigation");
            var subItem = this.TryToNavigateToEntityInList(specs, this, path.Add("SubEntity", field));
            if (subItem != null) return subItem;
        } catch { /* ignore */ }

        return new(result: null, fieldType: FieldIsNotFound, path: path) { Source = this };
    }

    protected override object GetInternalPropertyByName(string attributeNameLowerInvariant)
    {
        // first check a field which doesn't exist on EntityLight
        if (attributeNameLowerInvariant == EntityFieldIsPublished) return IsPublished;

        // Now handle the ones that EntityLight has
        return base.GetInternalPropertyByName(attributeNameLowerInvariant);
    }
}