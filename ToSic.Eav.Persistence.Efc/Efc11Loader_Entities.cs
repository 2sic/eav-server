using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Plumbing;
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
                _primaryLanguage = _environmentLazy.Value.DefaultLanguage.ToLowerInvariant();
                Log.Add($"Primary language from environment (for attribute sorting): {_primaryLanguage}");
                return _primaryLanguage;
            }
            set => _primaryLanguage = value;
        }

        public const int IdChunkSize = 5000;
        public const int MaxLogDetailsCount = 250;

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
            //var chunkedRelationships = entityIdChunks.Select(idList => GetRelationshipChunk(appId, idList));
            // Load relationships in batches / chunks
            var allChunks = entityIdChunks.Select(idList => GetRelationshipChunk(appId, idList))
                .SelectMany(chunk => chunk)
                .ToList();
            // in some strange cases we get duplicate keys - this should try to report what's happening
            //Dictionary<int, IEnumerable<TempRelationshipList>> relatedEntities;
            //try
            //{
            var relatedEntities = GroupUniqueRelationships(allChunks); // .ToDictionary(i => i.Key, i => i.Value);
            //}
            //catch (Exception ex)
            //{
            //    Log.Add("Ran into big problem merging relationship chunks. Will attempt to add more information");
            //    try
            //    {
            //        Log.Add($"These are the entity ID chunks in bundles of {IdChunkSize}");
            //        entityIdChunks.ForEach(eid => Log.Add("Chunk: " + string.Join(",", eid)));
            //    }
            //    catch { /* ignored */ }

            //    // do more detailed error reporting - but only if we didn't cause more errors
            //    try
            //    {
            //        Log.Add("These are the duplicates we think cause the problems");
            //        var duplicates = allChunks.GroupBy(c => c.Key)
            //            .Where(g => g.Count() > 1)
            //            .Select(g => g.Key)
            //            .ToList();
            //        Log.Add($"Found {duplicates.Count} duplicates.");
            //        Log.Add($"The duplicate IDs are probably: {string.Join(",", duplicates)}");
            //        throw new Exception("Ran into problems merging relationship chunks. Check the insights logs.", ex);
            //    }
            //    catch
            //    {
            //        throw new Exception("Ran into problems merging relationship chunks, and detailed analysis failed. Check Insights logs.", ex);
            //    }
            //}

            Log.Add($"Found {relatedEntities.Count} entity relationships in {sqlTime.ElapsedMilliseconds}ms");


            #region load attributes & values

            var chunkedAttributes = entityIdChunks.Select(GetAttributesOfEntityChunk);
            var attributes = chunkedAttributes.SelectMany(chunk => chunk).ToDictionary(i => i.Key, i => i.Value);
            Log.Add($"Found {attributes.Count} attributes");
            #endregion

            sqlTime.Stop();

            #endregion

            #region Build EntityModels

            var serializer = ServiceProvider.Build<IDataDeserializer>();
            serializer.Initialize(app, Log);

            var entityTimer = Stopwatch.StartNew();
            foreach (var e in rawEntities)
            {
                if (AddLogCount++ == MaxLogDetailsCount) Log.Add($"Will stop logging each item now, as we've already logged {AddLogCount} items");

                var newEntity = BuildNewEntity(app, e, serializer, relatedEntities, attributes);

                // If entity is a draft, also include references to Published Entity
                app.Add(newEntity, e.PublishedEntityId, AddLogCount <= MaxLogDetailsCount);

            }

            entityTimer.Stop();
            Log.Add($"entities timer:{entityTimer.Elapsed}");

            #endregion


            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
            wrapLog("ok");
        }



        private static Entity BuildNewEntity(AppState app, TempEntity e, 
            IDataDeserializer serializer,
            Dictionary<int, IEnumerable<TempRelationshipList>> relatedEntities,
            Dictionary<int, IEnumerable<TempAttributeWithValues>> attributes)
        {
            Entity newEntity;

            if (e.Json != null)
            {
                newEntity = serializer.Deserialize(e.Json, false, true) as Entity;
                // add properties which are not in the json
                // ReSharper disable once PossibleNullReferenceException
                newEntity.IsPublished = e.IsPublished;
                newEntity.Modified = e.Modified;
                newEntity.Owner = e.Owner;
                return newEntity;
            }

            var contentType = app.GetContentType(e.AttributeSetId);
            if (contentType == null)
                throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);

            newEntity = EntityBuilder.EntityFromRepository(app.AppId, e.EntityGuid, e.EntityId, e.EntityId,
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

            if (!attributes.ContainsKey(e.EntityId)) 
                return newEntity;

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

            return newEntity;
        }
    }
}
