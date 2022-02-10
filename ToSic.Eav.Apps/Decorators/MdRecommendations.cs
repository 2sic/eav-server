using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.AppMetadata;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Helpers;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using static System.Array;
using static ToSic.Eav.Apps.AppMetadata.MetadataRecommendation;
using static ToSic.Eav.Apps.Decorators.MetadataForDecorator;
using static ToSic.Eav.Metadata.Decorators;

namespace ToSic.Eav.Apps.Decorators
{
    // WIP v13 2dm - trying to simplify Read-Helpers to not be part of the large construct but a simple standalone read service
    public partial class MdRecommendations: ReadBase<MdRecommendations>
    {
        public MdRecommendations(): base($"{AppConstants.LogName}.MdRead")
        { }



        public IEnumerable<MetadataRecommendation> GetRecommendations(int targetTypeId, string key, string recommendedTypeName = null)
        {
            var wrapLog = Log.Call<IEnumerable<MetadataRecommendation>>($"targetType: {targetTypeId}");

            // Option 1. Specified typeName
            // If a specific contentType was given, that's the only thing we'll recommend
            // This is the case for a Permissions dialog
            if (!string.IsNullOrWhiteSpace(recommendedTypeName))
            {
                var recommendedType = AppState.GetContentType(recommendedTypeName);
                if (recommendedType == null) return wrapLog("type name not found", null);
                return wrapLog("use existing name", new[] { new MetadataRecommendation(recommendedType, null, -1, "Use preset type", PrioMax) });
            }

            // Option 2. A specific Target exists, and we want to find all options
            // 2.1 Check if we know this target type
            // Only support TargetType which is a predefined
            // Since we only support the metadata-dialog to edit content-type, app, etc. and not anything random/custom
            if (targetTypeId < 0 || !Enum.IsDefined(typeof(TargetTypes), targetTypeId))
                return wrapLog("invalid target type", null);

            // 2.2 Find Content-Types marked with `MetadataFor` this specific target
            // For example Types which are marked to decorate an App
            var initialTypes =
                (TypesWhichDeclareTheyAreForTheTarget(targetTypeId, key) ?? new List<(IContentType Type, IEntity Recommendation)>())
                .Select(set => new MetadataRecommendation(set.Type, set.Recommendation, 1, "Self-Declaring", PrioMedium));

            // 2.3 Ask the target if it knows of expected types using `MetadataExpected`
            // Check if this object-type has a specific list of Content-Types which it expects
            // For example a attribute which says "I want this kind of Metadata"
            // Not fully worked out yet...
            // TODO #metadata
            var attachedRecommendations = GetTargetsExpectations(targetTypeId, key)
                ?? new List<MetadataRecommendation>();

            attachedRecommendations.AddRange(initialTypes);

            var distinct = attachedRecommendations.OrderByDescending(ar => ar.Priority).Distinct();

            return wrapLog("unknown case", distinct);
        }

