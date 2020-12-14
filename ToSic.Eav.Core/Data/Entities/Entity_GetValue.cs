using System.Linq;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {

        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages)
            => _useLightModel
                ? base.GetBestValue(attributeName)
                : GetBestValueAndType(attributeName, languages, out _);


        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages) => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages));



        private object GetBestValueAndType(string attributeName, string[] languages, out string attributeType)
        {
            languages = ExtendDimsWithDefault(languages);

            object result;

            if (Attributes.ContainsKey(attributeName))
            {
                var attribute = Attributes[attributeName];
                result = attribute[languages];
                attributeType = attribute.Type;
            }
            else if (attributeName == Constants.EntityFieldTitle)
            {
                result = Title?[languages];
                var attribute = Title;
                attributeType = attribute?.Type;
            }
            else
            {
                attributeType = Constants.EntityFieldIsVirtual;
                // directly return internal properties, don't allow further Link resolution
                result = attributeName == Constants.EntityFieldIsPublished
                    ? IsPublished
                    : GetInternalPropertyByName(attributeName);
            }

            return result;
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
