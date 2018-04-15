using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
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
        //public EntitiesController() { }

        public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<EntityWithHeaderOldFormat> items, [FromUri] bool partOfPage = false, bool draftOnly = false)
        {
            var myLog = new Log("Eav.SavMny", Log, "start");
            SetAppId(appId);

            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                i.Entity.Guid = i.Header.Guid;

            if(draftOnly)
                foreach (var i in items)
                {
                    i.Entity.IsPublished = false;
                    i.Entity.IsBranch = true;
                }

            IDeferredEntitiesList appPack = AppManager.Package;

            var entitiesToImport = items
                .Where(entity => entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty)
                .Select(e => CreateEntityFromTransferObject(e, appPack))
                .Cast<IEntity>()
                .ToList();

            myLog.Add("will save " + entitiesToImport.Count + " items");
            AppManager.Entities.Save(entitiesToImport);

            // find / update IDs of items updated to return to client
            var idList = items.Select(e =>
            {
                var foundEntity = AppManager.Read.Entities.Get(e.Header.Guid);
                return foundEntity?.GetDraft() ?? foundEntity;  // return the draft (that would be the latest), or the found, or null if not found
            })
            .Where(e => e != null)
            .ToDictionary(f => f.EntityGuid, f => f.EntityId);

            return idList;
        }


        private Entity CreateEntityFromTransferObject(EntityWithHeaderOldFormat editInfo, IDeferredEntitiesList allEntitiesForRelationships)
        {
            var toEntity = editInfo.Entity;
            var toMetadata = editInfo.Header.Metadata;

            // Attributes
            var attribs = new Dictionary<string, IAttribute>();

            // only transfer the fields / values which exist in the content-type definition
            var type = AppManager.Read.ContentTypes.Get(toEntity.Type.StaticName);
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
                        // manually de-serialize
                        objValue = JsonConvert.DeserializeObject<Guid?[]>(objValue.ToString());


                    var importValue = attribs.AddValue(attribute.Key, objValue, attributeType, allEntitiesForRelationships: allEntitiesForRelationships);

                    // append languages OR empty language as fallback
                    if (value.Dimensions != null)
                        foreach (var dimension in value.Dimensions)
                            importValue.Languages.Add(new Dimension { Key = dimension.Key, ReadOnly = dimension.Value });

                }
            }

            if (toEntity.Guid == Guid.Empty)
                throw new Exception("got empty guid - should never happen");

            var importEntity = new Entity(AppId, toEntity.Id, type, attribs.ToDictionary(x => x.Key, y => (object)y.Value))
            {
                #region Guids, Ids, Published, Content-Types
                IsPublished = toEntity.IsPublished,
                PlaceDraftInBranch = toEntity.IsBranch, // if it's not a branch, it should also force no branch...
                #endregion
            };

            importEntity.SetGuid(toEntity.Guid);

            #region Metadata if we have any
            // todo: as the objects are of the same type, we can probably remove the type Format.Metadata soon...
            if (toMetadata != null && toMetadata.HasMetadata)
                importEntity.SetMetadata(new MetadataFor
                {
                    TargetType = toMetadata.TargetType,
                    KeyGuid = toMetadata.KeyGuid,
                    KeyNumber = toMetadata.KeyNumber,
                    KeyString = toMetadata.KeyString
                });
            else
            {
                // todo: must re-attach metadata-info from previous generation of item...
                // this is important because the json must know about this for full serialization
                var orig = AppManager.Read.Entities.Get(editInfo.Header.Guid);
                if(orig != null)
                    importEntity.SetMetadata(new MetadataFor(orig.MetadataFor));
            }
            #endregion

            return importEntity;
        }




    }
}