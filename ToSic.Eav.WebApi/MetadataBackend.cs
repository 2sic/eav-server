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
using ToSic.Eav.WebApi.Dto;
using static System.StringComparison;
using static ToSic.Eav.Metadata.Decorators;
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
            var wrapLog = Log.Call<MetadataListDto>($"appId:{appId},targetType:{targetType},keyType:{keyType},key:{key},contentType:{contentType}");
            IEnumerable<IEntity> entityList = null;

            var appState = _appStates.Get(appId);

            var mdFor = new JsonMetadataFor()
            {
                // #TargetTypeIdInsteadOfTarget
                Target = _metadataTargets.GetName(targetType),
                TargetType = targetType,
            };
            Log.Add($"Target: {mdFor.Target} ({targetType})");
            switch (keyType)
            {
                case "guid":
                    if (Guid.TryParse(key, out var guidKey))
                    {
                        Log.Add($"guid:{guidKey}");
                        mdFor.Guid = guidKey;
                        entityList = appState.GetMetadata(targetType, guidKey, contentType);
                    }
                    else
                        Log.Add($"error: invalid guid:{key}");
                    break;
                case "string":
                    Log.Add($"string:{key}");
                    mdFor.String = key;
                    entityList = appState.GetMetadata(targetType, key, contentType);
                    break;
                case "number":
                    if (int.TryParse(key, out var keyInt))
                    {
                        Log.Add($"number:{keyInt}");
                        mdFor.Number = keyInt;   
                        entityList = appState.GetMetadata(targetType, keyInt, contentType);
                    }
                    else
                        Log.Add($"error: invalid number:{key}");
                    break;
                default:
                    Log.Add($"error: key type unknown");
                    throw new Exception("key type unknown:" + keyType);
            }

            if(entityList == null)
            {
                Log.Add($"error: entityList is null");
                throw new Exception($"Was not able to convert '{key}' to key-type {keyType}, must cancel");
            }

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

        private IEnumerable<MetadataRecommendationDto> GetRecommendations(AppState appState, int targetType, string keyType, string key, string contentType = null)
        {
            var wrapLog = Log.Call<IEnumerable<MetadataRecommendationDto>>($"targetType: {targetType}");

            // If a specific contentType was given, that's the only thing we'll recommend
            // This is the case for a Permissions dialog
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                var type = appState.GetContentType(contentType);
                if (type == null) return wrapLog("existing, not found", null);
                return wrapLog("use existing name", new[] { new MetadataRecommendationDto(type, null, -1, "Use preset type") });
            }

            // Only support TargetType which is a predefined
            if (!Enum.IsDefined(typeof(TargetTypes), targetType))
                return wrapLog("invalid target type", null);

            // Find Content-Types marked with `MetadataFor` this specific target
            // For example Types which are marked to decorate an App
            var initialTypes =
                (TypesWhichDeclareTheyAreForTheTarget(appState, targetType, key) ?? new List<Tuple<IContentType, IEntity>>())
                .Select(set => new MetadataRecommendationDto(set.Item1, set.Item2, 1, "Self-Declaring"));

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
            }
            return new List<MetadataRecommendationDto>();

        }


        private List<Tuple<IContentType, IEntity>> TypesWhichDeclareTheyAreForTheTarget(AppState appState, int targetType, string key)
        {
            var wrapLog = Log.Call<List<Tuple<IContentType, IEntity>>>();
            // for path comparisons, make sure we have the slashes cleaned
            var keyForward = (key ??"").ForwardSlash().Trim();

            // Do this #StepByStep to better debug in case of issues
            var allTypes = appState.ContentTypes;
            var recommendedTypes = allTypes
                .Select(ct =>
                {
                    var decor = ct.Metadata
                        .OfType(MetadataForDecoratorId)
                        .FirstOrDefault(dec => dec.GetBestValue<int>(MetadataForTargetTypeField, Array.Empty<string>()) == targetType);
                    return new
                    {
                        Found = decor != null,
                        Type = ct, 
                        Decorator = decor
                    };
                });

            // Filter out these without recommendations
            recommendedTypes = recommendedTypes
                .Where(set => set.Found);

            recommendedTypes = recommendedTypes
                .Where(set =>
                {
                    var decor = set.Decorator;

                    var targetName = decor.GetBestValue<string>(MetadataForTargetNameField, Array.Empty<string>()) ?? "";

                    switch (targetType)
                    {
                        case (int)TargetTypes.Undefined:
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

            var result = recommendedTypes
                .Select(set => new Tuple<IContentType, IEntity>(set.Type, set.Decorator))
                .ToList();

            return wrapLog($"{result.Count}", result);
        }

        private List<MetadataRecommendationDto> GetRecommendationsOfMetadata(AppState appState, IMetadataOf md, string debugMessage)
        {
            var wrapLog = Log.Call<List<MetadataRecommendationDto>>();

            var recommendations = md.OfType(MetadataExpectedDecoratorId).FirstOrDefault();
            if (recommendations == null) return wrapLog("no recommendations", null);
            var config = recommendations.GetBestValue<string>(MetadataExpectedTypesField, new string[0]);
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
                : wrapLog("use existing name", new MetadataRecommendationDto(type, null, count, debugMessage));
        }
    }
}