using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11
{
    /// <summary>
    /// 
    /// </summary>
    internal class Efc11Loader
    {
        #region constructor and private vars
        internal Efc11Loader(EavDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly EavDbContext _dbContext;

        private Dictionary<int, Dictionary<int, IContentType>> _contentTypes 
            = new Dictionary<int, Dictionary<int, IContentType>>();
        #endregion

        #region Testing / Analytics helpers
        internal void ResetCacheForTesting()
        => _contentTypes = new Dictionary<int, Dictionary<int, IContentType>>();
        #endregion

        #region Load Content-Types into IContent-Type Dictionary
        /// <summary>
        /// Get all ContentTypes for specified AppId. 
        /// If uses temporary caching, so if called multiple times it loads from a private field.
        /// </summary>
        internal IDictionary<int, IContentType> GetEavContentTypes(int appId)
        {
            if (!_contentTypes.ContainsKey(appId))
                LoadContentTypesIntoLocalCache(appId);
            return _contentTypes[appId];
        }

        /// <summary>
        /// Load DB content-types into loader-cache
        /// </summary>
        /// <param name="appId"></param>
        private void LoadContentTypesIntoLocalCache(int appId)
        {
            // Load from DB
            var contentTypes = _dbContext.ToSicEavAttributeSets
                    .Where(set => set.AppId == appId && set.ChangeLogDeleted == null)
                    .Include(set => set.ToSicEavAttributesInSets)
                        .ThenInclude(attrs => attrs.Attribute)
                    .Include(set => set.App)
                    .Include(set => set.UsesConfigurationOfAttributeSetNavigation)
                        .ThenInclude(master => master.App)
                    .ToList()
                    .Select(set => new
                    {
                        set.AttributeSetId,
                        set.Name,
                        set.StaticName,
                        set.Scope,
                        set.Description,
                        Attributes = set.ToSicEavAttributesInSets
                            .Select(a => new
                            {
                                a.AttributeId,
                                a.Attribute.StaticName,
                                a.Attribute.Type,
                                a.IsTitle,
                                a.SortOrder
                            })
                        ,
                        IsGhost = set.UsesConfigurationOfAttributeSet,
                        SharedDefinitionId = set.UsesConfigurationOfAttributeSet,
                        AppId = set.UsesConfigurationOfAttributeSetNavigation?.AppId ?? set.AppId,
                        ZoneId = set.UsesConfigurationOfAttributeSetNavigation?.App?.ZoneId ?? set.App.ZoneId,
                        ConfigIsOmnipresent =
                        set.UsesConfigurationOfAttributeSetNavigation?.AlwaysShareConfiguration ?? set.AlwaysShareConfiguration,
                    })
                .ToList();

            var shareids = contentTypes.Select(c => c.SharedDefinitionId).ToList();
            var sharedAttribs = _dbContext.ToSicEavAttributeSets
                .Include(s => s.ToSicEavAttributesInSets)
                    .ThenInclude(a => a.Attribute)
                .Where(s => shareids.Contains(s.AttributeSetId))
                .ToDictionary(s => s.AttributeSetId, s => s.ToSicEavAttributesInSets.Select(a => new
                {
                    a.AttributeId,
                    a.Attribute.StaticName,
                    a.Attribute.Type,
                    a.IsTitle,
                    a.SortOrder
                }));

            #region old stuff / hidden
            //var contentTypes = from set in DbContext.ToSicEavAttributeSets
            //                   where set.AppId == appId && !set.ChangeLogDeleted.HasValue
            //                   select new
            //                   {
            //                       set.AttributeSetId,
            //                       set.Name,
            //                       set.StaticName,
            //                       set.Scope,
            //                       set.Description,
            //                       Attributes =
            //                       (from a in set.ToSicEavAttributesInSets
            //                                     select new
            //                                     {
            //                                         a.AttributeId,
            //                                         a.Attribute.StaticName,
            //                                         a.Attribute.Type,
            //                                         a.IsTitle,
            //                                         a.SortOrder
            //                                     }),
            //                       ToSicEavAttributesInSets = set.ToSicEavAttributesInSets,


            //                       IsGhost = set.UsesConfigurationOfAttributeSet

            //,
            //SharedAttributes = (from a in DbContext.ToSicEavAttributesInSets
            //                    where a.AttributeSetId == set.UsesConfigurationOfAttributeSet
            //                    select new
            //                    {
            //                        a.AttributeId,
            //                        a.Attribute.StaticName,
            //                        a.Attribute.Type,
            //                        a.IsTitle,
            //                        a.SortOrder
            //                    })

            //    ,
            //SharedAppDef = (from master in DbContext.ToSicEavAttributeSets
            //                where master.AttributeSetId == (set.UsesConfigurationOfAttributeSet ?? set.AttributeSetId)
            //                      && master.UsesConfigurationOfAttributeSet == null
            //                select new
            //                {
            //                    master.AppId,
            //                    master.App.ZoneId,
            //                    ConfigIsOmnipresent = master.AlwaysShareConfiguration
            //                }).FirstOrDefault()
            //};
            #endregion 

            // Convert to ContentType-Model
            _contentTypes[appId] = contentTypes.ToDictionary(k1 => k1.AttributeSetId,
                set => (IContentType) new ContentType(set.Name, set.StaticName, set.AttributeSetId, 
                    set.Scope, set.Description, set.IsGhost, set.ZoneId, set.AppId, set.ConfigIsOmnipresent)
                    {
                        AttributeDefinitions =  (set.SharedDefinitionId.HasValue
                                ? sharedAttribs[set.SharedDefinitionId.Value]
                                : set.Attributes)
                            .ToDictionary(k2 => k2.AttributeId,
                                a => new AttributeBase(a.StaticName, a.Type, a.IsTitle, a.AttributeId, a.SortOrder) as
                                    IAttributeBase)
                    }
            );
        }


        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
        /// </summary>
        internal IDictionary<int, IContentType> GetEavContentTypesSlower(int appId)
        {
            if (!_contentTypes.ContainsKey(appId))
            {
                // Load from DB
                var contentTypes = from set in _dbContext.ToSicEavAttributeSets
                    where set.AppId == appId && !set.ChangeLogDeleted.HasValue
                    select new
                    {
                        set.AttributeSetId,
                        set.Name,
                        set.StaticName,
                        set.Scope,
                        set.Description,
                        Attributes = (from a in set.ToSicEavAttributesInSets
                            select new
                            {
                                a.AttributeId,
                                a.Attribute.StaticName,
                                a.Attribute.Type,
                                a.IsTitle,
                                a.SortOrder
                            }),
                        IsGhost = set.UsesConfigurationOfAttributeSet,
                        SharedAttributes = (from a in _dbContext.ToSicEavAttributesInSets
                            where a.AttributeSetId == set.UsesConfigurationOfAttributeSet
                            select new
                            {
                                a.AttributeId,
                                a.Attribute.StaticName,
                                a.Attribute.Type,
                                a.IsTitle,
                                a.SortOrder
                            })
                            ,
                        SharedAppDef = (from master in _dbContext.ToSicEavAttributeSets
                                        where master.AttributeSetId == (set.UsesConfigurationOfAttributeSet ?? set.AttributeSetId)
                                              && master.UsesConfigurationOfAttributeSet == null
                            select new
                            {
                                master.AppId, master.App.ZoneId,
                                ConfigIsOmnipresent = master.AlwaysShareConfiguration
                            }).FirstOrDefault()
                    };

                // Convert to ContentType-Model
                _contentTypes[appId] = contentTypes.ToDictionary(k1 => k1.AttributeSetId, set => (IContentType)new ContentType(set.Name, set.StaticName, set.AttributeSetId, set.Scope, set.Description, set.IsGhost, set.SharedAppDef.ZoneId, set.SharedAppDef.AppId, set.SharedAppDef.ConfigIsOmnipresent)
                {
                    AttributeDefinitions = (set.IsGhost.HasValue ? set.SharedAttributes : set.Attributes)
                            .ToDictionary(k2 => k2.AttributeId, a => new AttributeBase(a.StaticName, a.Type, a.IsTitle, a.AttributeId, a.SortOrder) as IAttributeBase)
                });
            }

            return _contentTypes[appId];
        }

        #endregion

        /// <summary>Get Data to populate ICache</summary>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="source">DataSource to get child entities</param>
        /// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
        /// <returns>Item1: EntityModels, Item2: all ContentTypes, Item3: Assignment Object Types</returns>
        internal AppDataPackage GetAppDataPackage(int[] entityIds, int appId, IDeferredEntitiesList source, bool entitiesOnly = false)
        {
            var contentTypes = GetEavContentTypes(appId);

            var metadataForGuid = new Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>>();
            var metadataForNumber = new Dictionary<int, Dictionary<int, IEnumerable<IEntity>>>();
            var metadataForString = new Dictionary<int, Dictionary<string, IEnumerable<IEntity>>>();

            var relationships = new List<EntityRelationshipItem>();

            #region Prepare & Extend EntityIds
            if (entityIds == null)
                entityIds = new int[0];

            var filterByEntityIds = entityIds.Any();

            // Ensure published Versions of Drafts are also loaded (if filtered by EntityId, otherwise all Entities from the app are loaded anyway)
            if (filterByEntityIds)
                entityIds = entityIds.Union(from e in _dbContext.ToSicEavEntities
                                            where e.PublishedEntityId.HasValue && !e.IsPublished && entityIds.Contains(e.EntityId) && !entityIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                                            select e.PublishedEntityId.Value).ToArray();
            #endregion

            #region Get Entities with Attribute-Values from Database

            var rawEntities = _dbContext.ToSicEavEntities
                .Include(e => e.AttributeSet)
                .Include(e => e.ToSicEavValues)
                    .ThenInclude(v => v.ToSicEavValuesDimensions)
                .Where(e => !e.ChangeLogDeleted.HasValue &&
                            e.AttributeSet.AppId == appId &&
                            e.AttributeSet.ChangeLogDeleted == null &&
                            ( 
                                // filter by EntityIds (if set)
                                !filterByEntityIds || entityIds.Contains(e.EntityId) ||
                                e.PublishedEntityId.HasValue && entityIds.Contains(e.PublishedEntityId.Value)
                                // also load Drafts
                            ))
                .Select(e => new
                {
                    e.EntityId,
                    e.EntityGuid,
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
                    // RelTest = e.RelationshipsWithThisAsParent,
                    //RelatedEntities = e.RelationshipsWithThisAsParent
                    //    // .Where(r => r.ParentEntityId == e.EntityId) // test
                    //    .GroupBy(r => r.AttributeId)
                    //    .Select(rg => new {
                    //        AttributeID = rg.Key,
                    //        Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    //    }),


                    //Attributes = e.ToSicEavValues
                    //    .Where(v => !v.ChangeLogDeleted.HasValue)
                    //    .GroupBy(v => v.AttributeId)
                    //    .Select(vg =>  new {
                    //        AttributeID = vg.Key,
                    //        Values = vg
                    //            .OrderBy(v2 => v2.ChangeLogCreated)
                    //            .Select(v2 => new {
                    //            v2.ValueId,
                    //            v2.Value,
                    //            Languages = v2.ToSicEavValuesDimensions
                    //                .Select(l =>  new Dimension
                    //                {
                    //                    DimensionId = l.DimensionId,
                    //                    ReadOnly = l.ReadOnly,
                    //                    Key = l.Dimension.ExternalKey.ToLower()
                    //                }),
                    //            v2.ChangeLogCreated
                    //        })
                    //    })
                }).ToList();
            var eIds = rawEntities.Select(e => e.EntityId).ToList();

            var relatedEntities = _dbContext.ToSicEavEntityRelationships
                .Where(r => eIds.Contains(r.ParentEntityId))
                .GroupBy(g => g.ParentEntityId)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new
                    {
                        AttributeID = rg.Key,
                        Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    }));

            var attributes = _dbContext.ToSicEavValues
                .Include(v => v.ToSicEavValuesDimensions)
                .Where(r => eIds.Contains(r.EntityId))
                .Where(v => !v.ChangeLogDeleted.HasValue)
                .GroupBy(e => e.EntityId)
                .ToDictionary(e => e.Key, e => e.GroupBy(v => v.AttributeId)
                    .Select(vg => new
                    {
                        AttributeID = vg.Key,
                        Values = vg
                            .OrderBy(v2 => v2.ChangeLogCreated)
                            .Select(v2 => new
                            {
                                v2.ValueId,
                                v2.Value,
                                Languages = v2.ToSicEavValuesDimensions
                                    .Select(l => new Dimension
                                    {
                                        DimensionId = l.DimensionId,
                                        ReadOnly = l.ReadOnly,
                                        Key = l.Dimension.ExternalKey.ToLower()
                                    }),
                                v2.ChangeLogCreated
                            })
                    }));

            #region hidden / commented out
            //var entitiesWithAandVfromDb = from e in _dbContext.ToSicEavEntities
            //                     where
            //                         !e.ChangeLogDeleted.HasValue &&
            //                         e.AttributeSet.AppId == appId &&
            //                         e.AttributeSet.ChangeLogDeleted == null &&
            //                         (	// filter by EntityIds (if set)
            //                             !filterByEntityIds ||
            //                             entityIds.Contains(e.EntityId) ||
            //                             (e.PublishedEntityId.HasValue && entityIds.Contains(e.PublishedEntityId.Value))	// also load Drafts
            //                             )
            //                     orderby
            //                         e.EntityId	// guarantees Published appear before draft
            //                     select new
            //                     {
            //                         e.EntityId,
            //                         e.EntityGuid,
            //                         e.AttributeSetId,
            //                         Metadata = new Metadata
            //                         {
            //                             TargetType = e.AssignmentObjectTypeId,
            //                             KeyGuid = e.KeyGuid,
            //                             KeyNumber = e.KeyNumber,
            //                             KeyString = e.KeyString
            //                         },
            //                         e.IsPublished,
            //                         e.PublishedEntityId,
            //                         e.Owner, // new 2016-03-01
            //                         Modified = e.ChangeLogModifiedNavigation.Timestamp, //.ChangeLogModified.Timestamp,
            //                         RelatedEntities = from r in e.ToSicEavEntityRelationshipsParentEntity
            //                                           group r by r.AttributeId
            //                                               into rg
            //                                               select new
            //                                               {
            //                                                   AttributeID = rg.Key,
            //                                                   Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
            //                                               },
            //                         Attributes = from v in e.ToSicEavValues
            //                                      where !v.ChangeLogDeleted.HasValue
            //                                      group v by v.AttributeId
            //                                          into vg
            //                                          select new
            //                                          {
            //                                              AttributeID = vg.Key,
            //                                              Values = from v2 in vg
            //                                                       orderby v2.ChangeLogCreated
            //                                                       select new
            //                                                       {
            //                                                           v2.ValueId,
            //                                                           v2.Value,
            //                                                           Languages = from l in v2.ToSicEavValuesDimensions//.ValuesDimensions
            //                                                                       select new Dimension
            //                                                                       {
            //                                                                           DimensionId = l.DimensionId,
            //                                                                           ReadOnly = l.ReadOnly,
            //                                                                           Key = l.Dimension.ExternalKey.ToLower()
            //                                                                       },
            //                                                           v2.ChangeLogCreated
            //                                                       }
            //                                          }
            //                     };
            #endregion
            #endregion

            // return null;

            #region Build EntityModels
            var entities = new Dictionary<int, IEntity>();
            var entList = new List<IEntity>();

            foreach (var e in rawEntities)
            {
                var contentType = (ContentType)contentTypes[e.AttributeSetId];
                var newEntity = new Entity(e.EntityGuid, e.EntityId, e.EntityId, e.Metadata /* e.AssignmentObjectTypeID */, contentType, e.IsPublished, relationships, e.Modified, e.Owner);

                var allAttribsOfThisType = new Dictionary<int, IAttributeManagement>();	// temporary Dictionary to set values later more performant by Dictionary-Key (AttributeId)
                IAttributeManagement titleAttrib = null;

                // Add all Attributes from that Content-Type
                foreach (var definition in contentType.AttributeDefinitions.Values)
                {
                    var newAttribute = AttributeHelperTools.GetAttributeManagementModel(definition);
                    newEntity.Attributes.Add(((IAttributeBase)newAttribute).Name, newAttribute);
                    allAttribsOfThisType.Add(definition.AttributeId, newAttribute);
                    if (newAttribute.IsTitle)
                        titleAttrib = newAttribute;
                }

                // If entity is a draft, add references to Published Entity
                if (!e.IsPublished && e.PublishedEntityId.HasValue)
                {
                    // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
                    newEntity.PublishedEntity = entities[e.PublishedEntityId.Value];
                    ((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
                    newEntity.EntityId = e.PublishedEntityId.Value;
                }

                #region Add metadata-lists based on AssignmentObjectTypes

                // unclear why #1 is handled in a special way - why should this not be cached? I believe 1 means no specific assignment
                if (e.Metadata.HasMetadata && !entitiesOnly)
                {
                    // Try guid first. Note that an item can be assigned to both a guid, string and an int if necessary, though not commonly used
                    if (e.Metadata.KeyGuid.HasValue)
                    {
                        // Ensure that this assignment-Type (like 4 = entity-assignment) already has a dictionary for storage
                        if (!metadataForGuid.ContainsKey(e.Metadata.TargetType)) // ensure AssignmentObjectTypeID
                            metadataForGuid.Add(e.Metadata.TargetType, new Dictionary<Guid, IEnumerable<IEntity>>());

                        // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) has an empty list of items
                        if (!metadataForGuid[e.Metadata.TargetType].ContainsKey(e.Metadata.KeyGuid.Value)) // ensure Guid
                            metadataForGuid[e.Metadata.TargetType][e.Metadata.KeyGuid.Value] = new List<IEntity>();

                        // Now all containers must exist, add this item
                        ((List<IEntity>)metadataForGuid[e.Metadata.TargetType][e.Metadata.KeyGuid.Value]).Add(newEntity);
                    }
                    if (e.Metadata.KeyNumber.HasValue)
                    {
                        if (!metadataForNumber.ContainsKey(e.Metadata.TargetType)) // ensure AssignmentObjectTypeID
                            metadataForNumber.Add(e.Metadata.TargetType, new Dictionary<int, IEnumerable<IEntity>>());

                        if (!metadataForNumber[e.Metadata.TargetType].ContainsKey(e.Metadata.KeyNumber.Value)) // ensure Guid
                            metadataForNumber[e.Metadata.TargetType][e.Metadata.KeyNumber.Value] = new List<IEntity>();

                        ((List<IEntity>)metadataForNumber[e.Metadata.TargetType][e.Metadata.KeyNumber.Value]).Add(newEntity);
                    }
                    if (!string.IsNullOrEmpty(e.Metadata.KeyString))
                    {
                        if (!metadataForString.ContainsKey(e.Metadata.TargetType)) // ensure AssignmentObjectTypeID
                            metadataForString.Add(e.Metadata.TargetType, new Dictionary<string, IEnumerable<IEntity>>());

                        if (!metadataForString[e.Metadata.TargetType].ContainsKey(e.Metadata.KeyString)) // ensure Guid
                            metadataForString[e.Metadata.TargetType][e.Metadata.KeyString] = new List<IEntity>();

                        ((List<IEntity>)metadataForString[e.Metadata.TargetType][e.Metadata.KeyString]).Add(newEntity);
                    }
                }

                #endregion

                #region add Related-Entities Attributes to the entity
                if(relatedEntities.ContainsKey(e.EntityId))
                foreach (var r in relatedEntities[e.EntityId])
                {
                    var attributeModel = allAttribsOfThisType[r.AttributeID];
                    var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, r.Childs, source);
                    var valuesModelList = new List<IValue> { valueModel };
                    attributeModel.Values = valuesModelList;
                    attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
                }
                #endregion

                //if (false)
                    #region Add "normal" Attributes (that are not Entity-Relations)
                if(attributes.ContainsKey(e.EntityId))
                    foreach (var a in attributes[e.EntityId])// e.Attributes)
                    {
                        IAttributeManagement attributeModel;
                        try
                        {
                            attributeModel = allAttribsOfThisType[a.AttributeID];
                        }
                        catch (KeyNotFoundException)
                        {
                            continue;
                        }
                        if (attributeModel.IsTitle)
                            newEntity.Title = attributeModel;
                        var valuesModelList = new List<IValue>();

                        #region Add all Values
                        foreach (var v in a.Values)
                        {
                            var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, v.Value, v.Languages, v.ValueId, v.ChangeLogCreated);
                            valuesModelList.Add(valueModel);
                        }
                        #endregion

                        attributeModel.Values = valuesModelList;
                        attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
                    }

                // Special treatment in case there is no title 
                // sometimes happens if the title-field is re-defined and ol data might no have this
                // also happens in rare cases, where the title-attrib is an entity-picker
                if (newEntity.Title == null)
                    newEntity.Title = titleAttrib;
                #endregion

                entities.Add(e.EntityId, newEntity);
                entList.Add(newEntity);
            }
            #endregion

            #region Populate Entity-Relationships (after all Entitys are created)

            var relationshipsRaw = from r in _dbContext.ToSicEavEntityRelationships
                                    where
                                    r.Attribute.ToSicEavAttributesInSets.Any(
                                        s =>
                                            s.AttributeSet.AppId == appId &&
                                            (!filterByEntityIds ||
                                            (!r.ChildEntityId.HasValue || entityIds.Contains(r.ChildEntityId.Value)) ||
                                            entityIds.Contains(r.ParentEntityId)))
                                    orderby r.ParentEntityId, r.AttributeId, r.ChildEntityId
                                    select new { r.ParentEntityId, r.Attribute.StaticName, r.ChildEntityId };

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
                catch (KeyNotFoundException)
                {
                    // may occour if not all entities are loaded - edited 2rm 2015-09-29: Should not occur anymore
                    // ignore
                }
            }

            #endregion

            return new AppDataPackage(entities, entList, contentTypes, metadataForGuid, metadataForNumber, metadataForString, relationships);
        }

        ///// <summary>
        ///// Get EntityModel for specified EntityId
        ///// </summary>
        ///// <returns>A single IEntity or throws InvalidOperationException</returns>
        //public IEntity GetEavEntity(int entityId, BaseCache source = null)
        //    => GetAppDataPackage(new[] { entityId }, DbContext.AppId, source, true)
        //        .Entities.Single(e => e.Key == entityId).Value; // must filter by EntityId again because of Drafts

    }
}
