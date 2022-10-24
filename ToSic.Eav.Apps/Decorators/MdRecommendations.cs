using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.DI;
using ToSic.Eav.Helpers;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
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
    public class MdRecommendations: ReadBase<MdRecommendations>
    {

        public MdRecommendations(LazyInitLog<MdRequirements> requirements): base($"{AppConstants.LogName}.MdRead")
        {
            _requirements = requirements.SetLog(Log);
        }
        private readonly LazyInitLog<MdRequirements> _requirements;


        public IList<MetadataRecommendation> GetAllowedRecommendations(int targetTypeId, string key, string recommendedTypeName = null)
        {
            var recommendations = GetRecommendations(targetTypeId, key, recommendedTypeName);

            var remaining = recommendations
                .Where(r => _requirements.Ready.RequirementMet(r.Type.Metadata))
                .ToList();
            return remaining;
        }
        

        public IEnumerable<MetadataRecommendation> GetRecommendations(int targetTypeId, string key, string recommendedTypeName = null)
        {
            var wrapLog = Log.Fn<IEnumerable<MetadataRecommendation>>($"targetType: {targetTypeId}");

            // Option 1. Specified typeName
            // If a specific contentType was given, that's the only thing we'll recommend
            // This is the case for a Permissions dialog
            if (!string.IsNullOrWhiteSpace(recommendedTypeName))
            {
                var recommendedType = AppState.GetContentType(recommendedTypeName);
                if (recommendedType == null) return wrapLog.ReturnNull("type name not found");
                return wrapLog.Return(new[] { new MetadataRecommendation(recommendedType, null, -1, "Use preset type", PrioMax) }, 
                    "use existing name");
            }

            // Option 2. A specific Target exists, and we want to find all options
            // 2.1 Check if we know this target type
            // Only support TargetType which is a predefined
            // Since we only support the metadata-dialog to edit content-type, app, etc. and not anything random/custom
            if (targetTypeId < 0 || !Enum.IsDefined(typeof(TargetTypes), targetTypeId))
                return wrapLog.ReturnNull("invalid target type");

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
                    new MetadataRecommendation(set.Type, set.Recommendation, null, "Self-Declaring", PrioMedium))
                .ToList() ?? new List<MetadataRecommendation>();

            attachedRecommendations.AddRange(initialTypes);

            var distinct = attachedRecommendations.OrderByDescending(ar => ar.Priority).Distinct().ToList();

            return wrapLog.Return(distinct, $"final: {distinct.Count}");
        }

        private List<(IContentType Type, IEntity Recommendation)> TypesWhichDeclareTheyAreForTheTarget(int targetType, string key)
        {
            var wrapLog = Log.Fn<List<(IContentType, IEntity)>>();
            // for path comparisons, make sure we have the slashes cleaned
            var keyForward = (key ?? "").ForwardSlash().Trim();

            // Do this #StepByStep to better debug in case of issues
            var allTypes = AppState.ContentTypes;
            var recommendedTypes = allTypes
                .Select(ct =>
                {
                    IEntity decor;
                    // 2022-09-29 2dm - we have an issue here where the metadata must be fully loaded from app state
                    // and it's ServiceProvider is dead at that time, trying to debug
                    try
                    {
                        decor = ct.Metadata
                            .OfType(ForDecorator.TypeGuid)
                            .FirstOrDefault(dec => new ForDecorator(dec).TargetType == targetType);
                    }
                    catch (Exception e)
                    {
                        Log.A($"Error on {ct.Name} ({ct.NameId}");
                        Log.Ex(e);
                        decor = null;
                    }
                    return new
                    {
                        Found = decor != null,
                        Type = ct,
                        Decorator = decor
                    };
                })
                .ToList();

            // Filter out these without recommendations
            recommendedTypes = recommendedTypes
                .Where(set => set.Found)
                .ToList();

            recommendedTypes = recommendedTypes
                .Where(set =>
                {
                    var decor = set.Decorator;
                    var decorNew = new ForDecorator(set.Decorator);

                    var targetName = decorNew.TargetName;

                    switch (targetType)
                    {
                        case (int)TargetTypes.Undefined:
                        case (int)TargetTypes.None:
                            return false;
                        case (int)TargetTypes.Attribute:
                            if (targetName.Equals(key, StringComparison.InvariantCultureIgnoreCase)) return true;
                            var attr = AppState.FindAttribute(key);
                            return keyForward.EqualsInsensitive(attr.Item1.NameId + "/" + attr.Item2.Name)
                                   || keyForward.EqualsInsensitive(attr.Item1.Name + "/" + attr.Item2.Name);
                        // App and ContentType don't need extra specifiers
                        case (int)TargetTypes.App: return true;
                        case (int)TargetTypes.Entity:
                            if (!Guid.TryParse(key, out var guidKey)) return false;
                            var entity = AppState.List.One(guidKey);
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
                .Select(set => (set.Type, set.Decorator))
                .ToList();

            return wrapLog.Return(result, $"{result.Count}");
        }

        private List<MetadataRecommendation> GetTargetsExpectations(int targetType, string key)
        {
            var wrapLog = Log.Fn<List<MetadataRecommendation>>($"targetType: {targetType}");

            switch ((TargetTypes)targetType)
            {
                case TargetTypes.Undefined:
                case TargetTypes.None:
                    return wrapLog.ReturnNull("no target");
                case TargetTypes.Attribute:
                    // TODO - PROBABLY TRY TO FIND THE ATTRIBUTE
                    return wrapLog.ReturnNull("attributes not supported ATM");
                case TargetTypes.App:
                    // TODO: this won't work - needs another way of finding assignments
                    return wrapLog.Return(GetMetadataExpectedDecorators(AppState.Metadata, 0, "attached to App", PrioMax), "app");
                case TargetTypes.Entity:
                    if (!Guid.TryParse(key, out var guidKey)) return wrapLog.ReturnNull("entity not guid");
                    var entity = AppState.List.One(guidKey);
                    if (entity == null) return wrapLog.ReturnNull("entity not found");
                    var onEntity = GetMetadataExpectedDecorators(entity.Metadata, (int)TargetTypes.Entity, "attached to Entity", PrioMax)
                        ?? new List<MetadataRecommendation>();

                    // Now also ask the content-type for MD related to this
                    var onEntType = GetMetadataExpectedDecorators(entity.Type.Metadata, (int)TargetTypes.Entity, "attached to entity-type", PrioHigh);
                    var merged = onEntity.Union(onEntType).ToList();
                    return wrapLog.Return(merged, $"entity {onEntity.Count} type {onEntType.Count} all {merged.Count}");
                case TargetTypes.ContentType:
                    var ct = AppState.GetContentType(key);
                    if (ct == null) return wrapLog.ReturnNull("type not found");
                    var onType = GetMetadataExpectedDecorators(ct.Metadata, (int)TargetTypes.ContentType, "attached to Content-Type", PrioHigh);
                    return wrapLog.Return(onType, "content type");
                case TargetTypes.Zone:
                case TargetTypes.CmsItem:
                    // todo: maybe improve?
                    return wrapLog.ReturnNull("zone or CmsObject not supported");
            }
            return new List<MetadataRecommendation>();

        }

        /// <summary>
        /// Will get the MetadataExpected decorators of a target.
        /// </summary>
        /// <param name="md">MD List</param>
        /// <param name="meantFor">What it's for - to differentiate between MD on a Type which is for the type, or for the items of that type</param>
        /// <param name="debug"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private List<MetadataRecommendation> GetMetadataExpectedDecorators(IMetadataOf md, int meantFor, string debug, int priority)
        {
            var wrapLog = Log.Fn<List<MetadataRecommendation>>();

            var all = md.OfType(ExpectedDecorator.TypeGuid).ToList();
            if (meantFor > 0) all = all.Where(r => meantFor == new ForDecorator(r).TargetType).ToList();
            if (!all.Any()) return wrapLog.Return(new List<MetadataRecommendation>(), "no recommendations");

            var resultAll = all.SelectMany(rEntity =>
            {
                var rec = new ExpectedDecorator(rEntity);
                var config = rec.Types;
                var delWarning = rec.DeleteWarning;
                if (string.IsNullOrWhiteSpace(config)) return wrapLog.ReturnNull("no values in config");
                return config
                    .Split(',')
                    .Select(name => TypeAsRecommendation(name.Trim(), debug, priority, delWarning))
                    .Where(x => x != null)
                    .ToList();
            }).ToList();

            return wrapLog.Return(resultAll, $"found {resultAll.Count}");
        }

        /// <summary>
        /// Find a content-type and convert it into a recommendation object
        /// </summary>
        /// <returns></returns>
        private MetadataRecommendation TypeAsRecommendation(string name, string debug, int priority, string delWarning)
        {
            var wrapLog = Log.Fn<MetadataRecommendation>(name);

            if (string.IsNullOrWhiteSpace(name)) return wrapLog.ReturnNull("empty name");

            var type = AppState.GetContentType(name);
            return type == null
                ? wrapLog.ReturnNull("name not found")
                : wrapLog.Return(new MetadataRecommendation(type, null, null, debug, priority)
                        { DeleteWarning = delWarning }, "use existing name");
        }

    }
}
