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
using ToSic.Eav.Types;

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
        /// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
        /// <param name="parentLog"></param>
        /// <returns>app package with initialized app</returns>
        public AppDataPackage AppPackage(int appId, int[] entityIds = null, bool entitiesOnly = false, Log parentLog = null)
        {
            Log = new Log("DB.EFLoad", parentLog, $"get app data package for a#{appId}, ids only:{entityIds != null}, entities-only:{entitiesOnly}");
            var app = new AppDataPackage(appId, parentLog);

            #region prepare metadata lists & relationships etc.

            var sqlTime = Stopwatch.StartNew();
            InitMetadataLists(app, _dbContext);
            sqlTime.Stop();

            #endregion

            #region prepare content-types
            var typeTimer = Stopwatch.StartNew();
            var contentTypes = ContentTypes(appId, app.BetaDeferred);
            var sysTypes = Global.AllContentTypes();
            app.Set2ContentTypes(contentTypes);
            typeTimer.Stop();
            #endregion




            #region Prepare & Extend EntityIds
            if (entityIds == null)
                entityIds = new int[0];

            var filterByEntityIds = entityIds.Any();

            // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
            sqlTime.Start();
            if (filterByEntityIds)
                entityIds = entityIds.Union(from e in _dbContext.ToSicEavEntities
                                            where e.PublishedEntityId.HasValue && !e.IsPublished && entityIds.Contains(e.EntityId) && !entityIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                                            select e.PublishedEntityId.Value).ToArray();
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
                .Include(er => er.Attribute.ToSicEavAttributesInSets)
                                 /* new - but can't use, as sometimes we're using a global schema, so this would block those relationships */
                                 //.Where(r => r.Attribute.ToSicEavAttributesInSets.Any(s => s.AttributeSet.AppId == appId))

                // very new...
                .Where(rel => rel.ParentEntity.AppId == appId)

                 //.Where(r => entityIdsFound.Contains(r.ParentEntityId))
                .Where(r => /*!filterByEntityIds ||*/ !r.ChildEntityId.HasValue || /*entityIds*/entityIdsFound.Contains(r.ChildEntityId.Value) ||
                            /*entityIds*/entityIdsFound.Contains(r.ParentEntityId))

                .GroupBy(g => g.ParentEntityId)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new
                    {
                        AttributeID = rg.Key,
                        rg.First().Attribute.StaticName,
                        Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    }));

            var debug = relatedEntities.Count;

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
            var entities = new Dictionary<int, IEntity>();

            var serializer = Factory.Resolve<IThingDeserializer>();
            serializer.Initialize(appId, contentTypes, app.BetaDeferred, Log);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                Entity newEntity;

                var useJson = e.Json != null;
                
                if(useJson)
                    newEntity = serializer.Deserialize(e.Json, false, true) as Entity;

                else
                {
                    // todo: continue here!
                    var contentType = (ContentType)contentTypes.SingleOrDefault(ct => ct.ContentTypeId == e.AttributeSetId);
                    if(contentType == null) throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);
                    
                    // test if there is a global code-type overriding this type
                    if (sysTypes.ContainsKey(contentType.StaticName))
                        contentType = (ContentType)sysTypes[contentType.StaticName];

                    newEntity = EntityBuilder.EntityFromRepository(appId, e.EntityGuid, e.EntityId, e.EntityId, 
                        e.Metadata, contentType, e.IsPublished, app.Relationships, app.BetaDeferred, e.Modified, e.Owner, e.Version);

                    IAttribute titleAttrib = null;

                    // Add all Attributes of that Content-Type
                    foreach (var definition in contentType.Attributes)
                    {
                        var entityAttribute = ((AttributeDefinition) definition).CreateAttribute();
                        newEntity.Attributes.Add(entityAttribute.Name, entityAttribute);
                        if (definition.IsTitle)
                            titleAttrib = entityAttribute;
                    }


                    #region add Related-Entities Attributes to the entity

                    if (relatedEntities.ContainsKey(e.EntityId))
                        foreach (var r in relatedEntities[e.EntityId])
                        {
                            var attrib = newEntity.Attributes[r.StaticName];
                            attrib.Values = new List<IValue> {ValueBuilder.Build(attrib.Type, r.Childs, null, app.BetaDeferred)};
                        }

                    #endregion

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
                            if (attrib == titleAttrib)
                                newEntity.SetTitleField(attrib.Name);

                            attrib.Values = a.Values.Select(v => ValueBuilder.Build(attrib.Type, v.Value, v.Languages))
                                .ToList();

                            #region issue fix faulty data dimensions

                            // Background: there are rare cases, where data was stored incorrectly
                            // this happens when a attribute has multiple values, but some don't have languages assigned
                            // that would be invalid, as any property with a language code must have all the values (for that property) with language codes
                            if (attrib.Values.Count > 1 && attrib.Values.Any(v => !v.Languages.Any()))
                            {
                                var badValuesWithoutLanguage = attrib.Values.Where(v => !v.Languages.Any()).ToList();
                                if (badValuesWithoutLanguage.Any())
                                    badValuesWithoutLanguage.ForEach(badValue =>
                                        attrib.Values.Remove(badValue));
                            }

                            #endregion
                        }

                    // Special treatment in case there is no title 
                    // sometimes happens if the title-field is re-defined and old data might no have this
                    // also happens in rare cases, where the title-attrib is an entity-picker
                    if (newEntity.Title == null && titleAttrib != null)
                        newEntity.SetTitleField(titleAttrib.Name);

                    #endregion
                }

                #region If entity is a draft, add references to Published Entity
                if (!e.IsPublished && e.PublishedEntityId.HasValue)
                {
                    // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
                    newEntity.PublishedEntity = entities[e.PublishedEntityId.Value];
                    ((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
                    newEntity.EntityId = e.PublishedEntityId.Value;
                }
                #endregion

                #region Add to metadata

                if (!entitiesOnly)
                    app.Metadata.Add(newEntity);

                #endregion

                entities.Add(e.EntityId, newEntity);
            }
            entityTimer.Stop();
            #endregion

            #region Populate Entity-Relationships (after all Entities are created)

            var relTimer = Stopwatch.StartNew();

            foreach (var relGroup in relatedEntities)
                foreach (var rel in relGroup.Value)
                    foreach (var child in rel.Childs)
                    //try
                    //{
                        app.Relationships.Add(entities, relGroup.Key, child.Value);
                    //}
                    //catch (KeyNotFoundException)
                    //{
                    //    /* ignore */
                    //}


            //var relationshipQuery = _dbContext.ToSicEavEntityRelationships
            //    .Include(er => er.Attribute.ToSicEavAttributesInSets)
            //    .Where(r => r.Attribute.ToSicEavAttributesInSets.Any(s => s.AttributeSet.AppId == appId))
            //    .Where(r => !filterByEntityIds || !r.ChildEntityId.HasValue || /*entityIds*/entityIdsFound.Contains(r.ChildEntityId.Value) ||
            //             /*entityIds*/entityIdsFound.Contains(r.ParentEntityId))
            //    .OrderBy(r => r.ParentEntityId)
            //    .ThenBy(r => r.AttributeId)
            //    .ThenBy(r => r.ChildEntityId)
            //    .Select(r => new { r.ParentEntityId, r.Attribute.StaticName, r.ChildEntityId });

            //var relationshipsRaw = relationshipQuery.ToList();

            //foreach (var relationship in relationshipsRaw)
            //{
            //    try
            //    {
            //        app.Relationships.Add(entities, relationship.ParentEntityId, relationship.ChildEntityId);
            //        //if (entities.ContainsKey(relationship.ParentEntityId) &&
            //        //    (!relationship.ChildEntityId.HasValue ||
            //        //     entities.ContainsKey(relationship.ChildEntityId.Value)))
            //        //    relationships.Add(new EntityRelationshipItem(entities[relationship.ParentEntityId],
            //        //        relationship.ChildEntityId.HasValue ? entities[relationship.ChildEntityId.Value] : null));
            //    }
            //    catch (KeyNotFoundException) { /* ignore */ }
            //}

            relTimer.Stop();
            #endregion

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
            Log.Add($"timers types&typesql:{typeTimer.Elapsed} sqlAll:{_sqlTotalTime}, entities:{entityTimer.Elapsed}, relationship:{relTimer.Elapsed}");

            app.Set3Entities(entities.Values);
            return app;

            //var appPack = new AppDataPackage(appId, entities.Values, contentTypes, 
            //    appMdManager,
            //    //relationships, 
            //    //source,
            //    Log);
            //return appPack;
        }


        private static void InitMetadataLists(AppDataPackage app, EavDbContext dbContext)
        {
            var metadataTypes = dbContext.ToSicEavAssignmentObjectTypes
                .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);

            var appMdManager = new AppMetadataManager(metadataTypes);
            app.Set1MetadataManager(appMdManager);
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
