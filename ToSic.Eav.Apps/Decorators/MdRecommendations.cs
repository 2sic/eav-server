﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Eav.Helpers;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Services;
using static System.String;
using static ToSic.Eav.Apps.Decorators.MetadataRecommendation;

namespace ToSic.Eav.Apps.Decorators
{
    /// <summary>
    /// Figure out all the recommendations for a Metadata Target
    ///
    /// Note: this uses a new model for such helpers, which is different from the previous solution.
    /// We're trying to simplify Read-Helpers to not be part of the large construct but a simple standalone read service
    ///
    /// If this model works well, we'll probably reconsider how our xxxRead objects work
    /// </summary>
    /// <remarks>new in v13.02</remarks>
    public class MdRecommendations: ServiceBase
    {

        public MdRecommendations(LazySvc<MdRequirements> requirements): base($"{AppConstants.LogName}.MdRead") 
            => ConnectServices(_requirements = requirements);
        private readonly LazySvc<MdRequirements> _requirements;

        public void Init(AppState appState) => AppState = appState;

        public AppState AppState
        {
            get => _appState ?? throw new Exception("Can't use this Read class before setting AppState");
            protected set => _appState = value;
        }
        private AppState _appState;


        public IList<MetadataRecommendation> GetAllowedRecommendations(int targetTypeId, string key, string recommendedTypeName = null)
        {
            var recommendations = GetRecommendations(targetTypeId, key, recommendedTypeName);

            var remaining = recommendations
                .Select(r =>
                {
                    var (approved, featureId) = _requirements.Value.RequirementMet(r.Type.Metadata);
                    r.Enabled = approved;
                    if (!approved && !IsNullOrWhiteSpace(featureId))
                        r.MissingFeature = featureId;
                    return r;
                })
                .Where(r => r.PushToUi)
                .ToList();
            return remaining;
        }


        public IEnumerable<MetadataRecommendation> GetRecommendations(int targetTypeId, string key,
            string recommendedTypeName = null
        ) => Log.Func<IEnumerable<MetadataRecommendation>>($"targetType: {targetTypeId}", l =>
        {
            // Option 1. Specified typeName
            // If a specific contentType was given, that's the only thing we'll recommend
            // This is the case for a Permissions dialog
            if (!IsNullOrWhiteSpace(recommendedTypeName))
            {
                var recommendedType = AppState.GetContentType(recommendedTypeName);
                if (recommendedType == null) return (null, "type name not found");
                return (
                    new[] { new MetadataRecommendation(recommendedType, null, -1, "Use preset type", PrioMax) },
                    "use existing name");
            }

            // Option 2. A specific Target exists, and we want to find all options
            // 2.1 Check if we know this target type
            // Only support TargetType which is a predefined
            // Since we only support the metadata-dialog to edit content-type, app, etc. and not anything random/custom
            if (targetTypeId < 0 || !Enum.IsDefined(typeof(TargetTypes), targetTypeId))
                return (null, "invalid target type");

            // 2.2 Ask the target if it knows of expected types using `MetadataExpected`
            // Check if this object-type has a specific list of Content-Types which it expects
            // For example a attribute which says "I want this kind of Metadata"
            // Not fully worked out yet...
            // TODO #metadata
            var attachedRecommendations = GetTargetsExpectations(targetTypeId, key)
                                          ?? new List<MetadataRecommendation>();

            // 2.3 Find Content-Types marked with `MetadataFor` this specific target
            // For example Types which are marked to decorate an App
            var typesForTheTarget = TypesWhichDeclareTheyAreForTheTarget(targetTypeId, key);
            var initialTypes = typesForTheTarget?
                .Select(set =>
                    new MetadataRecommendation(set.Type, set.Decorator, null, "Self-Declaring", PrioMedium))
                .ToList() ?? new List<MetadataRecommendation>();

            attachedRecommendations.AddRange(initialTypes);

            var distinct = attachedRecommendations.OrderByDescending(ar => ar.Priority).Distinct().ToList();

            return (distinct, $"final: {distinct.Count}");
        });

        private class RecommendationInfos
        {
            public IContentType Type;
            public MetadataForDecorator Decorator;
        }

