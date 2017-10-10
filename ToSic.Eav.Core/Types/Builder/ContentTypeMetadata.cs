using System.Collections.Generic;
using System.Linq;
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
            values.Where(v => v.Value == null)
                .Select(v => v.Key).ToList()
                .ForEach(k => values.Remove(k));

            (type as Data.ContentType)?.AddMetadata("ContentType", values);
            return type; // for chaining...
        }
    }
}
