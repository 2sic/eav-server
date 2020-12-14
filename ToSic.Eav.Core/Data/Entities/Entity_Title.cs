using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public new IAttribute Title => TitleFieldName == null
            ? null
            : Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null;


        /// <inheritdoc />
        public new string GetBestTitle() => GetBestTitle(null, 0);

        /// <inheritdoc />
        public string GetBestTitle(string[] dimensions) => GetBestTitle(dimensions, 0);

        /// <inheritdoc />
        internal string GetBestTitle(string[] dimensions, int recursionCount)
        {
            var bestTitle = GetBestValue(Constants.EntityFieldTitle, dimensions);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            var maybeRelationship = bestTitle as IEnumerable<IEntity>;
            if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
                bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
                            .GetBestTitle(dimensions, recursionCount + 1)
                            ?? bestTitle;

            return bestTitle?.ToString();
        }
    }
}
