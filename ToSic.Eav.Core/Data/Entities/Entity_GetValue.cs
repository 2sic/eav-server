using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {

        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages)
            => _useLightModel
                ? base.GetBestValue(attributeName)
                : FindPropertyInternal(attributeName, languages, null).Result;


        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages) => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages));


        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string attributeName, string[] languages, ILog parentLogOrNull)
        {
            languages = ExtendDimsWithDefault(languages);
            attributeName = attributeName.ToLowerInvariant();
            if (Attributes.ContainsKey(attributeName))
            {
                var attribute = Attributes[attributeName];
                return new PropertyRequest {Result = attribute[languages], FieldType = attribute.Type, Source = this};
            }
            
            if (attributeName == Data.Attributes.EntityFieldTitle)
            {
                var attribute = Title;
                return new PropertyRequest { Result = Title?[languages], FieldType = attribute?.Type, Source = this};
            }

            // directly return internal properties, mark as virtual to not cause further Link resolution
            return new PropertyRequest
                {Result = GetInternalPropertyByName(attributeName), FieldType = Data.Attributes.FieldIsVirtual, Source = this};
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
