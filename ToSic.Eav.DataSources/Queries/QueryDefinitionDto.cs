using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Queries
{

    public class QueryDefinitionDto
    {
        public Dictionary<string, object> Pipeline;
        public readonly List<Dictionary<string, object>> DataSources = new List<Dictionary<string, object>>();
    }
}
