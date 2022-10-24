using System.Collections.Generic;
using System.Text.Json.Nodes;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.DataSources.Queries
{
    //public class QueryPartDefinitionDto
    //{
    //    public QueryPartDefinitionDto(QueryPartDefinition part)
    //    {
    //        Name = part.Title;
    //        Id = EntityId = part.Id;
    //        Guid = EntityGuid = part.Guid.ToString();

    //        DataSourceType = PartAssemblyAndType = part.DataSourceType;

    //        Description = part.Entity.GetBestValue<string>(nameof(Description), new string[0]);

    //        VisualDesignerData = JsonConvert.DeserializeObject(part.VisualDesignerData);
    //    }

    //    public string Description { get; set; }

    //    // Guid must be a string, as it's sometimes sent as "unsaved" from the front-end
    //    public string Guid { get; set; }
    //    public string EntityGuid { get; set; }

    //    public int Id { get; set; }
    //    public int EntityId { get; set; }

    //    public string Name { get; set; }

    //    public string PartAssemblyAndType { get; set; }
    //    public string DataSourceType { get; set; }

    //    // IDictionary<string, object>
    //    public object VisualDesignerData { get; set; }

    //}

    // TODO: this is the old untyped implementation - should be replaced with a clean Dto version
    // Note that the DTO already exists in another file...

    // not really easy, because it appears that the EntityGuid is used as a string to send back infos from the UI
    public static class QueryPartDefExtTemp
    {
        public static Dictionary<string, object> AsDictionary(this QueryPartDefinition part)
        {
            var attributes = part.Entity.AsDictionary();

            attributes[QueryConstants.VisualDesignerData] = JsonNode.Parse(part.VisualDesignerData, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions).AsObject();

            // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
            attributes[QueryConstants.PartAssemblyAndType] = part.DataSourceType;

            return attributes;
        }

    }
}
