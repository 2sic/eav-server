using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Generics;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Intermediate;
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
                _primaryLanguage = _environmentLazy.Value.DefaultCultureCode.ToLowerInvariant();
                Log.A($"Primary language from environment (for attribute sorting): {_primaryLanguage}");
                return _primaryLanguage;
            }
            set => _primaryLanguage = value;
        }

        public const int IdChunkSize = 5000;
        public const int MaxLogDetailsCount = 250;

        internal int AddLogCount;

        private void LoadEntities(AppState app, int[] entityIds = null) => Log.Do($"{app.AppId}, {entityIds?.Length ?? 0}", timer: true, action: l =>
        {
            AddLogCount = 0; // reset, so anything in this call will be logged again up to 1000 entries
            var appId = app.AppId;

            #region Prepare & Extend EntityIds

            if (entityIds == null)
                entityIds = Array.Empty<int>();

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
            l.A(
                $"Found {entityIdsFound.Count} raw entities in {sqlTime.ElapsedMilliseconds}ms - chunked into {entityIdChunks.Count} chunks");

            sqlTime.Start();
            // Load relationships in batches / chunks
            var allChunks = entityIdChunks.Select(idList => GetRelationshipChunk(appId, idList))
                .SelectMany(chunk => chunk)
                .ToList();
            // in some strange cases we get duplicate keys - this should try to report what's happening
            var relatedEntities = GroupUniqueRelationships(allChunks);

            l.A($"Found {relatedEntities.Count} entity relationships in {sqlTime.ElapsedMilliseconds}ms");

            #region load attributes & values

            var chunkedAttributes = entityIdChunks.Select(GetAttributesOfEntityChunk);
            var attributes = chunkedAttributes.SelectMany(chunk => chunk).ToDictionary(i => i.Key, i => i.Value);
            l.A($"Found {attributes.Count} attributes");

            #endregion

            sqlTime.Stop();

            #endregion

            #region Build EntityModels

            var serializer = _dataDeserializer.New();
            serializer.Initialize(app);

            var entityTimer = Stopwatch.StartNew();
            foreach (var rawEntity in rawEntities)
            {
                if (AddLogCount++ == MaxLogDetailsCount)
                    l.A($"Will stop logging each item now, as we've already logged {AddLogCount} items");

                var newEntity = BuildNewEntity(app, rawEntity, serializer, relatedEntities, attributes, PrimaryLanguage);

                // If entity is a draft, also include references to Published Entity
                app.Add(newEntity, rawEntity.PublishedEntityId, AddLogCount <= MaxLogDetailsCount);
            }

            entityTimer.Stop();
            l.A($"entities timer:{entityTimer.Elapsed}");

            #endregion

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);
        });



        private IEntity BuildNewEntity(AppState app, TempEntity e, 
            IDataDeserializer serializer,
            Dictionary<int, IEnumerable<TempRelationshipList>> relatedEntities,
            Dictionary<int, IEnumerable<TempAttributeWithValues>> attributes,
            string primaryLanguage)
        {

            if (e.Json != null)
            {
                var fromJson = serializer.Deserialize(e.Json, false, true);
                // add properties which are not in the json
                // ReSharper disable once PossibleNullReferenceException
                //fromJson.IsPublished = e.IsPublished;
                //fromJson.Created = e.Created;
                //fromJson.Modified = e.Modified;
                //fromJson.Owner = e.Owner;
                var clonedExtended = _multiBuilder.Entity.Clone(fromJson,
                    isPublished: e.IsPublished,
                    created: e.Created,
                    modified: e.Modified,
                    owner: e.Owner
                );
                return clonedExtended; // fromJson;
            }

            var contentType = app.GetContentType(e.AttributeSetId);
            if (contentType == null)
                throw new NullReferenceException("content type is not found for type " + e.AttributeSetId);

            // Get all Attributes of that Content-Type
            var specs = _multiBuilder.Attribute.GenerateAttributesOfContentType(contentType);
            var newEntity = _multiBuilder.Entity.EntityFromRepository(app.AppId, e.EntityGuid, e.EntityId, e.EntityId,
                e.MetadataFor, contentType, e.IsPublished, app, e.Created, e.Modified, e.Owner,
                e.Version, values: specs.All, titleField: specs.Title);

            // add Related-Entities Attributes to the entity
            if (relatedEntities.ContainsKey(e.EntityId))
                foreach (var r in relatedEntities[e.EntityId])
                    _multiBuilder.Attribute.BuildReferenceAttribute(newEntity, r.StaticName, r.Children, app);

            #region Add "normal" Attributes (that are not Entity-Relations)

            if (!attributes.ContainsKey(e.EntityId)) 
                return newEntity;

            foreach (var a in attributes[e.EntityId])
            {
                if (!newEntity.Attributes.TryGetValue(a.Name, out var attrib))
                    continue;

                attrib.Values = a.Values
                    .Select(v => _multiBuilder.Value.Build(attrib.Type, v.Value, v.Languages))
                    .ToList();

                // fix faulty data dimensions in case old storage mechanisms messed up
                DataRepair.FixIncorrectLanguageDefinitions(attrib, primaryLanguage);
            }

            #endregion

            return newEntity;
        }

       
    }

}
