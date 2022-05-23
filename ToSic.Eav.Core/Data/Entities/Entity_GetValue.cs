using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages)
            => _useLightModel
                ? base.GetBestValue(attributeName)
                : FindPropertyInternal(attributeName, languages, null, null).Result;


        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages) => GetBestValue(name, languages).ConvertOrDefault<TVal>();


        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull, PropertyLookupPath path)
        {
            path = path?.Add("Entity", EntityId.ToString(), field);
            languages = ExtendDimsWithDefault(languages);
            field = field.ToLowerInvariant();
            if (Attributes.ContainsKey(field))
            {
                var attribute = Attributes[field];
                var valT = attribute.GetTypedValue(languages);
                return new PropertyRequest { Value = valT.Item1, Result = valT.Item2, FieldType = attribute.Type, Source = this, Path = path};
            }
            
            if (field == Data.Attributes.EntityFieldTitle)
            {
                var attribute = Title;
                var valT = attribute?.GetTypedValue(languages);
                return new PropertyRequest { Value = valT?.Item1, Result = valT?.Item2, FieldType = attribute?.Type, Source = this, Path = path };
            }

            // directly return internal properties, mark as virtual to not cause further Link resolution
            var likelyResult = new PropertyRequest
            { Result = GetInternalPropertyByName(field), FieldType = Data.Attributes.FieldIsVirtual, Source = this, Path = path };

            // New Feature in 12.03 - Experimental
            try
            {
                var logOrNull = parentLogOrNull?.SubLogOrNull($"{LogNames.Eav}.Entity");
                logOrNull.A("Nothing found in properties, will try Sub-Item navigation");
                var subItem = this.TryToNavigateToEntityInList(field, this, logOrNull, path.Add("SubEntity", field));
                if (subItem != null) return subItem;
            } catch { /* ignore */ }
            
            return likelyResult;
        }

        protected override object GetInternalPropertyByName(string attributeNameLowerInvariant)
        {
            // first check a field which doesn't exist on EntityLight
            if (attributeNameLowerInvariant == Data.Attributes.EntityFieldIsPublished) return IsPublished;

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
}
