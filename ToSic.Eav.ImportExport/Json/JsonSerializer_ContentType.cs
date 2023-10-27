using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public string Serialize(IContentType contentType, JsonSerializationSettings settings = default)
        {
            var package = ToPackage(contentType, /*false,*/ settings ?? new JsonSerializationSettings
            {
                CtIncludeInherited = false,
                CtAttributeIncludeInheritedMetadata = true
            });

            var simple = System.Text.Json.JsonSerializer.Serialize(package, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            return simple;
        }

        public JsonFormat ToPackage(IContentType contentType, JsonSerializationSettings settings)
        {
            var l = Log.Fn<JsonFormat>(contentType.Name);
            var package = new JsonFormat
            {
                ContentType = ToJson(contentType, settings)
            };

            // now v12 - try to include metadata items
            try
            {
                // check all metadata of these attributes - get possible sub-entities attached
                var attribMdItems = contentType.Attributes.SelectMany(a => a.Metadata).ToArray();
                var attribMdEntityAttribs = attribMdItems.SelectMany(m => m.Attributes
                        .Where(a => a.Value.Type == ValueTypes.Entity))
                    .ToArray();
                var mdParts =
                    // On Dynamically Typed Entities, the Children()-Call won't work, because the Relationship-Manager doesn't know the children.
                    // So we must go the hard way and look at each ObjectContents
                    attribMdEntityAttribs
                        .SelectMany(a => a.Value.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>)
                        .Where(e => e != null) // filter out possible null items
                        .ToList();

                l.A($"Sub items: {mdParts.Count}");
                package.Entities = mdParts.Select(e => ToJson(e, 0)).ToArray();
            }
            catch (Exception ex)
            {
                l.Ex(ex);
            }

            return l.ReturnAsOk(package);
        }

        // Note: only seems to be used in a test...
        public JsonContentType ToJson(IContentType contentType)
            => ToJson(contentType, /*false,*/ new JsonSerializationSettings { CtIncludeInherited = false, CtAttributeIncludeInheritedMetadata = true });

        private JsonContentType ToJson(IContentType contentType, JsonSerializationSettings settings)
        {
            JsonContentTypeShareable jctShare = null;

            var attribs = contentType.Attributes
                .OrderBy(a => a.SortOrder)
                .Select(a =>
                {
                    // #SharedFieldDefinition
                    var inheritsMetadata = a.SysSettings?.InheritsMetadata() == true; // a.SysSettings?.Inherit != null && a.SysSettings.InheritMetadata;
                    var metadata = inheritsMetadata && !settings.CtAttributeIncludeInheritedMetadata
                        ? null 
                        : a.Metadata?.Select(md => ToJson(md)).ToList(); /* important: must call with params, otherwise default param metadata = 1 instead of 0*/
                    return new JsonAttributeDefinition
                    {
                        Name = a.Name,
                        Type = a.Type.ToString(),
                        InputType = a.InputType(),
                        IsTitle = a.IsTitle,
                        Metadata = metadata,

                        // #SharedFieldDefinition
                        Guid = a.Guid,
                        SysSettings = JsonAttributeSysSettings.FromSysSettings(a.SysSettings),
                    };
                })
                .ToList();

            // clean up metadata info on this metadata list, as it's already packed inside something it's related to
            attribs.Where(a => a.Metadata != null)
                .SelectMany(a => a.Metadata)
                .ToList()
                .ForEach(e => e.For = null);

            var sharableCt = contentType as IContentTypeShared;

            var ancestorDecorator = contentType.GetDecorator<IAncestor>();
            var isSharedNew = ancestorDecorator != null &&
                              ancestorDecorator.Id != Constants.PresetContentTypeFakeParent;

            // Note 2021-11-22 2dm - AFAIK this is skipped when creating a JSON for edit-UI
            if (isSharedNew && !settings.CtIncludeInherited)
            {
                // if it's a shared type, flush definition as we won't include it
                if (ancestorDecorator.Id != 0) attribs = null;

                jctShare = new JsonContentTypeShareable
                {
                    AlwaysShare = sharableCt.AlwaysShareConfiguration,
                    ParentAppId = ancestorDecorator.AppId,
                    ParentZoneId = ancestorDecorator.ZoneId,
                    ParentId = ancestorDecorator.Id,
                };
            }
            var package = new JsonContentType
            {
                Id = contentType.NameId,
                Name = contentType.Name,
                Scope = contentType.Scope,
                Attributes = attribs,
                Sharing = jctShare,
                Metadata = contentType.Metadata.Select(md => ToJson(md)).ToList()
            };
            return package;
        }
    }
}
