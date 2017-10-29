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
        /// <returns>Item1: EntityModels, Item2: all ContentTypes, Item3: Assignment Object Types</returns>
        public AppDataPackage AppPackage(int appId, int[] entityIds = null, bool entitiesOnly = false, Log parentLog = null)
        {
            Log = new Log("DB.EFLoad", parentLog, $"get app data package for a#{appId}, ids only:{entityIds != null}, entities-only:{entitiesOnly}");
            var source = new AppDataPackageDeferredList();

            #region prepare content-types
            var typeTimer = Stopwatch.StartNew();
            var contentTypes = ContentTypes(appId, source);
            var sysTypes = Global.SystemContentTypes();
            
            typeTimer.Stop();
            #endregion

            var relationships = new List<EntityRelationshipItem>();

            #region prepare metadata lists for relationships etc.
            var metadataForGuid = new Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>>();
            var metadataForNumber = new Dictionary<int, Dictionary<int, IEnumerable<IEntity>>>();
            var metadataForString = new Dictionary<int, Dictionary<string, IEnumerable<IEntity>>>();

            var sqlTime = Stopwatch.StartNew();
            var metadataTypes = _dbContext.ToSicEavAssignmentObjectTypes.ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
            sqlTime.Stop();

            foreach (var mdt in metadataTypes.ToList())
            {
                metadataForGuid.Add(mdt.Key, new Dictionary<Guid, IEnumerable<IEntity>>());
                metadataForNumber.Add(mdt.Key, new Dictionary<int, IEnumerable<IEntity>>());
                metadataForString.Add(mdt.Key, new Dictionary<string, IEnumerable<IEntity>>());
            }

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
                    Metadata = new Metadata
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
                    e.Json
                })
                .ToList();
            sqlTime.Stop();
            var eIds = rawEntities.Select(e => e.EntityId).ToList();

            sqlTime.Start();
            var relatedEntities = _dbContext.ToSicEavEntityRelationships
                .Include(rel => rel.Attribute)
                .Where(r => eIds.Contains(r.ParentEntityId))
                .GroupBy(g => g.ParentEntityId)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new
                    {
                        AttributeID = rg.Key,
                        Name = rg.First().Attribute.StaticName,
                        Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    }));

            var attributes = _dbContext.ToSicEavValues
                .Include(v => v.Attribute)
                .Include(v => v.ToSicEavValuesDimensions)
                    .ThenInclude(d => d.Dimension)
                .Where(r => eIds.Contains(r.EntityId))
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
            sqlTime.Stop();
            #endregion

            #region Build EntityModels
            var entities = new Dictionary<int, IEntity>();
            var entList = new List<IEntity>();

            var serializer = Factory.Resolve<IThingDeserializer>();
            serializer.Initialize(appId, contentTypes.Values, source);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                Entity newEntity;

                var useJson = e.Json != null;
                
                if(useJson)
                    newEntity = serializer.Deserialize(e.Json) as Entity;

                else
                {
                    var contentType = (ContentType)contentTypes[e.AttributeSetId];
                    
                    // test if there is a global code-type overriding this type
                    if (sysTypes.ContainsKey(contentType.StaticName))
                        contentType = (ContentType)sysTypes[contentType.StaticName];

                    newEntity = EntityBuilder.EntityFromRepository(appId, e.EntityGuid, e.EntityId, e.EntityId, e.Metadata, contentType, e.IsPublished, relationships, e.Modified, e.Owner, e.Version);

                    //var allAttribsOfThisType = new Dictionary<string, IAttribute>(); // temporary Dictionary to set values later more performant by Dictionary-Key (AttributeId)
                    IAttribute titleAttrib = null;

                    // Add all Attributes of that Content-Type
                    foreach (var definition in contentType.Attributes)
                    {
                        var entityAttribute = ((AttributeDefinition) definition).CreateAttribute();
                        newEntity.Attributes.Add(entityAttribute.Name, entityAttribute);
                        //allAttribsOfThisType.Add(definition.Name, entityAttribute);
                        if (definition.IsTitle)
                            titleAttrib = entityAttribute;
                    }


                    #region add Related-Entities Attributes to the entity

                    if (relatedEntities.ContainsKey(e.EntityId))
                        foreach (var r in relatedEntities[e.EntityId])
                        {
                            var attrib = newEntity.Attributes[r.Name];//  allAttribsOfThisType[r.Name];//r.AttributeID];
                            attrib.Values = new List<IValue> {Value.Build(attrib.Type, r.Childs, null, source)};
                        }

                    #endregion

                    #region Add "normal" Attributes (that are not Entity-Relations)

                    if (attributes.ContainsKey(e.EntityId))
                        foreach (var a in attributes[e.EntityId]) // e.Attributes)
                        {
                            IAttribute attrib;
                            try
                            {
                                attrib = newEntity.Attributes[a.Name];// allAttribsOfThisType[a.Name];// a.AttributeID];
                            }
                            catch (KeyNotFoundException)
                            {
                                continue;
                            }
                            if (attrib == titleAttrib)
                                newEntity.SetTitleField(attrib.Name);

                            attrib.Values = a.Values.Select(v => Value.Build(attrib.Type, v.Value, v.Languages))
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

                #region Add metadata-lists based on AssignmentObjectTypes

                if (e.Metadata.IsMetadata && !entitiesOnly)
                {
                    // Try guid first. Note that an item can be assigned to both a guid, string and an int if necessary, though not commonly used
                    if (e.Metadata.KeyGuid.HasValue)
                        AddToMetaDic(metadataForGuid, e.Metadata.TargetType, e.Metadata.KeyGuid.Value, newEntity);
                    if (e.Metadata.KeyNumber.HasValue)
                        AddToMetaDic(metadataForNumber, e.Metadata.TargetType, e.Metadata.KeyNumber.Value, newEntity);
                    if (!string.IsNullOrEmpty(e.Metadata.KeyString))
                        AddToMetaDic(metadataForString, e.Metadata.TargetType, e.Metadata.KeyString, newEntity);
                }

                #endregion

                entities.Add(e.EntityId, newEntity);
                entList.Add(newEntity);
            }
            entityTimer.Stop();
            #endregion

            #region Populate Entity-Relationships (after all Entitys are created)

            var relTimer = Stopwatch.StartNew();
            var relationshipQuery = _dbContext.ToSicEavEntityRelationships
                .Include(er => er.Attribute.ToSicEavAttributesInSets)
                .Where(r => r.Attribute.ToSicEavAttributesInSets.Any(s => s.AttributeSet.AppId == appId))
                .Where(r => !filterByEntityIds || !r.ChildEntityId.HasValue || entityIds.Contains(r.ChildEntityId.Value) ||
                         entityIds.Contains(r.ParentEntityId))
                .OrderBy(r => r.ParentEntityId).ThenBy(r => r.AttributeId).ThenBy(r => r.ChildEntityId)
                .Select(r => new {r.ParentEntityId, r.Attribute.StaticName, r.ChildEntityId});

            var relationshipsRaw = relationshipQuery.ToList();
            foreach (var relationship in relationshipsRaw)
            {
                try
                {
                    if (entities.ContainsKey(relationship.ParentEntityId) &&
                        (!relationship.ChildEntityId.HasValue ||
                         entities.ContainsKey(relationship.ChildEntityId.Value)))
                        relationships.Add(new EntityRelationshipItem(entities[relationship.ParentEntityId],
                            relationship.ChildEntityId.HasValue ? entities[relationship.ChildEntityId.Value] : null));
                }
                catch (KeyNotFoundException) { /* ignore */ }
            }
            relTimer.Stop();
            #endregion

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
            Log.Add($"timers types&typesql:{typeTimer.Elapsed} sqlAll:{_sqlTotalTime}, entities:{entityTimer.Elapsed}, relationship:{relTimer.Elapsed}");

            var appPack = new AppDataPackage(appId, entities, entList, contentTypes, metadataForGuid, metadataForNumber, metadataForString, relationships, source);
            return appPack;
        }

        private static void AddToMetaDic<T>(Dictionary<int, Dictionary<T, IEnumerable<IEntity>>> metadataForGuid, int mdTargetType, T mdValue, Entity newEntity)
        {
            // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) has an empty list of items
            if (!metadataForGuid[mdTargetType].ContainsKey(mdValue)) // ensure target list exists
                metadataForGuid[mdTargetType][mdValue] = new List<IEntity>();

            // Now all containers must exist, add this item
            ((List<IEntity>) metadataForGuid[mdTargetType][mdValue]).Add(newEntity);
        }

        #endregion


        //public Dictionary<int, string> MetadataTargetTypes() => _dbContext.ToSicEavAssignmentObjectTypes
        //    .ToDictionary(a => a.AssignmentObjectTypeId, a => a.Name);

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
