using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    /// <summary>
    /// Will load all DB data into the memory data model using Entity Framework Core 1.1
    /// </summary>
    public partial class Efc11Loader: IRepositoryLoader
    {
        #region constructor and private vars
        public Efc11Loader(EavDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly EavDbContext _dbContext;

        #endregion


        #region AppPackage

        /// <inheritdoc />
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="parentLog"></param>
        /// <returns>app package with initialized app</returns>
        public AppDataPackage AppPackage(int appId, int[] entityIds = null, Log parentLog = null)
        {
            Log = new Log("DB.EFLoad", parentLog, $"get app data package for a#{appId}, ids only:{entityIds != null}");
            var app = new AppDataPackage(appId, parentLog);

            // prepare metadata lists & relationships etc.
            var timeUsed = InitMetadataLists(app, _dbContext);
            _sqlTotalTime = _sqlTotalTime.Add(timeUsed);

            // prepare content-types
            var typeTimer = Stopwatch.StartNew();
            var contentTypes = ContentTypes(appId, app/*.BetaDeferred*/);
            app.Set2ContentTypes(contentTypes);
            typeTimer.Stop();
            Log.Add($"timers types:{typeTimer.Elapsed}");

            // load data
            LoadEntitiesAndRelationships(app, entityIds);

            Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");

            return app;

        }

        private void LoadEntitiesAndRelationships(AppDataPackage app, int[] entityIds = null)
        {
            var appId = app.AppId;

            #region Prepare & Extend EntityIds

            if (entityIds == null)
                entityIds = new int[0];

            var filterByEntityIds = entityIds.Any();

            // if the app already exists and is being reloaded, remove all existing data
            if (!filterByEntityIds)
                app.Reset();

            // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
            var sqlTime = Stopwatch.StartNew();
            if (filterByEntityIds)
                entityIds = GetEntityIdOfPartnerEntities(entityIds);
            sqlTime.Stop();

            #endregion

            #region Get Entities with Attribute-Values from Database

            sqlTime.Start();
            var rawEntities = _dbContext.ToSicEavEntities
                .Include(e => e.AttributeSet)
                .Include(e => e.ToSicEavValues)
                .ThenInclude(v => v.ToSicEavValuesDimensions)
                .Where(e => !e.ChangeLogDeleted.HasValue &&
                            e.AppId == appId &&
                            // 2017-10 2dm previously was: e.AttributeSet.AppId == appId &&
                            e.AttributeSet.ChangeLogDeleted == null &&
                            (
                                // filter by EntityIds (if set)
                                !filterByEntityIds || entityIds.Contains(e.EntityId) ||
                                e.PublishedEntityId.HasValue && entityIds.Contains(e.PublishedEntityId.Value)
                                // also load Drafts
                            ))
                .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
                .Select(e => new
                {
                    e.EntityId,
                    e.EntityGuid,
                    e.Version,
                    e.AttributeSetId,
                    Metadata = new MetadataFor
                    {
                        TargetType = e.AssignmentObjectTypeId,
                        KeyGuid = e.KeyGuid,
                        KeyNumber = e.KeyNumber,
                        KeyString = e.KeyString
                    },
                    e.IsPublished,
                    e.PublishedEntityId,
                    e.Owner,
                    Modified = e.ChangeLogModifiedNavigation.Timestamp,
                    e.Json,
                    e.ContentType
                })
                .ToList();
            sqlTime.Stop();
            var entityIdsFound = rawEntities.Select(e => e.EntityId).ToList();

            sqlTime.Start();
            var relatedEntities = _dbContext.ToSicEavEntityRelationships
                .Include(rel => rel.Attribute)
                .Where(rel => rel.ParentEntity.AppId == appId)
                .Where(r => /*!filterByEntityIds ||*/ !r.ChildEntityId.HasValue ||
                                                      entityIdsFound.Contains(r.ChildEntityId.Value) ||
                                                      entityIdsFound.Contains(r.ParentEntityId))
                .GroupBy(g => g.ParentEntityId)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new
                    {
                        AttributeID = rg.Key,
                        rg.First().Attribute.StaticName,
                        Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    }));


            #region load attributes & values

            var attributes = _dbContext.ToSicEavValues
                .Include(v => v.Attribute)
                .Include(v => v.ToSicEavValuesDimensions)
                .ThenInclude(d => d.Dimension)
                .Where(r => entityIdsFound.Contains(r.EntityId))
                .Where(v => !v.ChangeLogDeleted.HasValue)
                .GroupBy(e => e.EntityId)
                .ToDictionary(e => e.Key, e => e.GroupBy(v => v.AttributeId)
                    .Select(vg => new
                    {
                        AttributeID = vg.Key,
                        Name = vg.First().Attribute.StaticName,
                        Values = vg
                            .OrderBy(v2 => v2.ChangeLogCreated)
                            .Select(v2 => new
                            {
                                v2.Value,
                                Languages = v2.ToSicEavValuesDimensions.Select(l => new Dimension
                                {
                                    DimensionId = l.DimensionId,
                                    ReadOnly = l.ReadOnly,
                                    Key = l.Dimension.EnvironmentKey.ToLowerInvariant()
                                } as ILanguage).ToList(),
                            })
                    }));

            #endregion

            sqlTime.Stop();

            #endregion

            #region Build EntityModels

            var serializer = Factory.Resolve<IThingDeserializer>();
            serializer.Initialize(app, /*appId, contentTypes, app.BetaDeferred ,*/ Log);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                Entity newEntity;

                if (e.Json != null)
                    newEntity = serializer.Deserialize(e.Json, false, true) as Entity;

                else
                {
                    var contentType = app.GetContentType(e.AttributeSetId);
                    if (contentType == null)
                        throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);

                    newEntity = EntityBuilder.EntityFromRepository(appId, e.EntityGuid, e.EntityId, e.EntityId,
                        e.Metadata, contentType, e.IsPublished, app.Relationships, app/*.BetaDeferred*/, e.Modified, e.Owner,
                        e.Version);

                    // Add all Attributes of that Content-Type
                    var titleAttrib = newEntity.GenerateAttributesOfContentType(contentType);
                    if(titleAttrib != null)
                        newEntity.SetTitleField(titleAttrib.Name);

                    // add Related-Entities Attributes to the entity
                    if (relatedEntities.ContainsKey(e.EntityId))
                        foreach (var r in relatedEntities[e.EntityId])
                            newEntity.BuildReferenceAttribute(r.StaticName, r.Childs, app);

                    #region Add "normal" Attributes (that are not Entity-Relations)

                    if (attributes.ContainsKey(e.EntityId))
                        foreach (var a in attributes[e.EntityId])
                        {
                            IAttribute attrib;
                            try
                            {
                                attrib = newEntity.Attributes[a.Name];
                            }
                            catch (KeyNotFoundException)
                            {
                                continue;
                            }

                            attrib.Values = a.Values
                                .Select(v => ValueBuilder.Build(attrib.Type, v.Value, v.Languages))
                                .ToList();

                            // fix faulty data dimensions in case old storage mechanims messed up
                            attrib.FixIncorrectLanguageDefinitions();
                        }

                    #endregion
                }

                // If entity is a draft, add references to Published Entity
                app.AddAndMapDraftToPublished(newEntity, e.PublishedEntityId);

            }

            entityTimer.Stop();
            Log.Add($"entities timer:{entityTimer.Elapsed}");

            #endregion

            #region Populate Entity-Relationships (after all Entities are created)

            var relTimer = Stopwatch.StartNew();

            // todo !!! 2018-01-20
            // compare times
            // ensure new method doesn't store null-references

            foreach (var relGroup in relatedEntities)
                foreach (var rel in relGroup.Value)
                    foreach (var child in rel.Childs)
                        app.Relationships.Add(relGroup.Key, child.Value);

            var classicMethod = app.Relationships.Count;

            app.RebuildRelationshipIndex();

            var newMethod = app.Relationships.Count; 

            relTimer.Stop();
            Log.Add($"relationship timer:{relTimer.Elapsed}");

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);

            #endregion
        }



        private int[] GetEntityIdOfPartnerEntities(int[] entityIds)
        {
            entityIds = entityIds.Union(from e in _dbContext.ToSicEavEntities
                where e.PublishedEntityId.HasValue && !e.IsPublished && entityIds.Contains(e.EntityId) &&
                      !entityIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                select e.PublishedEntityId.Value).ToArray();
            return entityIds;
        }


        private static TimeSpan InitMetadataLists(AppDataPackage app, EavDbContext dbContext)
        {
            var sqlTime = Stopwatch.StartNew();

            var metadataTypes = dbContext.ToSicEavAssignmentObjectTypes
                .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
            sqlTime.Stop();

            app.Set1MetadataManager(metadataTypes);

            return sqlTime.Elapsed;
        }

        #endregion


        public Dictionary<int, Zone> Zones() => _dbContext.ToSicEavZones
            .Include(z => z.ToSicEavApps)
            .Include(z => z.ToSicEavDimensions)
                .ThenInclude(d => d.ParentNavigation)
            .ToDictionary(z => z.ZoneId, z => new Zone(z.ZoneId,
                z.ToSicEavApps.FirstOrDefault(a => a.Name == Constants.DefaultAppName)?.AppId ?? -1,
                z.ToSicEavApps.ToDictionary(a => a.AppId, a => a.Name),
                z.ToSicEavDimensions.Where(d => d.ParentNavigation?.Key == Constants.CultureSystemKey)
                    // ReSharper disable once RedundantEnumerableCastCall
                    .Cast<DimensionDefinition>().ToList()));
    }
}