        private List<RecommendationInfos> TypesWhichDeclareTheyAreForTheTarget(int targetType, string targetKey) => Log.Func(l =>
        {
            // for type/attribute path comparisons, make sure we have the slashes cleaned
            var keyForward = (targetKey ?? "").ForwardSlash().Trim();

            // Do this #StepByStep to better debug in case of issues
            var allTypes = AppState.ContentTypes;
            var recommendedTypes = allTypes
                .SelectMany(ct =>
                {
                    // 2022-09-29 2dm - we have an issue here where the metadata must be fully loaded from app state
                    // and it's ServiceProvider is dead at that time, trying to debug
                    try
                    {
                        var allForDecors = ct.Metadata
                            .OfType(MetadataForDecorator.ContentTypeNameId)
                            .Select(e => new MetadataForDecorator(e))
                            .ToList();
                        var allForThisTargetType = allForDecors
                            .Where(dec => dec.TargetType == targetType)
                            .ToList();
                        l.A($"Found {allForDecors.Count} {nameof(MetadataForDecorator)}s " +
                            $"of which {allForThisTargetType.Count} for targetType {targetType} on {ct.Name}");
                        return allForThisTargetType.Select(decorator => new RecommendationInfos
                        {
                            Type = ct,
                            Decorator = decorator
                        });
                    }
                    catch (Exception e)
                    {
                        l.A($"Error on {ct.Name} ({ct.NameId}), will skip this");
                        l.Ex(e);
                        return Array.Empty<RecommendationInfos>();
                    }
                })
                .ToList();

            // Filter out these without recommendations
            recommendedTypes = recommendedTypes
                .Where(set => set?.Decorator != null)
                .ToList();
            l.A($"Found {recommendedTypes.Count} recommended Types");

            recommendedTypes = recommendedTypes
                .Where(set =>
                {
                    var targetName = set.Decorator.TargetName;
                    switch (targetType)
                    {
                        case (int)TargetTypes.Undefined:
                        case (int)TargetTypes.None:
                            return false;
                        case (int)TargetTypes.Attribute:
                            if (targetName.EqualsInsensitive(targetKey)) return true;
                            var attr = AppState.FindAttribute(targetKey);
                            return keyForward.EqualsInsensitive($"{attr.Item1.NameId}/{attr.Item2.Name}")
                                   || keyForward.EqualsInsensitive($"{attr.Item1.Name}/{attr.Item2.Name}");
                        // App and ContentType don't need extra conditions / specifiers
                        case (int)TargetTypes.App:
                            return true;
                        case (int)TargetTypes.Entity:
                            // True if the decorator says it's for all entities
                            if (targetName == "*") return true;
                            // Test if the current item (targetKey) is the expected type
                            if (!Guid.TryParse(targetKey, out var guidKey)) return false;
                            var currentEntity = AppState.List.One(guidKey);
                            return currentEntity?.Type?.Is(targetName) ?? false;
                        // App and ContentType don't need extra conditions / specifiers
                        case (int)TargetTypes.ContentType:
                        case (int)TargetTypes.Zone:
                            return true;
                        // todo: not handled yet #metadata
                        case (int)TargetTypes.CmsItem:
                            return false;
                        default:
                            return false;
                    }
                })
                .ToList();

            return (recommendedTypes, $"{recommendedTypes.Count}");
        });

        private List<MetadataRecommendation> GetTargetsExpectations(int targetType, string key) => Log.Func($"targetType: {targetType}", () =>
        {
            switch ((TargetTypes)targetType)
            {
                case TargetTypes.Undefined:
                case TargetTypes.None:
                    return (null, "no target");
                case TargetTypes.Attribute:
                    // TODO - PROBABLY TRY TO FIND THE ATTRIBUTE
                    return (null, "attributes not supported ATM");
                case TargetTypes.App:
                    // TODO: this won't work - needs another way of finding assignments
                    return (GetMetadataExpectedDecorators(AppState.Metadata, 0, "attached to App", PrioMax), "app");
                case TargetTypes.Entity:
                    if (!Guid.TryParse(key, out var guidKey)) return (null, "entity not guid");
                    var entity = AppState.List.One(guidKey);
                    if (entity == null) return (null, "entity not found");
                    var onEntity = GetMetadataExpectedDecorators(entity.Metadata, (int)TargetTypes.Entity, "attached to Entity", PrioMax)
                        ?? new List<MetadataRecommendation>();

                    // Now also ask the content-type for MD related to this
                    var onEntType = GetMetadataExpectedDecorators(entity.Type.Metadata, (int)TargetTypes.Entity, "attached to entity-type", PrioHigh);
                    var merged = onEntity.Union(onEntType).ToList();
                    return (merged, $"entity {onEntity.Count} type {onEntType.Count} all {merged.Count}");
                case TargetTypes.ContentType:
                    var ct = AppState.GetContentType(key);
                    if (ct == null) return (null, "type not found");
                    var onType = GetMetadataExpectedDecorators(ct.Metadata, (int)TargetTypes.ContentType, "attached to Content-Type", PrioHigh);
                    return (onType, "content type");
                case TargetTypes.Zone:
                case TargetTypes.CmsItem:
                    // todo: maybe improve?
                    return (null, "zone or CmsObject not supported");
            }
            return (new List<MetadataRecommendation>(), "ok");
        });

        /// <summary>
        /// Will get the MetadataExpected decorators of a target.
        /// </summary>
        /// <param name="md">MD List</param>
        /// <param name="meantFor">What it's for - to differentiate between MD on a Type which is for the type, or for the items of that type</param>
        /// <param name="debug"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private List<MetadataRecommendation> GetMetadataExpectedDecorators(IMetadataOf md, int meantFor, string debug, int priority) => Log.Func(l =>
        {
            var all = md.OfType(MetadataExpectedDecorator.ContentTypeNameId).ToList();
            if (meantFor > 0) all = all.Where(r => meantFor == new MetadataForDecorator(r).TargetType).ToList();
            if (!all.Any()) return (new List<MetadataRecommendation>(), "no recommendations");

            var resultAll = all.SelectMany(rEntity =>
                {
                    var rec = new MetadataExpectedDecorator(rEntity);
                    var config = rec.Types;
                    var delWarning = rec.DeleteWarning;
                    if (IsNullOrWhiteSpace(config))
                    {
                        l.W("Found case with no values in config");
                        return new List<MetadataRecommendation>();
                    }

                    return config
                        .Split(',')
                        .Select(name => TypeAsRecommendation(name.Trim(), debug, priority, delWarning))
                        .Where(x => x != null)
                        .ToList();
                })
                .ToList();

            return (resultAll, $"found {resultAll.Count}");
        });

        /// <summary>
        /// Find a content-type and convert it into a recommendation object
        /// </summary>
        /// <returns></returns>
        private MetadataRecommendation TypeAsRecommendation(string name, string debug, int priority, string delWarning) => Log.Func(() =>
        {
            if (IsNullOrWhiteSpace(name)) return (null, "empty name");

            var type = AppState.GetContentType(name);
            return type == null
                ? (null, "name not found")
                : (new MetadataRecommendation(type, null, null, debug, priority)
                    { DeleteWarning = delWarning }, "use existing name");
        });

    }
}
