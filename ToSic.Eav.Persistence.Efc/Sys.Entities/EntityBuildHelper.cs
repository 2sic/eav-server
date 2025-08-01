using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Build;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;
using ToSic.Eav.Persistence.Efc.Sys.Values;
using ToSic.Eav.Serialization;
using static System.StringComparer;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class EntityBuildHelper
{
    internal static IEntity BuildNewEntity(DataBuilder dataBuilder,
        IAppReader appReader,
        TempEntity rawEntity,
        IDataDeserializer serializer,
        Dictionary<int, ICollection<TempRelationshipList>> relatedEntities,
        Dictionary<int, ICollection<TempAttributeWithValues>> attributes,
        string primaryLanguage, ILogCall<TimeSpan>? logCall)
    {

        if (rawEntity.Json != null)
        {
            var fromJson = serializer.Deserialize(rawEntity.Json, false, true);
            // add properties which are not in the json
            // ReSharper disable once PossibleNullReferenceException
            var clonedExtended = dataBuilder.Entity.CreateFrom(fromJson,
                isPublished: rawEntity.IsPublished,
                created: rawEntity.Created,
                modified: rawEntity.Modified,
                owner: rawEntity.Owner
            );
            return clonedExtended;
        }

        var contentType = appReader.GetContentTypeRequired(rawEntity.ContentTypeId);

        // Prepare relationships to add to AttributeGenerator
        var emptyValueList = new List<(string StaticName, IValue)>();
        var stateCache = appReader.GetCache();
        var preparedRelationships = relatedEntities.TryGetValue(rawEntity.EntityId, out var rawRels)
            ? rawRels.Select(r => (r.StaticName, dataBuilder.Value.Relationship(r.Children, stateCache))).ToList()
            : emptyValueList;

        var attributeValuesLookup = !attributes.TryGetValue(rawEntity.EntityId, out var attribValues)
            ? emptyValueList
            : attribValues
                .Select(a => new
                {
                    a.Name,
                    CtAttribute = contentType[a.Name],
                    a.Values
                })
                .Where(set => set.CtAttribute != null)
                .SelectMany(a =>
                {
                    var results = a.Values
                        .Select(v => dataBuilder.Value.Build(a.CtAttribute!.Type, v.Value, v.Languages))
                        .ToList();
                    var final = ValueLanguageRepairHelper.FixIncorrectLanguageDefinitions(results, primaryLanguage);
                    return final.Select(r => (a.Name, r));
                })
                .ToList();


        var mergedValueLookups = preparedRelationships
            .Concat(attributeValuesLookup)
            .ToLookup(x => x.Item1, x => x.Item2, InvariantCultureIgnoreCase);

        // 2025-08-01 #FinallyMakeEntityIdImmutable
        // Determine official EntityId, since our cache may need a different one in case of a draft
        var entityId = rawEntity.EntityId;
        if (rawEntity is { IsPublished: false, PublishedEntityId: not null } && rawEntity.PublishedEntityId != 0)
        {
            logCall.A($"map draft to published for new: {rawEntity.EntityId} on {entityId}");
            entityId = rawEntity.PublishedEntityId.Value;
        }

        // Get all Attributes of that Content-Type
        var newAttributes = dataBuilder.Attribute.Create(contentType, mergedValueLookups);
        var partsBuilder = EntityPartsLazy.ForAppAndOptionalMetadata(source: stateCache, metadata: null);
        var newEntity = dataBuilder.Entity.Create(
            appId: appReader.AppId,
            guid: rawEntity.EntityGuid,
            entityId: entityId, // rawEntity.EntityId,
            repositoryId: rawEntity.EntityId,
            metadataFor: rawEntity.MetadataFor,
            contentType: contentType,
            isPublished: rawEntity.IsPublished,
            created: rawEntity.Created,
            modified: rawEntity.Modified,
            owner: rawEntity.Owner,
            version: rawEntity.Version,
            attributes: newAttributes,
            partsBuilder: partsBuilder
        );

        return newEntity;
    }
}