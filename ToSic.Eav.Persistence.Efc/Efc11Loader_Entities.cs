using System.Diagnostics;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Generics;
using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Serialization;
using static System.StringComparer;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    private string _primaryLanguage;
    public string PrimaryLanguage
    {
        get {
            if (_primaryLanguage != null) return _primaryLanguage;
            var l = Log.Fn<string>(timer: true);
            _primaryLanguage = environmentLazy.Value.DefaultCultureCode.ToLowerInvariant();
            Log.A($"Primary language from environment (for attribute sorting): {_primaryLanguage}");
            return l.ReturnAndLog(_primaryLanguage);
        }
        set => _primaryLanguage = value;
    }

    public const int IdChunkSize = 5000;
    public const int MaxLogDetailsCount = 250;

    internal int AddLogCount;

    private void LoadEntities(IAppStateBuilder builder, CodeRefTrail codeRefTrail, int[] entityIds = null)
    {
        codeRefTrail.WithHere();
        var l = Log.Fn($"{builder.Reader.AppId}, {entityIds?.Length ?? 0}", timer: true);
        AddLogCount = 0; // reset, so anything in this call will be logged again up to 1000 entries
        var appId = builder.Reader.AppId;

        #region Prepare & Extend EntityIds

        entityIds ??= Array.Empty<int>();

        var filterByEntityIds = entityIds.Any();

        // if the app already exists and is being reloaded, remove all existing data
        if (!filterByEntityIds)
            builder.RemoveAllItems();

        // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
        var sqlTime = Stopwatch.StartNew();
        if (filterByEntityIds)
            entityIds = GetEntityIdOfPartnerEntities(entityIds);
        sqlTime.Stop();

        #endregion

        #region Get Entities with Attribute-Values from Database

        sqlTime.Start();
        var rawEntities = GetRawEntities(entityIds, appId, filterByEntityIds);
        sqlTime.Stop();
        var entityIdsFound = rawEntities
            .Select(e => e.EntityId)
            .ToList();
        var entityIdChunks = entityIdsFound.ChunkBy(IdChunkSize);
        l.A($"Found {entityIdsFound.Count} raw entities in {sqlTime.ElapsedMilliseconds}ms - chunked into {entityIdChunks.Count} chunks");

        // Load relationships in batches / chunks
        sqlTime.Start();
        var lRelationshipSql = Log.Fn("Relationship SQL", timer: true);
        var allChunks = entityIdChunks
            .Select(idList => GetRelationshipChunk(appId, idList))
            .SelectMany(chunk => chunk)
            .ToList();
        lRelationshipSql.Done();

        // in some strange cases we get duplicate keys - this should try to report what's happening
        var relatedEntities = GroupUniqueRelationships(allChunks);

        l.A($"Found {relatedEntities.Count} entity relationships in {sqlTime.ElapsedMilliseconds}ms");
        codeRefTrail.AddMessage($"Raw entities: {rawEntities.Count}");
        #region load attributes & values

        var chunkedAttributes = entityIdChunks.Select(GetAttributesOfEntityChunk);
        var attributes = chunkedAttributes
            .SelectMany(chunk => chunk)
            .ToDictionary(i => i.Key, i => i.Value);
        l.A($"Found {attributes.Count} attributes");

        #endregion

        sqlTime.Stop();

        #endregion

        #region Build EntityModels

        var serializer = dataDeserializer.New();
        serializer.Initialize(builder.Reader);

        var entityTimer = Stopwatch.StartNew();
        foreach (var rawEntity in rawEntities)
        {
            if (AddLogCount++ == MaxLogDetailsCount)
                l.A($"Will stop logging each item now, as we've already logged {AddLogCount} items");

            var newEntity = BuildNewEntity(builder.Reader, rawEntity, serializer, relatedEntities, attributes, PrimaryLanguage);

            // If entity is a draft, also include references to Published Entity
            builder.Add(newEntity, rawEntity.PublishedEntityId, AddLogCount <= MaxLogDetailsCount);
        }

        entityTimer.Stop();
        l.A($"entities timer:{entityTimer.Elapsed}");

        #endregion

        _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
        l.Done();
    }



    private IEntity BuildNewEntity(IAppReader appReader, TempEntity e, 
        IDataDeserializer serializer,
        Dictionary<int, IEnumerable<TempRelationshipList>> relatedEntities,
        Dictionary<int, IEnumerable<TempAttributeWithValues>> attributes,
        string primaryLanguage)
    {

        if (e.Json != null)
        {
            var fromJson = serializer.Deserialize(e.Json, false, true);
            // add properties which are not in the json
            // ReSharper disable once PossibleNullReferenceException
            //fromJson.IsPublished = e.IsPublished;
            //fromJson.Created = e.Created;
            //fromJson.Modified = e.Modified;
            //fromJson.Owner = e.Owner;
            var clonedExtended = dataBuilder.Entity.CreateFrom(fromJson,
                isPublished: e.IsPublished,
                created: e.Created,
                modified: e.Modified,
                owner: e.Owner
            );
            return clonedExtended; // fromJson;
        }

        var contentType = appReader.GetContentType(e.AttributeSetId);
        if (contentType == null)
            throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);

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
                        .Select(v => dataBuilder.Value.Build(a.CtAttribute.Type, v.Value, v.Languages))
                        .ToList();
                    var final = DataRepair.FixIncorrectLanguageDefinitions(results, primaryLanguage);
                    return final.Select(r => (a.Name, r));
                }).ToList();


        var mergedValueLookups = preparedRelationships
            .Concat(attributeValuesLookup)
            .ToLookup(x => x.Item1, x => x.Item2, InvariantCultureIgnoreCase);

        // Get all Attributes of that Content-Type
        var newAttributes = dataBuilder.Attribute.Create(contentType, mergedValueLookups);
        var partsBuilder = EntityPartsBuilder.ForAppAndOptionalMetadata(source: stateCache, metadata: null);
        var newEntity = dataBuilder.Entity.Create(
            appId: appReader.AppId,
            guid: e.EntityGuid, entityId: e.EntityId, repositoryId: e.EntityId,
            metadataFor: e.MetadataFor, 
            contentType: contentType, isPublished: e.IsPublished,
            created: e.Created, modified: e.Modified, 
            owner: e.Owner, version: e.Version, 
            attributes: newAttributes,
            partsBuilder: partsBuilder);

        return newEntity;
    }

       
}