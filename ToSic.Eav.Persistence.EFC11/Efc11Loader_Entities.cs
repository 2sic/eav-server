﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;
using ToSic.Eav.Serialization;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        private string _primaryLanguage;
        public string PrimaryLanguage
        {
            get {
                if(_primaryLanguage == null)
                {
                    var env = Factory.Resolve<IEnvironment>();
                    _primaryLanguage = env.DefaultLanguage.ToLowerInvariant();
                    Log.Add($"Primary language from environment (for attribute sorting): {_primaryLanguage}");
                }
                return _primaryLanguage;
            }
            set => _primaryLanguage = value;
        }

        private int[] GetEntityIdOfPartnerEntities(int[] repositoryIds)
        {
            var relatedIds = from e in _dbContext.ToSicEavEntities
                where e.PublishedEntityId.HasValue && !e.IsPublished && repositoryIds.Contains(e.EntityId) &&
                      !repositoryIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                select e.PublishedEntityId.Value;

            var combined = repositoryIds.Union(relatedIds).ToArray();

            return combined;
        }

        private void LoadEntities(AppState app, int[] entityIds = null)
        {
            var wrapLog = Log.Call($"{app.AppId}, {entityIds?.Length ?? 0}", useTimer: true);
            
            var appId = app.AppId;

            #region Prepare & Extend EntityIds

            if (entityIds == null)
                entityIds = new int[0];

            var filterByEntityIds = entityIds.Any();

            // if the app already exists and is being reloaded, remove all existing data
            if (!filterByEntityIds)
                app.RemoveAllItems();

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
                            e.AttributeSet.ChangeLogDeleted == null &&
                            (
                                // filter by EntityIds (if set)
                                !filterByEntityIds || entityIds.Contains(e.EntityId)
                            ))
                .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
                .Select(e => new
                {
                    e.EntityId,
                    e.EntityGuid,
                    e.Version,
                    e.AttributeSetId,
                    Metadata = new Metadata.Target
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
                .Where(r => !r.ChildEntityId.HasValue ||
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
                            // The order of values is significant because the 2sxc system uses the first value as fallback
                            // Because we can't ensure order of values when saving, order values: prioritize values without
                            // any dimensions, then values with primary language
                            .OrderByDescending(v2 => !v2.ToSicEavValuesDimensions.Any())
                            .ThenByDescending(v2 => v2.ToSicEavValuesDimensions.Any(l => string.Equals(l.Dimension.EnvironmentKey, PrimaryLanguage, StringComparison.InvariantCultureIgnoreCase)))
                            .ThenBy(v2 => v2.ChangeLogCreated)
                            .Select(v2 => new
                            {
                                v2.Value,
                                Languages = v2.ToSicEavValuesDimensions.Select(l => new Language
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

            var serializer = Factory.Resolve<IDataDeserializer>();
            serializer.Initialize(app, Log);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                Entity newEntity;

                if (e.Json != null)
                {
                    newEntity = serializer.Deserialize(e.Json, false, true) as Entity;
                    // add properties which are not in the json
                    newEntity.IsPublished = e.IsPublished;
                    newEntity.Modified = e.Modified;
                    newEntity.Owner = e.Owner;
                }
                else
                {
                    var contentType = app.GetContentType(e.AttributeSetId);
                    if (contentType == null)
                        throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);

                    newEntity = EntityBuilder.EntityFromRepository(appId, e.EntityGuid, e.EntityId, e.EntityId,
                        e.Metadata, contentType, e.IsPublished, app, e.Modified, e.Owner,
                        e.Version);

                    // Add all Attributes of that Content-Type
                    var titleAttrib = newEntity.GenerateAttributesOfContentType(contentType);
                    if (titleAttrib != null)
                        newEntity.SetTitleField(titleAttrib.Name);

                    // add Related-Entities Attributes to the entity
                    if (relatedEntities.ContainsKey(e.EntityId))
                        foreach (var r in relatedEntities[e.EntityId])
                            newEntity.BuildReferenceAttribute(r.StaticName, r.Childs, app);

                    #region Add "normal" Attributes (that are not Entity-Relations)

                    if (attributes.ContainsKey(e.EntityId))
                        foreach (var a in attributes[e.EntityId])
                        {
                            if (!newEntity.Attributes.TryGetValue(a.Name, out var attrib))
                                continue;

                            attrib.Values = a.Values
                                .Select(v => ValueBuilder.Build(attrib.Type, v.Value, v.Languages))
                                .ToList();

                            // fix faulty data dimensions in case old storage mechanims messed up
                            attrib.FixIncorrectLanguageDefinitions();
                        }

                    #endregion
                }

                // If entity is a draft, also include references to Published Entity
                app.Add(newEntity, e.PublishedEntityId);

            }

            entityTimer.Stop();
            Log.Add($"entities timer:{entityTimer.Elapsed}");

            #endregion


            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
            wrapLog("ok");
        }

    }
}
