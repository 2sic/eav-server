using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.WebApi.Helpers
{
    internal static class QueryHelpers
    {
        public static Dictionary<string, object> AsDictionary(this QueryPartDefinition part)
        {
            var attributes = part.Entity.AsDictionary();

            attributes[QueryConstants.VisualDesignerData] = JsonConvert.DeserializeObject(part.VisualDesignerData);

            // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
            attributes[QueryConstants.PartAssemblyAndType] = part.DataSourceType;

            return attributes;
        }
    }
}
