using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for various actions
    /// </summary>
    public class EntitiesController : Eav3WebApiBase
    {
        public EntitiesController(int appId) : base(appId) { }
        public EntitiesController(Log parentLog) : base(parentLog) { }

        public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<BundleWithHeader<EntityWithLanguages>> items,
            [FromUri] bool partOfPage = false, bool draftOnly = false)
        {
            //todo: remove this once we're sure we're not using the global appid for anything
            SetAppId(appId);
            var appMan = new AppManager(appId, Log);
            Log.Add($"SaveMany(appId:{appId}, items:{items.Count}, partOfPage (not used here, must be done in dnn):{partOfPage}, draftOnly:{draftOnly})");

            #region this area should probably be disabled, but I'm not sure if it's still needed
            // #1 set guid
            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                i.EntityGuid = i.Header.Guid; //i.Entity.Guid = i.Header.Guid;

            // #2 ensure draft/branch state
            if (draftOnly)
                foreach (var i in items)
                {
                    Log.Add($"draft only, will set published/isbranch on {i.Header.Guid}");
                    i.Entity.IsPublished = false;
                    i.Entity.IsBranch = true;
                }
            #endregion

            // #3 create valid list of entities
            //IDeferredEntitiesList appPack = appMan.Package;

            var entitySetToImport = items
                .Where(entity => entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty)
                .Select(e => new BundleWithHeader<IEntity>
                {
                    Header = e.Header,
                    Entity = CreateEntityFromTransferObject(appMan, e)
                })
                .ToList();

            return UpdateGuidAndPublishedAndSaveMany(appMan, items, entitySetToImport, draftOnly);
        }

        public Dictionary<Guid, int> UpdateGuidAndPublishedAndSaveMany(AppManager appMan, IEnumerable<BundleWithHeader> allItems, List<BundleWithHeader<IEntity>> itemsToImport, bool draftOnly)
        {
            foreach (var bundle in itemsToImport)
            {
                var currEntity = (Entity) bundle.Entity;
                currEntity.SetGuid(bundle.Header.Guid);
                if (draftOnly)
                {
                    Log.Add($"draft only, will set published/isbranch on {bundle.Header.Guid}");
                    currEntity.IsPublished = false;
                    currEntity.PlaceDraftInBranch = true;
                }
            }

            var entitiesToImport = itemsToImport.Select(e => e.Entity).ToList();

            // #4 save
            Log.Add("will save " + entitiesToImport.Count + " items");
            appMan.Entities.Save(entitiesToImport);

            // #5 find / update IDs of items updated to return to client
            return GenerateIdList(allItems, appMan.Read.Entities);
        }

        /// <summary>
        /// Generate pairs of guid/id of the newly added items
        /// </summary>
        /// <returns></returns>
        private Dictionary<Guid, int> GenerateIdList(IEnumerable<BundleWithHeader> items, EntityRuntime appEntities)
        {
            var idList = items.Select(e =>
                {
                    var foundEntity = appEntities.Get(e.Header.Guid);
                    var state = foundEntity == null ? "not found" : foundEntity.IsPublished ? "published" : "draft";
                    var draft = foundEntity?.GetDraft();
                    Log.Add(
                        $"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                    return
                        draft ?? foundEntity; // return the draft (that would be the latest), or the found, or null if not found
                })
                .Where(e => e != null)
                .ToDictionary(f => f.EntityGuid, f => f.EntityId);
            return idList;
        }


        private Entity CreateEntityFromTransferObject(AppManager appMan, BundleWithHeader<EntityWithLanguages> editInfo/*, IDeferredEntitiesList allEntitiesForRelationships*/)
        {
            var allEntitiesForRelationships = appMan.Package;

            Log.Add($"CreateEntityFromTransferObject(editInfo:{editInfo.Header.ContentTypeName}:{editInfo.Header.Guid}, allEntitiesForRelationships:{allEntitiesForRelationships?.List?.Count()})");
            var toEntity = editInfo.Entity;

            // Attributes
            var attribs = new Dictionary<string, IAttribute>();

            // only transfer the fields / values which exist in the content-type definition
            var type = appMan.Read.ContentTypes.Get(toEntity.Type.StaticName);
            foreach (var attribute in toEntity.Attributes)
            {
                var attDef = type[attribute.Key];
                var attributeType = attDef.Type;

                // don't save anything of the type empty - this is headings-items-only
                if (attributeType == AttributeTypeEnum.Empty.ToString())
                    continue;

                foreach (var value in attribute.Value.Values)
                {
                    var objValue = value.Value;

                    // special situation: if it's an array of Guids, mixed with NULLs, then it's not correctly auto-de-serialized
                    if (attributeType == AttributeTypeEnum.Entity.ToString() && objValue is Newtonsoft.Json.Linq.JArray)
                        objValue = JsonConvert.DeserializeObject<Guid?[]>(objValue.ToString());// manually de-serialize

                    var importValue = attribs.AddValue(attribute.Key, objValue, attributeType,
                        allEntitiesForRelationships: allEntitiesForRelationships);

                    // append languages OR empty language as fallback
                    if (value.Dimensions != null)
                        foreach (var dimension in value.Dimensions)
                            importValue.Languages.Add(new Dimension { Key = dimension.Key, ReadOnly = dimension.Value });
                }
            }


            var importEntity = new Entity(appMan.AppId, toEntity.Id, type, attribs.ToDictionary(x => x.Key, y => (object)y.Value))
            {
            // 2018-09-06 2dm moved this to later processing, full save should still work
                //IsPublished = toEntity.IsPublished,
                //PlaceDraftInBranch = toEntity.IsBranch, // if it's not a branch, it should also force no branch...
            };
            // 2018-09-06 2dm moved this to later processing, full save should still work
            //if (toEntity.Guid == Guid.Empty)
            //    throw new Exception("got empty guid - should never happen");
            //importEntity.SetGuid(toEntity.Guid);

            Log.Add($"Import Entity app:{importEntity.AppId} id:{importEntity.EntityId}, "
                    + $"guid:{importEntity.EntityGuid}, "
                    + $"type:{importEntity.Type.Name}, published:{importEntity.IsPublished}, "
                    + $"DraftInBranch:{importEntity.PlaceDraftInBranch}");


            #region Metadata if we have any
            var newMd = editInfo.Header.Metadata;
            if (newMd != null && newMd.HasMetadata)
            {
                Log.Add($"will set metadata based on input type:{newMd.TargetType} guid:{newMd.KeyGuid} #:{newMd.KeyNumber} $:{newMd.KeyString}");
                importEntity.SetMetadata(new MetadataFor
                {
                    TargetType = newMd.TargetType,
                    KeyGuid = newMd.KeyGuid,
                    KeyNumber = newMd.KeyNumber,
                    KeyString = newMd.KeyString
                });}
            else
            {
                // must re-attach metadata-info from previous generation of item...
                // this is important because the json must know about this for full serialization
                var original = appMan.Read.Entities.Get(editInfo.Header.Guid); // get again, using guid!
                if (original != null)
                {
                    Log.Add("found original - will use to re-attach metadata");
                    importEntity.SetMetadata(new MetadataFor(original.MetadataFor));
                }
                else
                    Log.Add("no original found, will not attach metadata");
            }
            #endregion

            return importEntity;
        }




    }
}