using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.BLL
{
    public class DbLoadIntoEavDataStructure: BllCommandBase
    {
        public DbLoadIntoEavDataStructure(EavDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Get all Entities Models for specified AppId
        /// </summary>
        internal IDictionary<int, IEntity> GetEavEntities(int appId, BaseCache source)
        {
            return GetAppDataPackage(null, appId, source, true).Entities;
        }

        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
        /// </summary>
        internal IDictionary<int, IContentType> GetEavContentTypes(int appId)
        {
            if (!Context.AttribSet.ContentTypes.ContainsKey(appId))
            {
                // Load from DB
                var contentTypes = from set in Context.SqlDb.AttributeSets
                    where set.AppID == appId && !set.ChangeLogIDDeleted.HasValue
                    select new
                    {
                        set.AttributeSetID,
                        set.Name,
                        set.StaticName,
                        set.Scope,
                        set.Description,
                        Attributes = (from a in set.AttributesInSets
                            select new
                            {
                                a.AttributeID,
                                a.Attribute.StaticName,
                                a.Attribute.Type,
                                a.IsTitle,
                                a.SortOrder
                            }),
                        IsGhost = set.UsesConfigurationOfAttributeSet,
                        SharedAttributes = (from a in Context.SqlDb.AttributesInSets
                            where a.AttributeSetID == set.UsesConfigurationOfAttributeSet
                            select new
                            {
                                a.AttributeID,
                                a.Attribute.StaticName,
                                a.Attribute.Type,
                                a.IsTitle,
                                a.SortOrder
                            }),
                        SharedAppDef = (from master in Context.SqlDb.AttributeSets
                                        where master.AttributeSetID == (set.UsesConfigurationOfAttributeSet ?? set.AttributeSetID)
                                              && master.UsesConfigurationOfAttributeSet == null
                            select new
                            {
                                AppId = master.AppID,
                                ZoneId = master.App.ZoneID,
                                ConfigIsOmnipresent = master.AlwaysShareConfiguration
                            }).FirstOrDefault()
                    };

                // Convert to ContentType-Model
                Context.AttribSet.ContentTypes[appId] = contentTypes.ToDictionary(k1 => k1.AttributeSetID, set => (IContentType)new ContentType(set.Name, set.StaticName, set.AttributeSetID, set.Scope, set.Description, set.IsGhost, set.SharedAppDef.ZoneId, set.SharedAppDef.AppId, set.SharedAppDef.ConfigIsOmnipresent)
                {
                    AttributeDefinitions = (set.IsGhost.HasValue ? set.SharedAttributes : set.Attributes)
                            .ToDictionary(k2 => k2.AttributeID, a => new AttributeBase(a.StaticName, a.Type, a.IsTitle, a.AttributeID, a.SortOrder) as IAttributeBase)
                });
            }

            return Context.AttribSet.ContentTypes[appId];
        }

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
                entityIds = entityIds.Union(from e in Context.SqlDb.Entities
                                            where e.PublishedEntityId.HasValue && !e.IsPublished && entityIds.Contains(e.EntityID) && !entityIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                                            select e.PublishedEntityId.Value).ToArray();
            #endregion

            #region Get Entities with Attribute-Values from Database

            var entitiesWithAandVfromDb = from e in Context.SqlDb.Entities
                                 where
                                     !e.ChangeLogIDDeleted.HasValue &&
                                     e.Set.AppID == appId &&
                                     e.Set.ChangeLogIDDeleted == null &&
                                     (	// filter by EntityIds (if set)
                                         !filterByEntityIds ||
                                         entityIds.Contains(e.EntityID) ||
                                         (e.PublishedEntityId.HasValue && entityIds.Contains(e.PublishedEntityId.Value))	// also load Drafts
                                         )
                                 orderby
                                     e.EntityID	// guarantees Published appear before draft
                                 select new
                                 {
                                     e.EntityID,
                                     e.EntityGUID,
                                     e.AttributeSetID,
                                     Metadata = new Metadata
                                     {
                                         TargetType = e.AssignmentObjectTypeID,
                                         KeyGuid = e.KeyGuid,
                                         KeyNumber = e.KeyNumber,
                                         KeyString = e.KeyString
                                     },
                                     e.IsPublished,
                                     e.PublishedEntityId,
                                     e.Owner, // new 2016-03-01
                                     Modified = e.ChangeLogModified.Timestamp,
                                     RelatedEntities = from r in e.EntityParentRelationships
                                                       group r by r.AttributeID
                                                           into rg
                                                           select new
                                                           {
                                                               AttributeID = rg.Key,
                                                               Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityID)
                                                           },
                                     Attributes = from v in e.Values
                                                  where !v.ChangeLogIDDeleted.HasValue
                                                  group v by v.AttributeID
                                                      into vg
                                                      select new
                                                      {
                                                          AttributeID = vg.Key,
                                                          Values = from v2 in vg
                                                                   orderby v2.ChangeLogIDCreated
                                                                   select new
                                                                   {
                                                                       v2.ValueID,
                                                                       v2.Value,
                                                                       Languages = from l in v2.ValuesDimensions
                                                                                   select new Data.Dimension
                                                                                   {
                                                                                       DimensionId = l.DimensionID,
                                                                                       ReadOnly = l.ReadOnly,
                                                                                       Key = l.Dimension.ExternalKey.ToLower()
                                                                                   },
                                                                       v2.ChangeLogIDCreated
                                                                   }
                                                      }
                                 };
            #endregion

            #region Build EntityModels
            var entities = new Dictionary<int, IEntity>();
            var entList = new List<IEntity>();

            foreach (var e in entitiesWithAandVfromDb)
            {
                var contentType = (ContentType)contentTypes[e.AttributeSetID];
                var newEntity = new Data.Entity(e.EntityGUID, e.EntityID, e.EntityID, e.Metadata /* e.AssignmentObjectTypeID */, contentType, e.IsPublished, relationships, e.Modified, e.Owner);

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
                    ((Data.Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
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

                #region add Related-Entities Attributes
                foreach (var r in e.RelatedEntities)
                {
                    var attributeModel = allAttribsOfThisType[r.AttributeID];
                    var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, r.Childs, source);
                    var valuesModelList = new List<IValue> { valueModel };
                    attributeModel.Values = valuesModelList;
                    attributeModel.DefaultValue = (IValueManagement)valuesModelList.FirstOrDefault();
                }
                #endregion

                #region Add "normal" Attributes (that are not Entity-Relations)
                foreach (var a in e.Attributes)
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
                        var valueModel = Value.GetValueModel(((IAttributeBase)attributeModel).Type, v.Value, v.Languages, v.ValueID, v.ChangeLogIDCreated);
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

                entities.Add(e.EntityID, newEntity);
                entList.Add(newEntity);
            }
            #endregion

            #region Populate Entity-Relationships (after all EntityModels are created)
            var relationshipsRaw = from r in Context.SqlDb.EntityRelationships
                                   where r.Attribute.AttributesInSets.Any(s => s.Set.AppID == appId && (!filterByEntityIds || (!r.ChildEntityID.HasValue || entityIds.Contains(r.ChildEntityID.Value)) || entityIds.Contains(r.ParentEntityID)))
                                   orderby r.ParentEntityID, r.AttributeID, r.ChildEntityID
                                   select new { r.ParentEntityID, r.Attribute.StaticName, r.ChildEntityID };
            foreach (var relationship in relationshipsRaw)
            {
                try
                {
                    if(entities.ContainsKey(relationship.ParentEntityID) && (!relationship.ChildEntityID.HasValue || entities.ContainsKey(relationship.ChildEntityID.Value)))
                        relationships.Add(new EntityRelationshipItem(entities[relationship.ParentEntityID], relationship.ChildEntityID.HasValue ? entities[relationship.ChildEntityID.Value] : null));
                }
                catch (KeyNotFoundException) { } // may occour if not all entities are loaded - edited 2rm 2015-09-29: Should not occur anymore
            }
            #endregion

            return new AppDataPackage(entities, entList, contentTypes, metadataForGuid, metadataForNumber, metadataForString, relationships);
        }

        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        public IEntity GetEavEntity(int entityId, BaseCache source = null)
            => GetAppDataPackage(new[] {entityId}, Context.AppId, source, true)
                .Entities.Single(e => e.Key == entityId).Value; // must filter by EntityId again because of Drafts

    }
}
