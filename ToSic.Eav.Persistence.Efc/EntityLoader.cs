using ToSic.Eav.Data.Build;
using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Persistence.Efc;

internal class EntityLoader(EfcAppLoader appLoader, Generator<IDataDeserializer> dataDeserializer, DataBuilder dataBuilder) : HelperBase(appLoader.Log, "Efc.EntLdr")
{
    public const int IdChunkSize = 5000;
    public const int MaxLogDetailsCount = 250;

    internal int AddLogCount;

    internal EntityQueries EntityQueries => field ??= new(appLoader.Context, Log);


    internal TimeSpan LoadEntities(IAppStateBuilder builder, CodeRefTrail codeRefTrail, int[] entityIds = null)
    {
        codeRefTrail.WithHere();
        var l = Log.Fn<TimeSpan>($"{builder.Reader.AppId}, {entityIds?.Length ?? 0}", timer: true);
        AddLogCount = 0; // reset, so anything in this call will be logged again up to 1000 entries
        var appId = builder.Reader.AppId;

        #region Prepare & Extend EntityIds

        // ensure not null
        entityIds ??= [];

        var filterByEntityIds = entityIds.Any();

        // if the app already exists and is being reloaded, remove all existing data
        if (!filterByEntityIds)
            builder.RemoveAllItems();

        // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
        var sqlTime = Stopwatch.StartNew();
        if (filterByEntityIds)
            entityIds = new PublishingHelper(appLoader).AddEntityIdOfPartnerEntities(entityIds);
        sqlTime.Stop();

        #endregion

        #region Get Entities with Attribute-Values from Database

        sqlTime.Start();
        var rawEntities = LoadRaw(appId, entityIds);
        sqlTime.Stop();

        var detailsLoadSpecs = new EntityDetailsLoadSpecs(appId, rawEntities, appLoader.Features, Log);

        var relLoader = new RelationshipLoader(appLoader, detailsLoadSpecs);
        var relatedEntities = relLoader.LoadRelationships();
        codeRefTrail.AddMessage($"Raw entities: {rawEntities.Count}");

        // load attributes & values
        var attributes = new ValueLoader(appLoader, detailsLoadSpecs).LoadValues();

        #endregion

        #region Build EntityModels

        var serializer = dataDeserializer.New();
        serializer.Initialize(builder.Reader);

        var entityTimer = Stopwatch.StartNew();
        foreach (var rawEntity in rawEntities)
        {
            if (AddLogCount++ == MaxLogDetailsCount)
                l.A($"Will stop logging each item now, as we've already logged {AddLogCount} items");

            var newEntity = EntityBuildHelper.BuildNewEntity(dataBuilder, builder.Reader, rawEntity, serializer, relatedEntities, attributes, appLoader.PrimaryLanguage);

            // If entity is a draft, also include references to Published Entity
            builder.Add(newEntity, rawEntity.PublishedEntityId, AddLogCount <= MaxLogDetailsCount);
        }

        entityTimer.Stop();
        l.A($"entities timer:{entityTimer.Elapsed}");

        #endregion

        //_sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
        //l.Done();
        return l.Return(sqlTime.Elapsed);
    }

    public List<TempEntity> LoadRaw(int appId, int[] entityIds, string filterType = null)
    {
        var l = Log.Fn<List<TempEntity>>($"app: {appId}, ids: {entityIds.Length}, {nameof(filterType)}: '{filterType}'", timer: true);

        var entitiesQuery = EntityQueries.EntitiesOfAppQuery(appId, entityIds, filterType);
        //var rawEntities = GetRawEntities(entitiesQuery);

        // TODO: @STV - THIS FAILS IN THE unit test .net 9 but not in .net 472 - why? Same EF .net 9.0.1 problem?
        var rawEntities = entitiesQuery
            .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
            .Select(e => new TempEntity
            {
                EntityId = e.EntityId,
                EntityGuid = e.EntityGuid,
                Version = e.Version,
                AttributeSetId = e.AttributeSetId,
                MetadataFor = new(e.AssignmentObjectTypeId, null, e.KeyString, e.KeyNumber, e.KeyGuid),
                IsPublished = e.IsPublished,
                PublishedEntityId = e.PublishedEntityId,
                Owner = e.Owner,
                Created = e.ChangeLogCreatedNavigation.Timestamp,
                Modified = e.ChangeLogModifiedNavigation.Timestamp,
                Json = e.Json,
            })
            .ToList();
        l.A($"Query executed and converted to {nameof(TempEntity)}");

        return l.Return(rawEntities, $"found: {rawEntities.Count}");
    }

    ///// <summary>
    ///// Load raw / intermediate entities from the database, without attributes/values.
    ///// </summary>
    ///// <param name="entitiesQuery"></param>
    ///// <returns></returns>
    //public List<TempEntity> GetRawEntities(IQueryable<ToSicEavEntities> entitiesQuery)
    //{
    //    var l = Log.Fn<List<TempEntity>>(timer: true);

    //    // TODO: @STV - THIS FAILS IN THE unit test .net 9 but not in .net 472 - why? Same EF .net 9.0.1 problem?
    //    var rawEntities = entitiesQuery
    //        .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
    //        .Select(e => new TempEntity
    //        {
    //            EntityId = e.EntityId,
    //            EntityGuid = e.EntityGuid,
    //            Version = e.Version,
    //            AttributeSetId = e.AttributeSetId,
    //            MetadataFor = new(e.AssignmentObjectTypeId, null, e.KeyString, e.KeyNumber, e.KeyGuid),
    //            IsPublished = e.IsPublished,
    //            PublishedEntityId = e.PublishedEntityId,
    //            Owner = e.Owner,
    //            Created = e.ChangeLogCreatedNavigation.Timestamp,
    //            Modified = e.ChangeLogModifiedNavigation.Timestamp,
    //            Json = e.Json,
    //        })
    //        .ToList();
    //    l.A($"Query executed and converted to {nameof(TempEntity)}");

    //    return l.Return(rawEntities, $"found: {rawEntities.Count}");
    //}

}