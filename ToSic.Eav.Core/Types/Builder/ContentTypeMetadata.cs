using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Types.Builder
{
    public static class ContentType
    {
        public static IContentType ContentTypeMetadata(this IContentType type, 
            string label, 
            string description = null,
            string notes = null, 
            string icon = null,
            string link = null,
            string editInstructions = null)
        {
            var values = new Dictionary<string, object>
            {
                {"Label", label},
                {"Description", description},
                {"Notes", notes},
                {"Icon", icon},
                {"Link", link},
                {"EditInstructions", editInstructions}
            };

            // clear unused values
            foreach (var pair in values)
                if (pair.Value == null)
                    values.Remove(pair.Key);

            type.AddMetadata("ContentType", values);
            return type; // for chaining...
        }
    }
}
