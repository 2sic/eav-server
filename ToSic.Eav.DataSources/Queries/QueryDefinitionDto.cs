using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.DataSources.Queries
{

    public class QueryDefinitionDto
    {
        [JsonConstructor]
        public QueryDefinitionDto(Dictionary<string, object> pipeline, List<Dictionary<string, object>> dataSources)
        {
            Pipeline = JsonElementUtilities.UnwrapJsonElementsInDictionary(pipeline);
            DataSources = dataSources.Select(JsonElementUtilities.UnwrapJsonElementsInDictionary).ToList();
        }

        public QueryDefinitionDto() { }

        public Dictionary<string, object> Pipeline { get; set; }
        public List<Dictionary<string, object>> DataSources { get; } = new List<Dictionary<string, object>>();
    }
}
