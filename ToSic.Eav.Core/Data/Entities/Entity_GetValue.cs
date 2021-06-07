using System;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {

        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages)
            => _useLightModel
                ? base.GetBestValue(attributeName)
                : ValueAndType(attributeName, languages).Item1;


        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages) => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages));


        [PrivateApi("WIP / Internal")]
        public Tuple<object, string> ValueAndType(string attributeName, string[] languages)
        {
            languages = ExtendDimsWithDefault(languages);
            attributeName = attributeName.ToLowerInvariant();
            if (Attributes.ContainsKey(attributeName))
            {
                var attribute = Attributes[attributeName];
                return new Tuple<object, string>(attribute[languages], attribute.Type);
            }
            
            if (attributeName == Data.Attributes.EntityFieldTitle)
            {
                var attribute = Title;
                return new Tuple<object, string>(Title?[languages], attribute?.Type);
            }

            // directly return internal properties, mark as virtual to not allow further Link resolution
            return new Tuple<object, string>(GetInternalPropertyByName(attributeName), Data.Attributes.FieldIsVirtual);
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
