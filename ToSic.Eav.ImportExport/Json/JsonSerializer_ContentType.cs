using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public string Serialize(IContentType contentType)
        {
            var package = ToPackage(contentType, false);

            var simple = JsonConvert.SerializeObject(package, JsonSettings.Defaults());
            return simple;
        }

        public JsonFormat ToPackage(IContentType contentType, bool includeSharedTypes)
        {
            var wrapLog = Log.Call<JsonFormat>(contentType.Name);
            var package = new JsonFormat {ContentType = ToJson(contentType, includeSharedTypes)};

            // now v12 - try to include metadata items
            try
            {
                // check all metadata of these attributes - get possible sub-entities attached
                var mdParts = contentType.Attributes
                    .SelectMany(a => a.Metadata.SelectMany(m => m.Children()))
                    .ToList();
                Log.Add($"Sub items: {mdParts.Count}");
                package.Entities = mdParts.Select(e => ToJson(e, 0)).ToArray();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return wrapLog(null, package);
        }

        public JsonContentType ToJson(IContentType contentType)
            => ToJson(contentType, false);

        public JsonContentType ToJson(IContentType contentType, bool includeSharedTypes)
        {
            var sharableCt = contentType as IContentTypeShared;
            JsonContentTypeShareable jctShare = null;

            var attribs = contentType.Attributes
                .OrderBy(a => a.SortOrder)
                .Select(a => new JsonAttributeDefinition
                {
                    Name = a.Name,
                    Type = a.Type,
                    InputType = a.InputType(),
                    IsTitle = a.IsTitle,
                    Metadata = a.Metadata
                        ?.Select(md => ToJson(md)) /* important: must write the method with params, otherwise default param metadata = 1 instead of 0*/
                        .ToList()
                })
                .ToList();

            // clean up metadata info on this metadata list, as it's already packed inside something it's related to
            attribs.Where(a => a.Metadata != null)
                .SelectMany(a => a.Metadata)
                .ToList()
                .ForEach(e => e.For = null);

            var typeIsShared = sharableCt != null && (sharableCt.AlwaysShareConfiguration ||
                                                      sharableCt.ParentId.HasValue && sharableCt.ParentId !=
                                                      Constants.SystemContentTypeFakeParent);
            if (typeIsShared && !includeSharedTypes)
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
                Metadata = contentType.Metadata.Select(md => ToJson(md)).ToList()
            };
            return package;
        }
    }
}
