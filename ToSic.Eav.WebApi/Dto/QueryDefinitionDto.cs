using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Formats
{

    public class QueryDefinitionInfo
    {
        public Dictionary<string, object> Pipeline;
        public readonly List<Dictionary<string, object>> DataSources = new List<Dictionary<string, object>>();
    }
}
