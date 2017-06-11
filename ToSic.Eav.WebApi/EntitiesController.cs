using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.ImportExport.Versioning;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.WebApi
{
    /// <summary>
    /// Web API Controller for various actions
    /// </summary>
    public class EntitiesController : Eav3WebApiBase
    {
        public EntitiesController(int appId) : base(appId) { }
        public EntitiesController()
        { }

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
                        RepoId = duplicateFrom.HasValue ? 0 : found.RepositoryId,
                        Guid = duplicateFrom.HasValue ? Guid.Empty : found.EntityGuid,
                        Type = new Formats.Type { Name = found.Type.Name, StaticName = found.Type.StaticName },
                        IsPublished = found.IsPublished,
                        IsBranch = (!found.IsPublished && found.GetPublished() != null),
                        TitleAttributeName = found.Title?.Name,
                        Attributes = found.Attributes.ToDictionary(a => a.Key, a => new Formats.Attribute()
                        {
                            Values = a.Value.Values?.Select(v => new ValueSet
                            {
                                Value = v.SerializableObject,  //v.Serialized, // Data.Value.GetValueModel(a.Value.Type, v., //
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
            SetAppIdAndUser(appId);

            // clean up content-type names in case it's using the nice-name instead of the static name...
            //var cache = DataSource.GetCache(null, appId);
            foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
            {
                var ct = AppManager.Read.ContentTypes.Get(itm.ContentTypeName);// cache.GetContentType(itm.ContentTypeName);
                if (ct == null)
                {
                    if (itm.ContentTypeName.StartsWith("@"))
                    {
                        items.Remove(itm);
                        continue;
                    }
                    throw new Exception("Can't find content type " + itm.ContentTypeName);
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
                i.Header.Guid = (i.Entity != null && i.Entity.Guid != Guid.Empty)
                    ? i.Entity.Guid
                    : Guid.NewGuid();

            foreach (var itm in list.Where(i => i.Header.ContentTypeName == null && i.Entity != null))
                itm.Header.ContentTypeName = itm.Entity.Type.StaticName;

            return list;
        }


        #endregion


        [HttpPost]
        public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<EntityWithHeader> items)
        {
            SetAppIdAndUser(appId);

            var entitiesToImport = new List<ImpEntity>();

            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                i.Entity.Guid = i.Header.Guid;

            foreach (var entity in items)
                if (entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty) // skip the ones which "shouldn't" be saved
                    entitiesToImport.Add(CreateImportEntity(entity));

            // Create Import-controller & run import
            var importController = new DbImport(null, appId, dontUpdateExistingAttributeValues: false,
                keepAttributesMissingInImport: false,
                preventUpdateOnDraftEntities: false,
                largeImport: false);
            importController.ImportIntoDb(null, entitiesToImport.ToArray());
            SystemManager.Purge(appId);

            // find / update IDs of items updated to return to client
            var foundItems = items.Select(e =>
            {
                var foundEntity = AppManager.Read.Entities.Get(e.Header.Guid);
                if (foundEntity == null)
                    return null;
                if (foundEntity.GetDraft() != null)
                    return foundEntity.GetDraft();
                return foundEntity;
            }).Where(e => e != null);

            var idList = foundItems.ToDictionary(f => f.EntityGuid, f => f.EntityId);

            return idList;
        }


        private ImpEntity CreateImportEntity(EntityWithHeader editInfo)
        {
            var newEntity = editInfo.Entity;
            var metadata = editInfo.Header.Metadata;

            #region initial data quality checks
            if (newEntity.Id == 0 && newEntity.Guid == Guid.Empty)
                throw new Exception("Item must have a GUID");
            #endregion

            // TODO 2tk: Refactor code - we use methods from XML import extensions!
            var importEntity = new ImpEntity
            {

                #region Guids, Ids, Published, Content-Types
                EntityGuid = newEntity.Guid,
                IsPublished = newEntity.IsPublished,
                ForceNoBranch = !newEntity.IsBranch, // if it's not a branch, it should also force no branch...
                AttributeSetStaticName = newEntity.Type.StaticName,
                #endregion
                // Metadata maybe?
                //KeyTypeId = Constants.NotMetadata  // default case
            };

            #region Metadata if we have any
            // todo: as the objects are of the same type, we can probably remove the Format.Metadata-type soon...
            if (metadata != null && metadata.HasMetadata)
                importEntity.Metadata = new Data.Metadata
                {
                    TargetType = metadata.TargetType,
                    KeyGuid = metadata.KeyGuid,
                    KeyNumber = metadata.KeyNumber,
                    KeyString = metadata.KeyString
                };

            #endregion

            // Attributes
            importEntity.Values = new Dictionary<string, List<IImpValue>>();


            // only transfer the fields / values which exist in the content-type definition
            var attributeSet = AppManager.Read.ContentTypes.Get(newEntity.Type.StaticName);// DataSource.GetCache(null, appId).GetContentType(newEntity.Type.StaticName);
            foreach (var attribute in newEntity.Attributes)
            {
                var attDef = attributeSet[attribute.Key];
                var attributeType = attDef.Type;

                // don't save anything of the type empty - this is heading-only
                if (attributeType == AttributeTypeEnum.Empty.ToString())
                    continue;

                foreach (var value in attribute.Value.Values)
                {
                    var stringValue = ImpEntity.ConvertValueObjectToString(value.Value);
                    var importValue = importEntity.AppendAttributeValue(attribute.Key, stringValue, attributeType);

                    // append languages OR empty language as fallback
                    if (value.Dimensions == null)
                    {   // Must this be done to save entities
                        importValue.AppendLanguageReference("", false);
                        continue;
                    }
                    foreach (var dimension in value.Dimensions)
                        importValue.AppendLanguageReference(dimension.Key, dimension.Value);

                }
            }

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

            var found = AppManager.Read.Entities.Get(id);
            if (found.Type.Name != contentType && found.Type.StaticName != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");

            // check if it has related items or another reason to prevent deleting
            var deleteControl = AppManager.Entities.DeletePossible(id);
            if (deleteControl || force)
                AppManager.Entities.Delete(id, force);
            else
                throw new InvalidOperationException($"Item {id} cannot be deleted. It is used by other items: {AppManager.Entities.DeleteHinderance(id)}");
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
            var versions = AppManager.Entities.GetHistory(entityId);// CurrentContext.Versioning.GetEntityHistory(entityId);
            return versions;
        }

        [HttpGet]
        public bool Restore(int appId, int entityId, int changeId, int defaultCultureDimension = 0)
        {
            //throw new Exception("this is not tested yet!");
            SetAppIdAndUser(appId);
            AppManager.Entities.RestorePrevious(entityId, changeId, defaultCultureDimension); // CurrentContext.Versioning.RestoreEntityVersion(entityId, changeId, DefaultCultureDimension);
            return true;
        }
        #endregion
    }
}