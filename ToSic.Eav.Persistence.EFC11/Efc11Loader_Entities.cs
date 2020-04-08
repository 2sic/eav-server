﻿using System;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
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
                if (_primaryLanguage != null) return _primaryLanguage;
                var env = Factory.Resolve<IEnvironment>();
                _primaryLanguage = env.DefaultLanguage.ToLowerInvariant();
                Log.Add($"Primary language from environment (for attribute sorting): {_primaryLanguage}");
                return _primaryLanguage;
            }
            set => _primaryLanguage = value;
        }

        public const int IdChunkSize = 5000;
        public const int MaxLogDetailsCount = 1000;

        internal int AddLogCount;

        private void LoadEntities(AppState app, int[] entityIds = null)
        {
            var wrapLog = Log.Call($"{app.AppId}, {entityIds?.Length ?? 0}", useTimer: true);
            AddLogCount = 0; // reset, so anything in this call will be logged again up to 1000 entries
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
            var rawEntities = GetRawEntities(entityIds, appId, filterByEntityIds);
            sqlTime.Stop();
            var entityIdsFound = rawEntities.Select(e => e.EntityId).ToList();
            var entityIdChunks = entityIdsFound.ChunkBy(IdChunkSize);
            Log.Add($"Found {entityIdsFound.Count} raw entities in {sqlTime.ElapsedMilliseconds}ms - chunked into {entityIdChunks.Count} chunks");

            sqlTime.Start();
            var chunkedRelationships = entityIdChunks.Select(eIdC => GetRelatedEntities(appId, eIdC));
            var relatedEntities = chunkedRelationships.SelectMany(chunk => chunk).ToDictionary(i => i.Key, i => i.Value);
            Log.Add($"Found {relatedEntities.Count} entity relationships in {sqlTime.ElapsedMilliseconds}ms");


            #region load attributes & values

            var chunkedAttributes = entityIdChunks.Select(GetAttributesOfEntityChunk);
            var attributes = chunkedAttributes.SelectMany(chunk => chunk).ToDictionary(i => i.Key, i => i.Value);
            Log.Add($"Found {attributes.Count} attributes");
            #endregion

            sqlTime.Stop();

            #endregion

            #region Build EntityModels

            var serializer = Factory.Resolve<IDataDeserializer>();
            serializer.Initialize(app, Log);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                if (AddLogCount++ == MaxLogDetailsCount) Log.Add($"Will stop logging each item now, as we've already logged {AddLogCount} items");

                Entity newEntity;

                if (e.Json != null)
                {
                    newEntity = serializer.Deserialize(e.Json, false, true) as Entity;
                    // add properties which are not in the json
                    // ReSharper disable once PossibleNullReferenceException
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
                            newEntity.BuildReferenceAttribute(r.StaticName, r.Children, app);

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
                app.Add(newEntity, e.PublishedEntityId, AddLogCount <= MaxLogDetailsCount);

            }

            entityTimer.Stop();
            Log.Add($"entities timer:{entityTimer.Elapsed}");

            #endregion


            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
            wrapLog("ok");
        }

    }
}
