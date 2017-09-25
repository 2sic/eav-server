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
using ToSic.Eav.Persistence.Versions;

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
       public EntitiesController() { }

        #region GetOne GetAll calls
        public IEntity GetEntityOrThrowError(string contentType, int id)
        {
            //SetAppIdAndUser(appId);

            // must use cache, because it shows both published  unpublished
            var found = AppManager.Read.Entities.Get(id);
            if (contentType != null && !(found.Type.Name == contentType || found.Type.StaticName == contentType))
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
            return found;
        }

        public IEntity GetEntityOrThrowError(string contentType, Guid guid, int? appId = null)
        {
            SetAppIdAndUser(appId);

            // must use cache, because it shows both published  unpublished
            var itm = AppManager.Read.Entities.Get(guid);
            if (itm == null || (contentType != null && !(itm.Type.Name == contentType || itm.Type.StaticName == contentType)))
                throw new KeyNotFoundException("Can't find " + guid + "of type '" + contentType + "'");
            return itm;
        }

        /// <summary>
        /// this is needed by 2sxc
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id"></param>
        /// <param name="appId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetOne(string contentType, int id, int? appId = null, string cultureCode = null)
        {
            SetAppIdAndUser(appId);

            var found = GetEntityOrThrowError(contentType, id);
            return Serializer.Prepare(found);
        }

        /// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(string contentType, string cultureCode = null, int? appId = null)
        {
            SetAppIdAndUser(appId);

            return Serializer.Prepare(AppManager.Read.Entities.Get(contentType));
            //var typeFilter = AppManager.Read.Entities.Get(contentType);//  DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
            //typeFilter.TypeName = contentType;

            //return Serializer.Prepare(typeFilter.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
        }

        public IEnumerable<Dictionary<string, object>> GetAllOfTypeForAdmin(int appId, string contentType)
        {
            SetAppIdAndUser(appId);
            Serializer.ConfigureForAdminUse();

            var list = Serializer.Prepare(AppManager.Read.Entities.Get(contentType));

            var newList = list.Select(li => li.ToDictionary(x1 => x1.Key, x2 => TruncateIfString(x2.Value, 50))).ToList();

            return newList;
        }

        internal object TruncateIfString(object value, int length)
        {
            var asTxt = value as string;
            if (asTxt == null)
                return value;

            if (asTxt.Length > length)
                asTxt = asTxt.Substring(0, length);
            return asTxt;
        }

        internal EntityWithLanguages GetOne(int appId, string contentType, int id, int? duplicateFrom = null, string format = "multi-language")
        {
            switch (format)
            {
                case "multi-language":
                    Serializer.IncludeAllEditingInfos = true;

                    var found = GetEntityOrThrowError(contentType, duplicateFrom ?? id);
                    var maybeDraft = found.GetDraft();
                    if (maybeDraft != null)
                        found = maybeDraft;

                    var ce = new EntityWithLanguages
                    {
                        AppId = appId,
                        Id = duplicateFrom.HasValue ? 0 : found.EntityId,
                        //RepoId = duplicateFrom.HasValue ? 0 : found.RepositoryId,
                        Guid = duplicateFrom.HasValue ? Guid.Empty : found.EntityGuid,
                        Type = new Formats.Type { Name = found.Type.Name, StaticName = found.Type.StaticName },
                        IsPublished = found.IsPublished,
                        IsBranch = (!found.IsPublished && found.GetPublished() != null),
                        TitleAttributeName = found.Title?.Name,
                        Attributes = found.Attributes.ToDictionary(a => a.Key, a => new Formats.Attribute
                            {
                            Values = a.Value.Values?.Select(v => new ValueSet
                            {
                                Value = v.SerializableObject,  
                                Dimensions = v.Languages.ToDictionary(l => l.Key, y => y.ReadOnly)
                            }).ToArray() ?? new ValueSet[0]
                        }
                        )
                    };
                    return ce;
                default:
                    throw new Exception("format: " + format + " unknown");
            }
        }

        [HttpPost]
        public List<EntityWithHeader> GetManyForEditing([FromUri]int appId, [FromBody]List<ItemIdentifier> items)
        {
            Log.Add($"get many for editing a#{appId}, items⋮{items.Count}");
            SetAppIdAndUser(appId);

            // clean up content-type names in case it's using the nice-name instead of the static name...
            // var cache = DataSource.GetCache(null, appId);
            foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
            {
                var ct = AppManager.Read.ContentTypes.Get(itm.ContentTypeName);
                if (ct == null)
                {
                    if (!itm.ContentTypeName.StartsWith("@"))
                        throw new Exception("Can't find content type " + itm.ContentTypeName);
                    items.Remove(itm);
                    continue;
                }
                if (ct.StaticName != itm.ContentTypeName) // not using the static name...fix
                    itm.ContentTypeName = ct.StaticName;
            }

            var list = items.Select(p => new EntityWithHeader
            {
                Header = p,
                Entity = p.EntityId != 0 || p.DuplicateEntity.HasValue ? GetOne(appId, p.ContentTypeName, p.EntityId, p.DuplicateEntity) : null
            }).ToList();

            // make sure the header has the right "new" guid as well - as this is the primary one to work with
            // it is really important to use the header guid, because sometimes the entity does not exist - so it doesn't have a guid either
            foreach (var i in list.Where(i => i.Header.Guid == Guid.Empty).ToArray()) // must do toarray, to prevent re-checking after setting the guid
                i.Header.Guid = i.Entity != null && i.Entity.Guid != Guid.Empty
                    ? i.Entity.Guid
                    : Guid.NewGuid();

            foreach (var itm in list.Where(i => i.Header.ContentTypeName == null && i.Entity != null))
                itm.Header.ContentTypeName = itm.Entity.Type.StaticName;

            Log.Add($"will return items⋮{list.Count}");
            return list;
        }

        #endregion
        [HttpPost]
        public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<EntityWithHeader> items, [FromUri] bool partOfPage = false)
        {
            var myLog = new Log("ESavM", Log, "start");
            SetAppIdAndUser(appId);

            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                i.Entity.Guid = i.Header.Guid;

            var entitiesToImport = items
                .Where(entity => entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty)
                .Select(CreateEntityFromTransferObject)
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


        private Entity CreateEntityFromTransferObject(EntityWithHeader editInfo)
        {
            var toEntity = editInfo.Entity;
            var toMetadata = editInfo.Header.Metadata;

            // Attributes
            var attribs = new Dictionary<string, IAttribute>();

            // only transfer the fields / values which exist in the content-type definition
            var attributeSet = AppManager.Read.ContentTypes.Get(toEntity.Type.StaticName);
            foreach (var attribute in toEntity.Attributes)
            {
                var attDef = attributeSet[attribute.Key];
                var attributeType = attDef.Type;

                // don't save anything of the type empty - this is headings-items-only
                if (attributeType == AttributeTypeEnum.Empty.ToString())
                    continue;

                foreach (var value in attribute.Value.Values)
                {
                    var objValue = value.Value;// ImpEntity.ImpConvertValueObjectToString(value.Value);

                    // special situation: if it's an array of Guids, mixed with NULLs, then it's not correctly auto-de-serialized
                    if (attributeType == AttributeTypeEnum.Entity.ToString() && objValue is Newtonsoft.Json.Linq.JArray)
                    {
                        // manually de-serialize
                        var guidArray = JsonConvert.DeserializeObject<Guid?[]>(objValue.ToString());
                        objValue = guidArray;
                    }


                    var importValue = attribs.AddValue(attribute.Key, objValue, attributeType);

                    // append languages OR empty language as fallback
                    if (value.Dimensions == null)
                    {
                        // 2017-06-12 2dm - AFAIK this never added anything, because key was "", so just comment ount
                        // Must this be done to save entities
                        //importValue.AddLanguageReference("", false);
                        continue;
                    }
                    foreach (var dimension in value.Dimensions)
                        importValue.Languages.Add(new Dimension { Key = dimension.Key, ReadOnly = dimension.Value });//.AddLanguageReference(dimension.Key, dimension.Value);

                }
            }

            if (toEntity.Guid == Guid.Empty)
                throw new Exception("got empty guid - should never happen");

            var importEntity = new Entity(AppId, toEntity.Id, toEntity.Type.StaticName, attribs.ToDictionary(x => x.Key, y => (object)y.Value))
            {
                #region Guids, Ids, Published, Content-Types
                IsPublished = toEntity.IsPublished,
                PlaceDraftInBranch = toEntity.IsBranch, // if it's not a branch, it should also force no branch...
                #endregion
            };

            importEntity.SetGuid(toEntity.Guid);
            //if (toEntity.IsBranch)
            //    importEntity.SetPublishedIdForSaving(toEntity.Id); 


            #region Metadata if we have any
            // todo: as the objects are of the same type, we can probably remove the type Format.Metadata soon...
            if (toMetadata != null && toMetadata.HasMetadata)
                importEntity.SetMetadata(new Data.Metadata
                {
                    TargetType = toMetadata.TargetType,
                    KeyGuid = toMetadata.KeyGuid,
                    KeyNumber = toMetadata.KeyNumber,
                    KeyString = toMetadata.KeyString
                });

            #endregion

            return importEntity;
        }




        #region Delete calls

        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id">Entity ID</param>
        /// <param name="appId"></param>
        /// <param name="force">try to force-delete</param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        [HttpGet]
        public void Delete(string contentType, int id, int? appId = null, bool force = false)
        {
            SetAppIdAndUser(appId);

            //var found = AppManager.Read.Entities.Get(id);
            //if (found.Type.Name != contentType && found.Type.StaticName != contentType)
            //    throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");

            // check if it has related items or another reason to prevent deleting
            //var deleteControl = AppManager.Entities.DeletePossible(id);
            //if (deleteControl || force)
                AppManager.Entities.Delete(id, contentType, force);
            //else
            //    throw new InvalidOperationException($"Item {id} cannot be deleted. It is used by other items: {AppManager.Entities.DeleteHinderance(id)}");
        }

        /// <summary>
        /// Delete the entity specified by GUID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="entityGuid">Entity GUID</param>
        /// <param name="appId"></param>
        /// <param name="force"></param>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        [HttpGet]
        public void Delete(string contentType, Guid entityGuid, int? appId = null, bool force = false)
        {
            SetAppIdAndUser(appId);
            var entity = AppManager.Read.Entities.Get(entityGuid);
            Delete(contentType, entity.EntityId, force: force);
        }


        #endregion


        #region History

        [HttpGet]
        public List<ItemHistory> History(int appId, int entityId)
        {
            SetAppIdAndUser(appId);
            var versions = AppManager.Entities.VersionHistory(entityId);
            return versions;
        }

        [HttpGet]
        public bool Restore(int appId, int entityId, int changeId)
        {
            SetAppIdAndUser(appId);
            AppManager.Entities.VersionRestore(entityId, changeId);
            return true;
        }
        #endregion
    }
}