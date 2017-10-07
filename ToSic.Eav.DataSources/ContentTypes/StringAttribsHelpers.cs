using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources.ContentTypes
{
    internal static class StringAttribsHelpers
    {
        internal static AttributeDefinition StringDefault(this AttributeDefinition attDef, int rowCount)
        {
            attDef.AddMetadata("@string-default", new Dictionary<string, object> { { "RowCount", rowCount } });
            return attDef; // for chaining...
        }
    }
}
