using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.Helpers;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Types;
using ToSic.Eav.WebApi.Dto;
using static System.StringComparison;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public class MetadataBackend: HasLog<MetadataBackend>
    {

        public MetadataBackend(IConvertToEavLight converter, IAppStates appStates, ITargetTypes metadataTargets) : base($"{LogNames.WebApi}.MetaDT")
        {
            _converter = converter;
            _appStates = appStates;
            _metadataTargets = metadataTargets;

            _converter.Type.Serialize = true;
            _converter.Type.WithDescription = true;
        }

        private readonly IConvertToEavLight _converter;
        private readonly IAppStates _appStates;
        private readonly ITargetTypes _metadataTargets;

        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public MetadataListDto Get(int appId, int targetType, string keyType, string key, string contentType = null)
        {
            var wrapLog = Log.Call<MetadataListDto>();
            IEnumerable<IEntity> entityList = null;

            var appState = _appStates.Get(appId);

            var mdFor = new JsonMetadataFor()
            {
                Target = _metadataTargets.GetName(targetType)
            };
            switch (keyType)
            {
                case "guid":
                    if (Guid.TryParse(key, out var guidKey))
                    {
                        entityList = appState.GetMetadata(targetType, guidKey, contentType);
                        mdFor.Guid = guidKey;
                    }
                    break;
                case "string":
                    entityList = appState.GetMetadata(targetType, key, contentType);
                    mdFor.String = key;
                    break;
                case "number":
                    if (int.TryParse(key, out var keyInt))
                    {
                        entityList = appState.GetMetadata(targetType, keyInt, contentType);
                        mdFor.Number = keyInt;
                    }
                    break;
                default:
                    throw new Exception("key type unknown:" + keyType);
            }

            if(entityList == null)
                throw new Exception($"Was not able to convert '{key}' to key-type {keyType}, must cancel");

            // When retrieving all items, make sure that permissions are _not_ included
            if (string.IsNullOrEmpty(contentType))
                entityList = entityList.Where(e => !Eav.Security.Permission.IsPermission(e));

            IEnumerable<MetadataRecommendationDto> recommendations = null;
            try
            {
                recommendations = GetRecommendations(appState, targetType, keyType, key, contentType);
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
            }
            catch { /* experimental / ignore */ }

            _converter.WithGuid = true;
            var result = new MetadataListDto()
            {
                For = mdFor,
                Items = _converter.Convert(entityList),
                Recommendations = recommendations,
            };


            return wrapLog("ok", result);
        }

        private IEnumerable<MetadataRecommendationDto> GetRecommendations(AppState appState, int targetType, string keyType, string key, string contentType = null)
        {
            var wrapLog = Log.Call<IEnumerable<MetadataRecommendationDto>>($"targetType: {targetType}");

            // If a specific contentType was given, that's the only thing we'll recommend
            // This is the case for a Permissions dialog
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                var type = appState.GetContentType(contentType);
                if (type == null) return wrapLog("existing, not found", null);
                return wrapLog("use existing name", new[] { new MetadataRecommendationDto(type, -1, "Use preset type") });
            }

            // Only support TargetType which is a predefined
            if (!Enum.IsDefined(typeof(TargetTypes), targetType))
                return wrapLog("invalid target type", null);

            // Find Content-Types marked with `MetadataFor` this specific target
            // For example Types which are marked to decorate an App
            var initialTypes =
                (FindSelfDeclaringTypes(appState, targetType, key) ?? new List<IContentType>())
                .Select(ct => new MetadataRecommendationDto(ct, 1, "Self-Declaring"));

            // Check if this object-type has a specific list of Content-Types which it expects
            // For example a attribute which says "I want this kind of Metadata"
            // Not fully worked out yet...
            // TODO #metadata
            var attachedRecommendations = GetAttachedRecommendations(appState, targetType, key)
                ?? new List<MetadataRecommendationDto>();

            attachedRecommendations.AddRange(initialTypes);

            var distinct = attachedRecommendations.Distinct();

            return wrapLog("unknown case", distinct);
        }

        private List<MetadataRecommendationDto> GetAttachedRecommendations(AppState appState, int targetType, string key)
        {
            var wrapLog = Log.Call<List<MetadataRecommendationDto>>($"targetType: {targetType}");

            switch (targetType)
            {
                case 0:
                case (int)TargetTypes.None:
                    return wrapLog("no target", null);
                case (int)TargetTypes.Attribute:
                    // TODO - PROBABLY TRY TO FIND THE ATTRIBUTE
                    return wrapLog("attributes not supported ATM", null);
                case (int)TargetTypes.App:
                    // TODO: this won't work - needs another way of finding assignments
                    return wrapLog("app", GetRecommendationsOfMetadata(appState, appState.Metadata, "attached to App"));
                case (int)TargetTypes.Entity:
                    if (!Guid.TryParse(key, out var guidKey)) return wrapLog("entity not guid", null);
                    var entity = appState.List.One(guidKey);
                    if (entity == null) return wrapLog("entity not found", null);
                    return wrapLog("entity", GetRecommendationsOfMetadata(appState, entity.Metadata, "attached to Entity"));
                case (int)TargetTypes.ContentType:
                    var ct = appState.GetContentType(key);
                    if (ct == null) return wrapLog("type not found", null);
                    return wrapLog("content type", GetRecommendationsOfMetadata(appState, ct.Metadata, "attached to Content-Type"));
                case (int)TargetTypes.Zone:
                case (int)TargetTypes.CmsItem:
                    // todo: maybe improve?
                    return wrapLog("zone or CmsObject not supported", null);
            };
            return new List<MetadataRecommendationDto>();

        }


        private List<IContentType> FindSelfDeclaringTypes(AppState appState, int targetType, string key)
        {
            var wrapLog = Log.Call<List<IContentType>>();
            // for path comparisons, make sure we have the slashes cleaned
            var keyForward = (key ??"").ForwardSlash().Trim();

            var recommendedTypes = appState.ContentTypes
                .Where(ct =>
                {
                    var decor = ct.Metadata.OfType(Decorators.MetadataForDecoratorId).FirstOrDefault();
                    if (decor == null) return false;
                    if (decor.GetBestValue<int>(Decorators.MetadataForTargetTypeField, new string[0]) != targetType)
                        return false;

                    var targetName = decor.GetBestValue<string>(Decorators.MetadataForTargetNameField, new string[0]) ?? "";

                    switch (targetType)
                    {
                        case 0:
                        case (int)TargetTypes.None:
                            return false;
                        case (int)TargetTypes.Attribute:
                            if (targetName.Equals(key, InvariantCultureIgnoreCase)) return true;
                            var attr = appState.FindAttribute(key);
                            return string.Equals(keyForward, attr.Item1.StaticName + "/" + attr.Item2.Name, InvariantCultureIgnoreCase)
                                   || string.Equals(keyForward, attr.Item1.Name + "/" + attr.Item2.Name, InvariantCultureIgnoreCase);
                        // App and ContentType don't need extra specifiers
                        case (int)TargetTypes.App: return true;
                        case (int)TargetTypes.Entity:
                            if (!Guid.TryParse(key, out var guidKey)) return false;
                            var entity = appState.List.One(guidKey);
                            return entity?.Type?.Is(targetName) ?? false;
                        // App and ContentType don't need extra specifiers
                        case (int)TargetTypes.ContentType: return true;
                        case (int)TargetTypes.Zone: return true;
                        // todo: not handled yet #metadata
                        case (int)TargetTypes.CmsItem: return false;
                        default: return false;
                    }
                }).ToList();
            return wrapLog($"{recommendedTypes.Count}", recommendedTypes);
        }

        private List<MetadataRecommendationDto> GetRecommendationsOfMetadata(AppState appState, IMetadataOf md, string debugMessage)
        {
            var wrapLog = Log.Call<List<MetadataRecommendationDto>>();

            var recommendations = md.OfType(Decorators.MetadataExpectedDecoratorId).FirstOrDefault();
            if (recommendations == null) return wrapLog("no recommendations", null);
            var config = recommendations.GetBestValue<string>(Decorators.MetadataExpectedTypesField, new string[0]);
            if (string.IsNullOrWhiteSpace(config)) return wrapLog("no values in config", null);
            var result = config
                .Split(',')
                .Select(s => s.Trim())
                .Select(s => FindTypeInAppAsRecommendation(appState, s, 1, debugMessage))
                .ToList();

            return wrapLog("found", result);
        }

        private MetadataRecommendationDto FindTypeInAppAsRecommendation(AppState appState, string name, int count, string debugMessage)
        {
            var wrapLog = Log.Call<MetadataRecommendationDto>(name);

            if (string.IsNullOrWhiteSpace(name)) 
                return wrapLog("empty name", null);
            
            var type = appState.GetContentType(name);
            return type == null 
                ? wrapLog("name not found", null) 
                : wrapLog("use existing name", new MetadataRecommendationDto(type, count, debugMessage));
        }
    }
}