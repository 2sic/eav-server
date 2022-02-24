﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Decorators;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.Admin.Metadata
{
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public class MetadataControllerReal: HasLog<MetadataControllerReal>, IMetadataController
    {
        #region Constructor

        public MetadataControllerReal(IConvertToEavLight converter, IAppStates appStates, ITargetTypes metadataTargets, LazyInitLog<MdRecommendations> mdRead) : base($"{LogNames.WebApi}.MetaDT")
        {
            _converter = converter;
            _appStates = appStates;
            _metadataTargets = metadataTargets;
            _mdRead = mdRead.SetLog(Log);

            _converter.Type.Serialize = true;
            _converter.Type.WithDescription = true;
        }
        private readonly IConvertToEavLight _converter;
        private readonly IAppStates _appStates;
        private readonly ITargetTypes _metadataTargets;
        private readonly LazyInitLog<MdRecommendations> _mdRead;

        #endregion

        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public MetadataListDto Get(int appId, int targetType, string keyType, string key, string contentType = null)
        {
            var wrapLog = Log.Call<MetadataListDto>($"appId:{appId},targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");

            var appState = _appStates.Get(appId);

            var (entityList, mdFor) = GetEntityListAndMd(targetType, keyType, key, contentType, appState);

            if(entityList == null)
            {
                Log.Add($"error: entityList is null");
                throw new Exception($"Was not able to convert '{key}' to key-type {keyType}, must cancel");
            }

            _mdRead.Ready.Init(appState);

            // When retrieving all items, make sure that permissions are _not_ included
            if (string.IsNullOrEmpty(contentType))
                entityList = entityList.Where(e => !Eav.Security.Permission.IsPermission(e));

            IEnumerable<MetadataRecommendation> recommendations = null;
            try
            {
                recommendations = _mdRead.Ready.GetAllowedRecommendations(targetType, key, contentType);
            }
            catch (Exception e)
            {
                Log.Add("Error getting recommendations");
                Log.Exception(e);
            }

            try
            {
                var title = appState.FindTargetTitle(targetType, key);
                mdFor.Title = title;
                Log.Add($"title:{title}");
            }
            catch { /* experimental / ignore */ }

            _converter.WithGuid = true;
            var result = new MetadataListDto()
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


            return wrapLog("ok", result);
        }

        /// <summary>
        /// Get a stable Metadata-Header and the entities which are for this target
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private (IEnumerable<IEntity> entityList, JsonMetadataFor mdFor)
            GetEntityListAndMd(int targetType, string keyType, string key, string contentType, IMetadataSource appState)
        {
            var wrapLog = Log.Call<(IEnumerable<IEntity>, JsonMetadataFor)>();
            var mdFor = new JsonMetadataFor
            {
                // #TargetTypeIdInsteadOfTarget
                Target = _metadataTargets.GetName(targetType),
                TargetType = targetType,
            };
            Log.Add($"Target: {mdFor.Target} ({targetType})");

            switch (keyType)
            {
                case "guid":
                    if (!Guid.TryParse(key, out var guidKey)) return wrapLog($"error: invalid guid:{key}", (null, mdFor));
                    mdFor.Guid = guidKey;
                    return wrapLog($"guid:{guidKey}", (appState.GetMetadata(targetType, guidKey, contentType), mdFor));
                case "string":
                    mdFor.String = key;
                    return wrapLog("string:{key}", (appState.GetMetadata(targetType, key, contentType), mdFor));
                case "number":
                    if (!int.TryParse(key, out var keyInt)) return wrapLog($"error: invalid number:{key}", (null, mdFor));
                    mdFor.Number = keyInt;
                    return wrapLog($"number:{keyInt}", (appState.GetMetadata(targetType, keyInt, contentType), mdFor));
                default:
                    Log.Add("error: key type unknown");
                    throw new Exception("key type unknown:" + keyType);
            }
        }
        
    }
}