        public List<(IContentType Type, IEntity Recommendation)> TypesWhichDeclareTheyAreForTheTarget(int targetType, string key)
        {
            var wrapLog = Log.Call<List<(IContentType, IEntity)>>();
            // for path comparisons, make sure we have the slashes cleaned
            var keyForward = (key ?? "").ForwardSlash().Trim();

            // Do this #StepByStep to better debug in case of issues
            var allTypes = AppState.ContentTypes;
            var recommendedTypes = allTypes
                .Select(ct =>
                {
                    var decor = ct.Metadata
                        .OfType(MetadataForDecoratorId)
                        .FirstOrDefault(dec => dec.GetBestValue<int>(MetadataForTargetTypeField, Empty<string>()) == targetType);
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

                    var targetName = decor.GetBestValue<string>(MetadataForTargetNameField, Empty<string>()) ?? "";

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

            return wrapLog($"{result.Count}", result);
        }

        public List<MetadataRecommendation> GetTargetsExpectations(int targetType, string key)
        {
            var wrapLog = Log.Call<List<MetadataRecommendation>>($"targetType: {targetType}");

            switch ((TargetTypes)targetType)
            {
                case TargetTypes.Undefined:
                case TargetTypes.None:
                    return wrapLog("no target", null);
                case TargetTypes.Attribute:
                    // TODO - PROBABLY TRY TO FIND THE ATTRIBUTE
                    return wrapLog("attributes not supported ATM", null);
                case TargetTypes.App:
                    // TODO: this won't work - needs another way of finding assignments
                    return wrapLog("app", GetExpected(AppState.Metadata, 0, "attached to App", PrioMax));
                case TargetTypes.Entity:
                    if (!Guid.TryParse(key, out var guidKey)) return wrapLog("entity not guid", null);
                    var entity = AppState.List.One(guidKey);
                    if (entity == null) return wrapLog("entity not found", null);
                    var onEntity = GetExpected(entity.Metadata, (int)TargetTypes.Entity, "attached to Entity", PrioMax)
                        ?? new List<MetadataRecommendation>();

                    // Now also ask the content-type for MD related to this
                    var onEntType = GetExpected(entity.Type.Metadata, (int)TargetTypes.Entity, "attached to entity-type", PrioHigh);
                    var merged = onEntity.Union(onEntType).ToList();
                    return wrapLog($"entity {onEntity.Count} type {onEntType.Count} all {merged.Count}", merged);
                case TargetTypes.ContentType:
                    var ct = AppState.GetContentType(key);
                    if (ct == null) return wrapLog("type not found", null);
                    var onType = GetExpected(ct.Metadata, (int)TargetTypes.ContentType, "attached to Content-Type", PrioHigh);
                    return wrapLog("content type", onType);
                case TargetTypes.Zone:
                case TargetTypes.CmsItem:
                    // todo: maybe improve?
                    return wrapLog("zone or CmsObject not supported", null);
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
        private List<MetadataRecommendation> GetExpected(IMetadataOf md, int meantFor, string debug, int priority)
        {
            var wrapLog = Log.Call<List<MetadataRecommendation>>();

            var allRecs = md.OfType(MetadataExpectedDecorator.MetadataExpectedDecoratorId).ToList();
            if (meantFor > 0) allRecs = allRecs.Where(r => meantFor == r.GetBestValue<int>(MetadataForTargetTypeField, Empty<string>())).ToList();
            if (!allRecs.Any()) return wrapLog("no recommendations", new List<MetadataRecommendation>());

            var resultAll = allRecs.SelectMany(rec =>
            {
                var config = rec.GetBestValue<string>(MetadataExpectedDecorator.MetadataExpectedTypesField, Empty<string>());
                var delWarning = rec.GetBestValue<string>(MetadataForDeleteWarningField, Empty<string>());
                if (string.IsNullOrWhiteSpace(config)) return wrapLog("no values in config", null);
                return config
                    .Split(',')
                    .Select(name => TypeAsRecommendation(name.Trim(), 1, debug, priority, delWarning))
                    .Where(x => x != null)
                    .ToList();
            }).ToList();

            return wrapLog($"found {resultAll.Count}", resultAll);
        }

        /// <summary>
        /// Find a content-type and convert it into a recommendation object
        /// </summary>
        /// <returns></returns>
        private MetadataRecommendation TypeAsRecommendation(string name, int count, string debug, int priority, string delWarning)
        {
            var wrapLog = Log.Call<MetadataRecommendation>(name);

            if (string.IsNullOrWhiteSpace(name)) return wrapLog("empty name", null);

            var type = AppState.GetContentType(name);
            return type == null
                ? wrapLog("name not found", null)
                : wrapLog("use existing name",
                    new MetadataRecommendation(type, null, count, debug, priority)
                        { DeleteWarning = delWarning });
        }

    }
}
