using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Helpers;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using static System.String;
using static ToSic.Eav.Apps.Internal.MetadataDecorators.MetadataRecommendation;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

/// <summary>
/// Figure out all the recommendations for a Metadata Target
///
/// Note: this uses a new model for such helpers, which is different from the previous solution.
/// We're trying to simplify Read-Helpers to not be part of the large construct but a simple standalone read service
///
/// If this model works well, we'll probably reconsider how our xxxRead objects work
/// </summary>
/// <remarks>new in v13.02</remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MdRecommendations(LazySvc<MdRequirements> requirements, GenWorkPlus<WorkInputTypes> inputTypes)
    : ServiceBase($"{AppConstants.LogName}.MdRead", connect: [requirements])
{
    public void Setup(IAppReader appReader, int appId)
    {
        _appReader = appReader;
        AppId = appId;
    }

    private IAppReader AppReader => _appReader ?? throw new("Can't use this Read class before setting AppState");
    private IAppReader _appReader;

    private int AppId;


    public IList<MetadataRecommendation> GetAllowedRecommendations(int targetTypeId, string key, string recommendedTypeName = null)
    {
        var recommendations = GetRecommendations(targetTypeId, key, recommendedTypeName);

        var remaining = recommendations
            .Select(r =>
            {
                var (approved, featureId) = requirements.Value.RequirementMet(r.Type.Metadata);
                r.Enabled = approved;
                if (!approved && !IsNullOrWhiteSpace(featureId))
                    r.MissingFeature = featureId;
                return r;
            })
            .Where(r => r.PushToUi)
            .ToList();
        return remaining;
    }


    public IEnumerable<MetadataRecommendation> GetRecommendations(int targetTypeId, string key, string reqTypeName = null)
    {
        var l = Log.Fn<IEnumerable<MetadataRecommendation>>($"targetType: {targetTypeId}");
        // Option 1. Specified typeName
        // If a specific contentType was given, that's the only thing we'll recommend
        // This is the case for a Permissions dialog
        if (!IsNullOrWhiteSpace(reqTypeName))
        {
            var recommendedType = AppReader.GetContentType(reqTypeName);
            if (recommendedType == null) return l.ReturnNull("type name not found");
            return l.Return(
                [new(recommendedType, null, -1, "Use preset type", PrioMax)],
                "use existing name");
        }

        // Option 2. A specific Target exists, and we want to find all options
        // 2.1 Check if we know this target type
        // Only support TargetType which is a predefined
        // Since we only support the metadata-dialog to edit content-type, app, etc. and not anything random/custom
        if (targetTypeId < 0 || !Enum.IsDefined(typeof(TargetTypes), targetTypeId))
            return l.ReturnNull("invalid target type");

        // 2.2 Ask the target if it knows of expected types using `MetadataExpected`
        // Check if this object-type has a specific list of Content-Types which it expects
        // For example a attribute which says "I want this kind of Metadata"
        // Not fully worked out yet...
        // TODO #metadata
        var attachedRecommendations = GetTargetsExpectations(targetTypeId, key)
                                      ?? [];

        // 2.3 Find Content-Types marked with `MetadataFor` this specific target
        // For example Types which are marked to decorate an App
        var typesForTheTarget = TypesWhichDeclareTheyAreForTheTarget(targetTypeId, key);
        var initialTypes = typesForTheTarget?
            .Select(set =>
                new MetadataRecommendation(set.Type, set.Decorator, null, "Self-Declaring", PrioMedium))
            .ToList() ?? [];

        attachedRecommendations.AddRange(initialTypes);

        var distinct = attachedRecommendations.OrderByDescending(ar => ar.Priority).Distinct().ToList();

        return l.Return(distinct, $"final: {distinct.Count}");
    }

    private class RecommendationInfos
    {
        public IContentType Type;
        public MetadataForDecorator Decorator;
    }

    private List<RecommendationInfos> TypesWhichDeclareTheyAreForTheTarget(int targetType, string targetKey)
    {
        var l = Log.Fn<List<RecommendationInfos>>();
        // for type/attribute path comparisons, make sure we have the slashes cleaned
        var keyForward = (targetKey ?? "").ForwardSlash().Trim();

        // Do this #StepByStep to better debug in case of issues
        var allTypes = AppReader.ContentTypes;
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
                    return [];
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
                        var attr = AppReader.ContentTypes.FindAttribute(targetKey);
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
                        var currentEntity = AppReader.List.One(guidKey);
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

        return l.Return(recommendedTypes, $"{recommendedTypes.Count}");
    }

    private List<MetadataRecommendation> GetTargetsExpectations(int targetType, string key)
    {
        var l = Log.Fn<List<MetadataRecommendation>>($"targetType: {targetType}");
        switch ((TargetTypes)targetType)
        {
            case TargetTypes.Undefined:
            case TargetTypes.None:
                return l.ReturnNull("no target");
            case TargetTypes.Attribute:
                if (!int.TryParse(key, out var attributeId))
                    return l.ReturnNull("attribute: key is not int");

                var attribute = AppReader.ContentTypes
                    .SelectMany(ct => ct.Attributes)
                    .FirstOrDefault(attr => attr.AttributeId == attributeId);

                if (attribute == null)
                    return l.ReturnNull($"attribute '{attributeId}' not found");

                // figure out what field input type it is
                var inputType = attribute.InputType();

                // Find the input type definition
                var inputTypeDef = inputTypes.New(AppId).GetInputTypes()
                    .FirstOrDefault(it => it.Type.EqualsInsensitive(inputType));

                if (inputTypeDef == null)
                    return l.ReturnNull($"input type '{inputType}' not found");

                var attrMdOnField = GetMetadataExpectedDecorators(inputTypeDef.Metadata, TargetTypes.Attribute, "attached to Attribute", PrioHigh);


                return l.Return(attrMdOnField);
            case TargetTypes.App:
                // TODO: this won't work - needs another way of finding assignments
                return l.Return(GetMetadataExpectedDecorators(AppReader.Specs.Metadata, TargetTypes.Undefined, "attached to App", PrioMax), "app");
            case TargetTypes.Entity:
                if (!Guid.TryParse(key, out var guidKey)) return l.ReturnNull("entity not guid");
                var entity = AppReader.List.One(guidKey);
                if (entity == null) return l.ReturnNull("entity not found");
                var onEntity = GetMetadataExpectedDecorators(entity.Metadata, TargetTypes.Entity, "attached to Entity", PrioMax)
                               ?? [];

                // Now also ask the content-type for MD related to this
                // So we ask the Type Metadata for all recommendations for Entities (since it will be attached to the entity of this type)
                var onEntType = GetMetadataExpectedDecorators(entity.Type.Metadata, TargetTypes.Entity, "attached to entity-type", PrioHigh);
                var merged = onEntity.Union(onEntType).ToList();
                return l.Return(merged, $"entity {onEntity.Count} type {onEntType.Count} all {merged.Count}");
            case TargetTypes.ContentType:
                var ct = AppReader.GetContentType(key);
                if (ct == null) return l.ReturnNull("type not found");
                var onType = GetMetadataExpectedDecorators(ct.Metadata, TargetTypes.ContentType, "attached to Content-Type", PrioHigh);
                return l.Return(onType, "content type");
            case TargetTypes.Zone:
            case TargetTypes.CmsItem:
                // todo: maybe improve?
                return l.ReturnNull("zone or CmsObject not supported");
        }
        return l.Return([], "ok");
    }

    /// <summary>
    /// Will get the MetadataExpected decorators of a target.
    /// </summary>
    /// <param name="md">MD List</param>
    /// <param name="meantFor">What it's for - to differentiate between MD on a Type which is for the type, or for the items of that type</param>
    /// <param name="debug"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    private List<MetadataRecommendation> GetMetadataExpectedDecorators(IMetadataOf md, TargetTypes targetTypeFor, string debug, int priority)
    {
        var l = Log.Fn<List<MetadataRecommendation>>($"for {targetTypeFor}");

        // meta data is sometimes null
        if (md is null)
            return l.Return([], "null metadata");

        var all = md
            .OfType(MetadataExpectedDecorator.ContentTypeNameId)
            .ToList();

        // var meantFor = (int)targetTypeFor;
        if (targetTypeFor > 0)
            all = all
                .Where(r => (int)targetTypeFor == new MetadataForDecorator(r).TargetType)
                .ToList();
        if (!all.Any())
            return l.Return([], "no recommendations");

        var resultAll = all
            .SelectMany(rEntity =>
            {
                var rec = new MetadataExpectedDecorator(rEntity);
                var config = rec.Types;
                var delWarning = rec.DeleteWarning;
                return IsNullOrWhiteSpace(config)
                    ? l.Return([], "Found case with no values in config")
                    : config
                        .CsvToArrayWithoutEmpty()
                        .Select(name => TypeAsRecommendation(name, debug, priority, delWarning))
                        .Where(x => x != null)
                        .ToList();
            })
            .ToList();

        return l.Return(resultAll, $"found {resultAll.Count}");
    }

    /// <summary>
    /// Find a content-type and convert it into a recommendation object
    /// </summary>
    /// <returns></returns>
    private MetadataRecommendation TypeAsRecommendation(string name, string debug, int priority, string delWarning)
    {
        var l = Log.Fn<MetadataRecommendation>($"name: {name}");
        if (IsNullOrWhiteSpace(name)) return l.ReturnNull("empty name");

        var type = AppReader.GetContentType(name);
        return type == null
            ? l.ReturnNull("name not found")
            : l.Return(new(type, null, null, debug, priority)
                { DeleteWarning = delWarning }, "use existing name");
    }

}