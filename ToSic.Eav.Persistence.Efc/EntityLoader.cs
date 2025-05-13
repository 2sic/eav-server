using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Persistence.Efc;

internal class EntityLoader(EfcAppLoader efcAppLoader, Generator<IDataDeserializer> dataDeserializer, DataBuilder dataBuilder, IEavFeaturesService featuresSvc)
    : HelperBase(efcAppLoader.Log, "Efc.EntLdr")
{
    public const int IdChunkSize = 5000;
    public const int MaxLogDetailsCount = 250;

    internal int AddLogCount;

    internal EntityQueries EntityQueries => field ??= new(efcAppLoader.Context, Log);


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
            entityIds = new PublishingHelper(efcAppLoader).AddEntityIdOfPartnerEntities(entityIds);
        sqlTime.Stop();

        #endregion

        #region Get Entities with Attribute-Values from Database

        sqlTime.Start();
        var rawEntities = LoadRaw(appId, entityIds);
        sqlTime.Stop();

        var detailsLoadSpecs = new EntityDetailsLoadSpecs(appId, rawEntities, featuresSvc, Log);

        var relLoader = new RelationshipLoader(efcAppLoader, detailsLoadSpecs);
        var relatedEntities = relLoader.LoadRelationships();
        codeRefTrail.AddMessage($"Raw entities: {rawEntities.Count}");

        // load attributes & values
        var attributes = new ValueLoader(efcAppLoader, detailsLoadSpecs, featuresSvc).LoadValues();

        #endregion

        #region Build EntityModels

        var serializer = dataDeserializer.New();
        serializer.Initialize(builder.Reader);
        serializer.ConfigureLogging(efcAppLoader.LogSettings);
        l.A($"🪵 Using LogSettings: {efcAppLoader.LogSettings}");

        var logDetails = efcAppLoader.LogSettings.Enabled && efcAppLoader.LogSettings.Details;

        var entityTimer = Stopwatch.StartNew();
        foreach (var rawEntity in rawEntities)
        {
            if (AddLogCount++ == MaxLogDetailsCount)
                l.A($"Will stop logging each item now, as we've already logged {AddLogCount} items");

            var newEntity = EntityBuildHelper.BuildNewEntity(dataBuilder, builder.Reader, rawEntity, serializer, relatedEntities, attributes, efcAppLoader.PrimaryLanguage);

            // If entity is a draft, also include references to Published Entity
            builder.Add(newEntity, rawEntity.PublishedEntityId, logDetails && AddLogCount <= MaxLogDetailsCount);
        }

        entityTimer.Stop();
        l.A($"entities timer:{entityTimer.Elapsed}");

        #endregion

        return l.Return(sqlTime.Elapsed);
    }

    public List<TempEntity> LoadRaw(int appId, int[] entityIds, string filterType = null)
    {
        var l = Log.Fn<List<TempEntity>>($"app: {appId}, ids: {entityIds.Length}, {nameof(filterType)}: '{filterType}'", timer: true);

        var entitiesQuery = EntityQueries.EntitiesOfAppQuery(appId, entityIds, filterType);

        // TODO: @STV - THIS FAILS IN THE unit test .net 9 but not in .net 472 - why? Same EF .net 9.0.1 problem?
        var rawEntities = entitiesQuery
            .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
            .Select(e => new TempEntity
            {
                EntityId = e.EntityId,
                EntityGuid = e.EntityGuid,
                Version = e.Version,
                ContentTypeId = e.ContentTypeId,
                MetadataFor = new(e.TargetTypeId, null, e.KeyString, e.KeyNumber, e.KeyGuid),
                IsPublished = e.IsPublished,
                PublishedEntityId = e.PublishedEntityId,
                Owner = e.Owner,
                Created = e.TransCreated.Timestamp,
                Modified = e.TransModified.Timestamp,
                Json = e.Json,
            })
            .ToList();
        l.A($"Query executed and converted to {nameof(TempEntity)}");

        return l.Return(rawEntities, $"found: {rawEntities.Count}");
    }

}