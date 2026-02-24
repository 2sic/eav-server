using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;
using ToSic.Eav.Persistence.Efc.Sys.Values;
using ToSic.Eav.Serialization;
using ToSic.Sys.Documentation;
using static System.StringComparer;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class EntityBuildHelper(
    DataAssembler dataAssembler,
    ContentTypeAssembler typeAssembler,
    IAppReader appReader,
    IDataDeserializer serializer,
    Dictionary<int, ICollection<TempRelationshipList>> relatedEntities,
    Dictionary<int, ICollection<TempAttributeWithValues>> attributes,
    string primaryLanguage,
    ILog parentLog): HelperBase(parentLog, "Efc.EnBlHl")
{
    private int _errorId = int.MaxValue;

    internal IEntity BuildNewEntity(
        TempEntity rawEntity,
        ILogCall? l)
    {
        // First check if JSON persisted entity
        if (rawEntity.Json != null)
        {
            var fromJson = serializer.Deserialize(rawEntity.Json, false, true);
            // add properties which are not in the json
            // ReSharper disable once PossibleNullReferenceException
            var clonedExtended = dataAssembler.Entity.CreateFrom(fromJson,
                isPublished: rawEntity.IsPublished,
                created: rawEntity.Created,
                modified: rawEntity.Modified,
                owner: rawEntity.Owner
            );
            return clonedExtended;
        }

        var contentType = appReader.GetContentTypeOptional(rawEntity.ContentTypeId);

        // Add error entity if content type is missing.
        // This is really rare, but it happens.
        // Example: If an app had a content type and data for it
        // But later an extension with the same content-type was added from the file system
        // Then the original content-type from the DB is gone, but the data remains.
        // Before v21, this would completely break the app, as all entities of the app would fail to load.
        if (contentType == null)
        {
            l.A($"ContentTypeId {rawEntity.ContentTypeId} not found for EntityId {rawEntity.EntityId}");
            return CreateErrorEntity(
                title: "Content Type Missing",
                message: $"Content Type '{rawEntity.ContentTypeId}' not found in App {appReader.AppId}. " +
                         $"Entity {rawEntity.EntityId} could not be loaded."
            );
        }

        // Prepare relationships to add to AttributeGenerator
        var emptyValueList = new List<(string StaticName, IValue)>();
        var stateCache = appReader.GetCache();
        var valAss = dataAssembler.Value;
        var relAss = dataAssembler.Relationship;
        var preparedRelationships = relatedEntities.TryGetValue(rawEntity.EntityId, out var rawRels)
            ? rawRels
                .Select(r => (r.StaticName, relAss.Relationship(relAss.ToSource(r.Children, stateCache))))
                .ToList()
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
                        .Select(v => valAss.Create(a.CtAttribute!.Type, v.Value, v.Languages))
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
            l.A($"map draft to published for new: {rawEntity.EntityId} on {entityId}");
            entityId = rawEntity.PublishedEntityId.Value;
        }

        // Get all Attributes of that Content-Type
        var newAttributes = dataAssembler.AttributeList.CreateListForType(contentType, mergedValueLookups);
        var partsBuilder = dataAssembler.EntityConnection.UseApp(stateCache);
        var newEntity = dataAssembler.Entity.Create(
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

    [PrivateApi("usually not needed externally")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    private IEntity CreateErrorEntity(string? title, string? message)
    {
        var values = new Dictionary<string, object?>
        {
            { DataConstants.ErrorFieldTitle, $"Error: {title}" },
            { DataConstants.ErrorFieldMessage, message },
            { DataConstants.ErrorFieldDebugNotes, DataConstants.ErrorDebugMessage }
        };

        // #DebugDataSource
        // When debugging I usually want to see where this happens. Feel free to comment in/out as needed
        // System.Diagnostics.Debugger.Break();

        // Don't use the default data builder here, as it needs DI and this object
        // will often be created late when DI is already destroyed
        var id = _errorId--;
        var errorEntity = dataAssembler.Entity.Create(
            appId: appReader.AppId,
            entityId: id,
            repositoryId: id,
            contentType: ErrorContentType,
            attributes: dataAssembler.AttributeList.Finalize(values),
            titleField: DataConstants.ErrorFieldTitle
        );
        return errorEntity;
    }

    private IContentType ErrorContentType => field ??= typeAssembler.Type.Transient(DataConstants.ErrorTypeName);
}