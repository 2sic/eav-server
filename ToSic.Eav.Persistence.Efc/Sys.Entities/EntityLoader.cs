using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Persistence.Efc.Sys.Relationships;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;
using ToSic.Eav.Persistence.Efc.Sys.Values;
using ToSic.Eav.Serialization;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class EntityLoader(EfcAppLoaderService appLoader, Generator<IDataDeserializer> dataDeserializer, DataBuilder dataBuilder, ISysFeaturesService featuresSvc)
    : HelperBase(appLoader.Log, "Efc.EntLdr")
{
    /// <summary>
    /// Default safe chunk size for any system.
    /// </summary>
    private const int RelationshipsIdChunkSizeDefault = 1000;       // todo: possibly reduce for "save default" once we know if there is a limit in SQL

    /// <summary>
    /// Higher chunk size for Enterprise, where we assume that the database is more powerful and can handle larger chunks.
    /// </summary>
    private const int RelationshipsIdChunkSizeEnterprise = 10000;   // FYI: We tried 25k as default, but seems that certain SQL servers had issues
    public const int MaxLogDetailsCount = 250;

    internal int AddLogCount;

    [field: AllowNull, MaybeNull]
    internal EntityQueries EntityQueries => field ??= new(appLoader.Context, appLoader.FeaturesService, Log);


    internal TimeSpan LoadEntities(IAppStateBuilder builder, CodeRefTrail codeRefTrail, int[] entityIds)
    {
        codeRefTrail.WithHere();
        var l = Log.IfSummary(appLoader.LogSettings)
            .Fn<TimeSpan>($"{builder.Reader.AppId}, {entityIds.Length}", timer: true);
        AddLogCount = 0; // reset, so anything in this call will be logged again up to 1000 entries
        var appId = builder.Reader.AppId;

        #region Prepare & Extend EntityIds

        var filterByEntityIds = entityIds.Any();

        // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
        var sqlTime = Stopwatch.StartNew();
        if (filterByEntityIds)
            entityIds = new PublishingHelper(appLoader).AddEntityIdOfPartnerEntities(entityIds);
        sqlTime.Stop();

        #endregion

        #region Get Entities with Attribute-Values from Database

        sqlTime.Start();
        var rawEntities = LoadEntityHeadersFromDb(appId, entityIds);
        sqlTime.Stop();

        // If optimized is enabled, then we tweak chunking size and skip unique checks if not necessary
        var optimized = appLoader.FeaturesService.IsEnabled(BuiltInFeatures.SqlLoadPerformance);
        var chunkSize = GetBestChunkSize(optimized);

        var detailsLoadSpecs = new EntityDetailsLoadSpecs(appId, !filterByEntityIds, rawEntities, featuresSvc, optimized, chunkSize, Log.IfDetails(appLoader.LogSettings));

        var relLoader = new RelationshipLoader(appLoader, detailsLoadSpecs);
        var relatedEntities = relLoader.LoadRelationships();
        codeRefTrail.AddMessage($"Raw entities: {rawEntities.Count}");

        // load attributes & values
        var attributes = optimized
            ? new ValueLoaderPro(appLoader, detailsLoadSpecs).LoadValues()
            : new ValueLoaderStandard(appLoader, detailsLoadSpecs).LoadValues();

        #endregion

        #region Build EntityModels

        var serializer = dataDeserializer.New();
        serializer.Initialize(builder.Reader);
        serializer.ConfigureLogging(appLoader.LogSettings);
        l.A($"🪵 Using LogSettings: {appLoader.LogSettings}");

        var logDetails = appLoader.LogSettings is { Enabled: true, Details: true };

        var buildHelper = new EntityBuildHelper(dataBuilder, builder.Reader, serializer, relatedEntities, attributes, appLoader.PrimaryLanguage, Log);

        var entityTimer = Stopwatch.StartNew();
        foreach (var rawEntity in rawEntities)
        {
            if (AddLogCount++ == MaxLogDetailsCount)
                l.A($"Will stop logging each item now, as we've already logged {AddLogCount} items");

            var newEntity = buildHelper.BuildNewEntity(rawEntity, l);

            // If entity is a draft, also include references to Published Entity
            builder.Add(newEntity, rawEntity.PublishedEntityId, logDetails && AddLogCount <= MaxLogDetailsCount);
        }

        entityTimer.Stop();
        l.A($"entities timer:{entityTimer.Elapsed}");

        #endregion

        return l.Return(sqlTime.Elapsed);
    }

    private int GetBestChunkSize(bool optimized)
    {
        if (!optimized)
            return RelationshipsIdChunkSizeDefault;

        var intChunkSize = featuresSvc.Get(BuiltInFeatures.SqlLoadPerformance.NameId)
            .ConfigInt(nameof(BuiltInFeatures.SqlPerformanceConfig.RelationshipLoadChunking),
                RelationshipsIdChunkSizeEnterprise, 10, 1000000);
            
        return intChunkSize;
    }

    public List<TempEntity> LoadEntityHeadersFromDb(int appId, int[] entityIds, string? filterJsonType = null)
    {
        var l = Log.IfSummary(appLoader.LogSettings).Fn<List<TempEntity>>($"app: {appId}, ids: {entityIds.Length}, {nameof(filterJsonType)}: '{filterJsonType}'", timer: true);

        if (appId == KnownAppsConstants.PresetAppId)
            return l.Return(new List<TempEntity>(), "preset app, skip DB query");

        var entitiesQuery = EntityQueries.EntitiesOfAppQuery(appId, entityIds, filterJsonType);

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
        l.A($"Query executed and converted to {nameof(TempEntity)}; {appLoader.Context.TrackingInfo()}");

        return l.Return(rawEntities, $"found: {rawEntities.Count}");
    }

}