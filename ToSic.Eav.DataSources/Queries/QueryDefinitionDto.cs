using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.DataSources.Queries
{

    public class QueryDefinitionDto
    {
        [JsonConstructor]
        public QueryDefinitionDto(Dictionary<string, object> pipeline, List<Dictionary<string, object>> dataSources) =>
            (Pipeline, DataSources) = (pipeline, dataSources);

        public QueryDefinitionDto() { }

        public Dictionary<string, object> Pipeline { get; set; }
        public List<Dictionary<string, object>> DataSources { get; } = new List<Dictionary<string, object>>();
    }
}
