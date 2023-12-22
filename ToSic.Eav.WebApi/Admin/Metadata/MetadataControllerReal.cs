using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Decorators;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Lib.DI;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin.Metadata;

/// <summary>
/// Web API Controller for MetaData
/// Metadata-entities (content-items) are additional information about some other object
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MetadataControllerReal: ServiceBase, IMetadataController
{
    public const string LogSuffix = "MetaDt";

    #region Constructor

    public MetadataControllerReal(IConvertToEavLight converter, IAppStates appStates, ITargetTypes metadataTargets, LazySvc<MdRecommendations> mdRead) : base($"{EavLogs.WebApi}.{LogSuffix}Rl")
    {
        ConnectServices(
            _converter = converter,
            _appStates = appStates,
            _metadataTargets = metadataTargets,
            _mdRead = mdRead
        );
        _converter.Type.Serialize = true;
        _converter.Type.WithDescription = true;
    }
    private readonly IConvertToEavLight _converter;
    private readonly IAppStates _appStates;
    private readonly ITargetTypes _metadataTargets;
    private readonly LazySvc<MdRecommendations> _mdRead;

    #endregion

    /// <summary>
    /// Get Entities with specified AssignmentObjectTypeId and Key
    /// </summary>
    public MetadataListDto Get(int appId, int targetType, string keyType, string key, string contentType = null)
    {
        var l = Log.Fn<MetadataListDto>($"appId:{appId},targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");
        var appState = _appStates.GetReader(appId);

        var (entityList, mdFor) = GetEntityListAndMd(targetType, keyType, key, contentType, appState);

        if(entityList == null)
        {
            l.A("error: entityList is null");
            throw l.Done(new Exception($"Was not able to convert '{key}' to key-type {keyType}, must cancel"));
        }

        _mdRead.Value.Init(appState);

        // When retrieving all items, make sure that permissions are _not_ included
        if (IsNullOrEmpty(contentType))
        {
            entityList = entityList.Where(e => !Eav.Security.Permission.IsPermission(e)).ToList();
            l.A($"Filtered for ContentType '{contentType}' - count: {entityList.Count}");
        }

        IEnumerable<MetadataRecommendation> recommendations = null;
        try
        {
            recommendations = _mdRead.Value.GetAllowedRecommendations(targetType, key, contentType);
        }
        catch (Exception e)
        {
            l.A("Error getting recommendations");
            l.Ex(e);
        }

        try
        {
            mdFor.Title = appState.FindTargetTitle(targetType, key);
            l.A($"title: '{mdFor.Title}'");
        }
        catch { /* experimental / ignore */ }

        _converter.WithGuid = true;
        var result = new MetadataListDto
        {
            For = mdFor,
            Items = _converter.Convert(entityList),
            Recommendations = recommendations,
        };

        // Special case for content-types without fields, ensure there is still a title
        foreach (var item in result.Items)
            if (item.TryGetValue(Attributes.TitleNiceName, out var title)
                && title == null
                && item.TryGetValue(ConvertToEavLight.InternalTypeField, out var typeInfo))
            {
                if (typeInfo is JsonType typeDic && typeDic.Name != null) 
                    item[Attributes.TitleNiceName] = typeDic.Name;
            }

        return l.ReturnAsOk(result);
    }

    /// <summary>
    /// Get a stable Metadata-Header and the entities which are for this target
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private (List<IEntity> entityList, JsonMetadataFor mdFor) GetEntityListAndMd(int targetType, string keyType, string key, string contentType, IMetadataSource appState)
    {
        var l = Log.Fn<(List<IEntity> entityList, JsonMetadataFor mdFor)>($"targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");
        var mdFor = new JsonMetadataFor
        {
            // #TargetTypeIdInsteadOfTarget
            Target = _metadataTargets.GetName(targetType),
            TargetType = targetType,
        };
        l.A($"Target: {mdFor.Target} ({targetType})");

        switch (keyType)
        {
            case "guid":
                if (!Guid.TryParse(key, out var guidKey)) return l.Return((null, mdFor), $"error: invalid guid:{key}");
                mdFor.Guid = guidKey;
                var md = appState.GetMetadata(targetType, guidKey, contentType).ToList();
                return l.Return((md, mdFor), $"guid:{guidKey}; count:{md.Count}");
            case "string":
                mdFor.String = key;
                md = appState.GetMetadata(targetType, key, contentType).ToList();
                return l.Return((md, mdFor), $"string:{key}; count:{md.Count}");
            case "number":
                if (!int.TryParse(key, out var keyInt)) return l.Return((null, mdFor), $"error: invalid number:{key}");
                mdFor.Number = keyInt;
                md = appState.GetMetadata(targetType, keyInt, contentType).ToList();
                return l.Return((md, mdFor), $"number:{keyInt}; count:{md.Count}");
            default:
                throw l.Done(new Exception("key type unknown:" + keyType));
        }
    }

}