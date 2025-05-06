using ToSic.Eav.Data.Shared;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Internal.Generics;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer
{
    public string Serialize(IContentType contentType, JsonSerializationSettings settings = default)
    {
        var package = ToPackage(contentType, settings ?? new JsonSerializationSettings
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

        // v17 option to skip including additional entities
        if (!settings.CtWithEntities)
            l.Return(package, "done without additional entities");

        // Include things within the metadata items
        try
        {
            // check all metadata of these attributes - get possible sub-entities attached
            var attribMds = contentType.Attributes
                .SelectMany(a =>
                    (GetMetadataOrSkip(a, settings) ?? Array.Empty<IEntity>() as IEnumerable<IEntity>)
                    .Select(e => new
                    {
                        // Add these for better debugging
                        a.Name,
                        a.Type,
                        // This is the only one we really need
                        Entity = e,
                    })
                )
                .ToArray();

            var attribMdsOfEntity = attribMds
                .SelectMany(set => set.Entity.Attributes
                    .Where(a => a.Value.Type == ValueTypes.Entity)
                    .Select(a => new { set.Name, a.Key, a.Value })
                )
                .ToArray();

            var mdParts =
                // On Dynamically Typed Entities, the Children()-Call won't work, because the Relationship-Manager doesn't know the children.
                // So we must go the hard way and look at each ObjectContents
                attribMdsOfEntity
                    .SelectMany(a => a.Value.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>)
                    .Where(e => e != null) // filter out possible null items
                    .ToList();

            // In some cases we may have references to the same entity, in which case we only need one
            var mdDeduplicated = mdParts
                .DistinctBy(e => e.EntityId)
                .ToList();

            l.A($"Sub items: {mdParts.Count}; Deduplicated: {mdDeduplicated.Count}");
            package.Entities = mdDeduplicated
                .Select(e => ToJson(e,  metadataDepth: 0))
                .ToList();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
        }

        return l.ReturnAsOk(package);
    }

    /// <summary>
    /// Get an Attributes Metadata - if it's directly owned, or if include-shared is active.
    ///
    /// TODO: Advanced cases where just a part of the metadata is inherited is not yet supported.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static IMetadataOf GetMetadataOrSkip(IContentTypeAttribute a, JsonSerializationSettings settings)
    {
        var inheritsMetadata = a.SysSettings?.InheritMetadata == true;
        var skipMetadata = inheritsMetadata && !settings.CtAttributeIncludeInheritedMetadata;
        return skipMetadata ? null : a.Metadata;
    }

    // Note: only seems to be used in a test...
    public JsonContentType ToJson(IContentType contentType)
        => ToJson(contentType, new()
        {
            CtIncludeInherited = false,
            CtAttributeIncludeInheritedMetadata = true
        });

    private JsonContentType ToJson(IContentType contentType, JsonSerializationSettings settings)
    {
        JsonContentTypeShareable jctShare = null;

        var attribs = contentType.Attributes
            .OrderBy(a => a.SortOrder)
            .Select(a =>
            {
                // #SharedFieldDefinition
                var metadata = GetMetadataOrSkip(a, settings)?
                    .Select(md => ToJson(md, metadataDepth: 0)) /* important: must call with params, otherwise default param metadata = 1 instead of 0*/
                    .ToList();
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
            .ForEach(jsonEntity => jsonEntity.For = null);

        var ancestorDecorator = contentType.GetDecorator<IAncestor>();
        var isSharedNew = ancestorDecorator is { Id: not Constants.PresetContentTypeFakeParent };

        // Note 2021-11-22 2dm - AFAIK this is skipped when creating a JSON for edit-UI
        if (isSharedNew && !settings.CtIncludeInherited)
        {
            // if it's a shared type, flush definition as we won't include it
            if (ancestorDecorator.Id != 0)
                attribs = null;

            var sharableCt = (IContentTypeShared)contentType;

            jctShare = new()
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

    public string Serialize(ContentTypeAttributeSysSettings sysSettings) => Serialize(sysSettings, Log);

    internal static string Serialize(ContentTypeAttributeSysSettings sysSettings, ILog log)
    {
        var l = log.Fn<string>($"serialize {sysSettings} to json string");
        if (sysSettings == null) return l.Return(null, "null sysSettings");
        try
        {
            var json = JsonAttributeSysSettings.FromSysSettings(sysSettings);
            var simple = System.Text.Json.JsonSerializer.Serialize(json, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            return l.Return(simple, "serialized sysSettings");
        }
        catch (Exception e)
        {
            l.Done(e);
            throw;
        }
    }
}