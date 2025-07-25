﻿using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Build;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;
using ToSic.Eav.Persistence.Efc.Sys.Values;
using ToSic.Eav.Serialization;
using static System.StringComparer;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class EntityBuildHelper
{
    internal static IEntity BuildNewEntity(
        DataBuilder dataBuilder,
        IAppReader appReader,
        TempEntity e,
        IDataDeserializer serializer,
        Dictionary<int, ICollection<TempRelationshipList>> relatedEntities,
        Dictionary<int, ICollection<TempAttributeWithValues>> attributes,
        string primaryLanguage)
    {

        if (e.Json != null)
        {
            var fromJson = serializer.Deserialize(e.Json, false, true);
            // add properties which are not in the json
            // ReSharper disable once PossibleNullReferenceException
            var clonedExtended = dataBuilder.Entity.CreateFrom(fromJson,
                isPublished: e.IsPublished,
                created: e.Created,
                modified: e.Modified,
                owner: e.Owner
            );
            return clonedExtended;
        }

        var contentType = appReader.GetContentTypeRequired(e.ContentTypeId);

        // Prepare relationships to add to AttributeGenerator
        var emptyValueList = new List<(string StaticName, IValue)>();
        var stateCache = appReader.GetCache();
        var preparedRelationships = relatedEntities.TryGetValue(e.EntityId, out var rawRels)
            ? rawRels.Select(r => (r.StaticName, dataBuilder.Value.Relationship(r.Children, stateCache))).ToList()
            : emptyValueList;

        var attributeValuesLookup = !attributes.TryGetValue(e.EntityId, out var attribValues)
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

        // Get all Attributes of that Content-Type
        var newAttributes = dataBuilder.Attribute.Create(contentType, mergedValueLookups);
        var partsBuilder = EntityPartsLazy.ForAppAndOptionalMetadata(source: stateCache, metadata: null);
        var newEntity = dataBuilder.Entity.Create(
            appId: appReader.AppId,
            guid: e.EntityGuid,
            entityId: e.EntityId,
            repositoryId: e.EntityId,
            metadataFor: e.MetadataFor,
            contentType: contentType,
            isPublished: e.IsPublished,
            created: e.Created,
            modified: e.Modified,
            owner: e.Owner,
            version: e.Version,
            attributes: newAttributes,
            partsBuilder: partsBuilder
        );

        return newEntity;
    }
}