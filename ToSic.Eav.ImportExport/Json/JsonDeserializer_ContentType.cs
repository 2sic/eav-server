using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types.Builder;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public IContentType DeserializeContentType(string serialized)
        {
            var jsonObj = UnpackAndTestGenericJsonV1(serialized);

            var jsonType = jsonObj.ContentType;

            var type = new ContentType(AppId, jsonType.Name, jsonType.Id, 0, 
                jsonType.Scope, 
                jsonType.Description,
                jsonType.Sharing?.ParentId, jsonType.Sharing?.ParentZoneId ?? 0, 
                jsonType.Sharing?.ParentAppId ?? 0,
                jsonType.Sharing?.AlwaysShare ?? false, 
                null);

            var ctMeta = jsonType.Metadata?.Select(je => Deserialize(je, false)).ToList() ?? new List<IEntity>();
            type.AttachMetadata(ctMeta);

            var attribs = jsonType.Attributes.Select((attr, pos) =>
            {
                var attDef = new AttributeDefinition(AppId, attr.Name, attr.Type, attr.IsTitle, 0, pos) ;
                var md = attr.Metadata?.Select(m => Deserialize(m, false)).ToList() ?? new List<IEntity>();
                attDef.AddMetadata(md);
                return (IAttributeDefinition) attDef;
            }).ToList();

            type.Attributes = attribs;

            return type;
        }

    }
}
