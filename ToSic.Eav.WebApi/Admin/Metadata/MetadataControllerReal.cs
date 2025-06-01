using ToSic.Eav.Apps.Internal.MetadataDecorators;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata;
using ToSic.Metadata.Recommendations.Sys;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin.Metadata;

/// <summary>
/// Web API Controller for MetaData
/// Metadata-entities (content-items) are additional information about some other object
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataControllerReal(
    IConvertToEavLight converter,
    IAppReaderFactory appReaders,
    ITargetTypeService metadataTargets,
    LazySvc<RecommendedMetadataService> mdRead)
    : ServiceBase($"{EavLogs.WebApi}.{LogSuffix}Rl", connect: [converter, appReaders, metadataTargets, mdRead]),
        IMetadataController
{
    public const string LogSuffix = "MetaDt";

    /// <summary>
    /// Get Entities with specified TargetTypeId and Key
    /// </summary>
    public MetadataListDto Get(int appId, int targetType, string keyType, string key, string contentType = null)
    {
        var l = Log.Fn<MetadataListDto>($"appId:{appId},targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");
        var appReader = appReaders.Get(appId);

        var (entityList, mdFor) = GetExistingEntitiesAndMd(targetType, keyType, key, contentType, appReader.Metadata);

        if (entityList == null)
        {
            l.A("error: entityList is null");
            throw l.Done(new Exception($"Was not able to convert '{key}' to key-type {keyType}, must cancel"));
        }

        mdRead.Value.Setup(appReader, appId);

        // When retrieving all items, make sure that permissions are _not_ included
        if (IsNullOrEmpty(contentType))
        {
            entityList = entityList.Where(e => !Eav.Security.Permission.IsPermission(e)).ToList();
            l.A($"Filtered for ContentType '{contentType}' - count: {entityList.Count}");
        }

        IEnumerable<MetadataRecommendation> recommendations = null;
        try
        {
            recommendations = mdRead.Value.GetAllowedRecommendations(targetType, key, contentType);
        }
        catch (Exception e)
        {
            l.A("Error getting recommendations");
            l.Ex(e);
        }

        try
        {
            mdFor.Title = appReader.FindTargetTitle(targetType, key);
            l.A($"title: '{mdFor.Title}'");
        }
        catch { /* experimental / ignore */ }

        converter.Type.Serialize = true;
        converter.Type.WithDescription = true;
        converter.WithGuid = true;

        var result = new MetadataListDto
        {
            For = mdFor,
            Items = converter.Convert(entityList),
            Recommendations = recommendations,
        };

        // Special case for content-types without fields, ensure there is still a title
        foreach (var item in result.Items)
            if (item.TryGetValue(Attributes.TitleNiceName, out var title)
                && title == null
                && item.TryGetValue(ConvertToEavLight.InternalTypeField, out var typeInfo))
            {
                if (typeInfo is JsonType { Name: not null } typeDic) 
                    item[Attributes.TitleNiceName] = typeDic.Name;
            }

        return l.ReturnAsOk(result);
    }

    /// <summary>
    /// Get a stable Metadata-Header and the entities which are for this target
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private (List<IEntity> entityList, JsonMetadataFor mdFor) GetExistingEntitiesAndMd(int targetType, string keyType, string key, string contentType, IMetadataSource mdSource)
    {
        var l = Log.Fn<(List<IEntity> entityList, JsonMetadataFor mdFor)>($"targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");
        var mdFor = new JsonMetadataFor
        {
            // #TargetTypeIdInsteadOfTarget
            Target = metadataTargets.GetName(targetType),
            TargetType = targetType,
        };
        l.A($"Target: {mdFor.Target} ({targetType})");

        switch (keyType)
        {
            case "guid":
                if (!Guid.TryParse(key, out var guidKey))
                    return l.Return((null, mdFor), $"error: invalid guid:{key}");
                mdFor.Guid = guidKey;
                var md = mdSource.GetMetadata(targetType, guidKey, contentType).ToList();
                return l.Return((md, mdFor), $"guid:{guidKey}; count:{md.Count}");
            case "string":
                mdFor.String = key;
                md = mdSource.GetMetadata(targetType, key, contentType).ToList();
                return l.Return((md, mdFor), $"string:{key}; count:{md.Count}");
            case "number":
                if (!int.TryParse(key, out var keyInt))
                    return l.Return((null, mdFor), $"error: invalid number:{key}");
                mdFor.Number = keyInt;
                md = mdSource.GetMetadata(targetType, keyInt, contentType).ToList();
                return l.Return((md, mdFor), $"number:{keyInt}; count:{md.Count}");
            default:
                throw l.Done(new Exception("key type unknown:" + keyType));
        }
    }

}