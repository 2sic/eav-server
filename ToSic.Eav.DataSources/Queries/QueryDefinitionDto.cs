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
            // JsonSerializer.DeserializeAsync happens in SystemTextJsonMediaTypeFormatter.
            // Since the object type could map the JSON payload to any .NET type, while deserializing,
            // the JsonSerializer returns a boxed JsonElement instead.
            // System.Text.Json by design (and unlikely to change) don't try to infer or guess the .NET type from the JSON payload during deserialization and hence,
            // the caller (who has enough context) would need to turn the returned JsonElement into the expected .NET type.
            // https://stackoverflow.com/questions/68519985/how-do-i-get-system-text-json-to-deserialize-objects-into-their-original-type
            // TODO: stv
            // - we can modify SystemTextJsonMediaTypeFormatter to unwrap JsonElements
            // - or we can create and register our custom object convertor in our JsonOptions.DefaultOptions
            Pipeline = JsonElementUtilities.UnwrapJsonElementsInDictionary(pipeline);
            DataSources = dataSources.Select(JsonElementUtilities.UnwrapJsonElementsInDictionary).ToList();
        }

        public QueryDefinitionDto() { }

        public Dictionary<string, object> Pipeline { get; set; }
        public List<Dictionary<string, object>> DataSources { get; } = new List<Dictionary<string, object>>();
    }
}
