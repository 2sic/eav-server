﻿using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public string Serialize(IContentType contentType)
        {
            var package = new JsonFormat {ContentType = ToJson(contentType)};

            var simple = JsonConvert.SerializeObject(package, JsonSerializerSettings());
            return simple;
        }

        private JsonContentType ToJson(IContentType contentType)
        {
            var sharableCt = contentType as IUsesSharedDefinition;
            JsonContentTypeShareable jctShare = null;

            var attribs = contentType.Attributes.OrderBy(a => a.SortOrder).Select(attrib => new JsonAttributeDefinition
            {
                Name = attrib.Name,
                Type = attrib.Type,
                IsTitle = attrib.IsTitle,
                Metadata = attrib.MetadataItems?.Select(ToJson).ToList()
            }).ToList();

            // clean up metadata info on this metadata list, as it's already packed inside something it's related to
            attribs.Where(a => a.Metadata != null).SelectMany(a => a.Metadata).ToList().ForEach(e => e.For = null);

            if (sharableCt != null && (sharableCt.AlwaysShareConfiguration || sharableCt.ParentId.HasValue && sharableCt.ParentId != Constants.SystemContentTypeFakeParent))
            {
                // if it's a shared type, flush definition as we won't include it
                if (sharableCt.ParentId.HasValue)
                    attribs = null; 

                jctShare = new JsonContentTypeShareable
                {
                    AlwaysShare = sharableCt.AlwaysShareConfiguration,
                    ParentAppId = sharableCt.ParentAppId,
                    ParentZoneId = sharableCt.ParentZoneId,
                    ParentId = sharableCt.ParentId
                };
            }
            var package = new JsonContentType
            {
                Id = contentType.StaticName,
                Name = contentType.Name,
                Scope = contentType.Scope,
                Description = contentType.Description,
                Attributes = attribs,
                Sharing = jctShare,
                Metadata = contentType.Metadata.Select(ToJson).ToList()
            };
            return package;
        }
    }
}
