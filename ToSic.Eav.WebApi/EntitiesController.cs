using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Api;
using ToSic.Eav.DataSources;
using ToSic.Eav.Persistence;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.ImportExport.Refactoring.Extensions;

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
        public IEntity GetEntityOrThrowError(string contentType, int id, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            // must use cache, because it shows both published  unpublished
            var found = DataSource.GetCache(null, AppId).List[id];
            if (contentType != null && !(found.Type.Name == contentType || found.Type.StaticName == contentType))
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
            return found;
        }

        public IEntity GetEntityOrThrowError(string contentType, Guid guid, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            // must use cache, because it shows both published  unpublished
            var list = DataSource.GetCache(null, AppId).LightList;
            var itm = list // pre-fetch for security and content-type check
                .FirstOrDefault(e => e.EntityGuid == guid);

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
            var found = GetEntityOrThrowError(contentType, id, appId);
            return Serializer.Prepare(found);
        }

        /// <summary>
		/// Get all Entities of specified Type
		/// </summary>
		public IEnumerable<Dictionary<string, object>> GetEntities(string contentType, string cultureCode = null, int? appId = null)
		{
            if (appId.HasValue)
                AppId = appId.Value;

			var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
			typeFilter.TypeName = contentType;

            return Serializer.Prepare(typeFilter.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
		}

	    public IEnumerable<Dictionary<string, object>> GetAllOfTypeForAdmin(int appId, string contentType)
	    {
	        SetAppIdAndUser(appId);
	        Serializer.ConfigureForAdminUse();

	        var api = new BetaFullApi(null, appId, CurrentContext);
	        var list = Serializer.Prepare(api.GetEntitiesOfType(contentType));

	        var newList = list.Select(li => li.ToDictionary(x1 => x1.Key, x2 => TruncateIfString(x2.Value, 50))).ToList();

	        return newList;
	    }

	    public object TruncateIfString(object value, int length)
	    {
	            var asTxt = value as string;
                if (asTxt == null)
                    return value;

                if (asTxt.Length > length)
                    asTxt = asTxt.Substring(0, length);
                return asTxt;
	    }

        //2015-08-29 2dm: these commands are kind of ready, but not in use yet
        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
     //   public IEnumerable<Dictionary<string, object>> GetEntitiesByType(int appId, string contentType, int pageSize = 1000, int pageNumber = 1)
     //   {
     //       AppId = appId;

     //       var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
     //       typeFilter.TypeName = contentType;
     //       var paging = DataSource.GetDataSource<Paging>(upstream: typeFilter);
     //       paging.PageNumber = pageNumber;
     //       paging.PageSize = pageSize;

     //       return Serializer.Prepare(paging.LightList);//  typeFilter.List.Select(t => Helpers.GetEntityValues(t.Value, cultureCode: cultureCode));
     //   }

	    //public int CountEntitiesOfType(int appId, string contentType)
	    //{
     //       AppId = appId;

     //       var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: InitialDS);
     //       typeFilter.TypeName = contentType;
	    //    return typeFilter.LightList.Count();
	    //}

        public EntityWithLanguages GetOne(int appId, string contentType, int id, int? duplicateFrom = null, string format = "multi-language")
        {
            switch (format)
            {
                case "multi-language":
                    Serializer.IncludeAllEditingInfos = true;

                    var found = GetEntityOrThrowError(contentType, duplicateFrom ?? id, appId);
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
            // clean up content-type names in case it's using the nice-name instead of the static name...
            var cache = DataSource.GetCache(null, appId);
            foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
            {
                var ct = cache.GetContentType(itm.ContentTypeName);
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
            foreach(var i in list.Where(i => i.Header.Guid == Guid.Empty).ToArray()) // must do toarray, to prevent re-checking after setting the guid
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
            var entitiesToImport = new List<ImpEntity>();

            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                i.Entity.Guid = i.Header.Guid; 

            foreach (var entity in items)
                if (entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty) // skip the ones which "shouldn't" be saved
                    entitiesToImport.Add(CreateImportEntity(entity, appId));

            // Create Import-controller & run import
            var importController = new Import.Import(null, appId, /*UserIdentityToken,*/ 
                dontUpdateExistingAttributeValues: false, 
                keepAttributesMissingInImport: false,
                preventUpdateOnDraftEntities: false,
                largeImport: false);
            importController.RunImport(null, entitiesToImport.ToArray());


            // find / update IDs of items updated to return to client
            var cache = DataSource.GetCache(null, appId);
            var foundItems = items.Select(e =>
            {
                var foundEntity = cache.LightList.FirstOrDefault(c => e.Header.Guid == c.EntityGuid);
                if (foundEntity == null)
                    return null;
                if (foundEntity.GetDraft() != null)
                    return foundEntity.GetDraft();
                return foundEntity;
            }).Where(e => e != null);

            var IdList = foundItems.ToDictionary(f => f.EntityGuid, f => f.EntityId);

            return IdList;
        }


        private ImpEntity CreateImportEntity(EntityWithHeader editInfo, int appId)
        {
            var newEntity = editInfo.Entity;
            var metadata = editInfo.Header.Metadata;

            #region initial data quality checks
            if (newEntity.Id == 0 && newEntity.Guid == Guid.Empty)
                throw new Exception("Item must have a GUID");
            #endregion

            // TODO 2tk: Refactor code - we use methods from XML import extensions!
            var importEntity = new ImpEntity();

            #region Guids, Ids, Published, Content-Types
            importEntity.EntityGuid = newEntity.Guid;
            importEntity.IsPublished = newEntity.IsPublished;
            // 2dm 2016-06-29
            importEntity.ForceNoBranch = !newEntity.IsBranch; // if it's not a branch, it should also force no branch...
            importEntity.AttributeSetStaticName = newEntity.Type.StaticName;
            #endregion

            #region Metadata if we have any
            importEntity.KeyTypeId = Constants.DefaultAssignmentObjectTypeId;  // default case
            if (metadata != null && metadata.HasMetadata)
            {
                importEntity.KeyTypeId = metadata.TargetType;
                importEntity.KeyGuid = metadata.KeyGuid;
                importEntity.KeyNumber = metadata.KeyNumber;
                importEntity.KeyString = metadata.KeyString;
            }
            #endregion

            // Attributes
            importEntity.Values = new Dictionary<string, List<IImpValue>>();

            
            // only transfer the fields / values which exist in the content-type definition
            var attributeSet = DataSource.GetCache(null, appId).GetContentType(newEntity.Type.StaticName);
            foreach (var attribute in newEntity.Attributes)
            {
                var attDef = attributeSet[attribute.Key];
                var attributeType = attDef.Type;

                // don't save anything of the type empty - this is heading-only
                if(attributeType == AttributeTypeEnum.Empty.ToString())
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
	        if (appId.HasValue)
	            AppId = appId.Value;
	        var finalAppId = appId ?? AppId;
            var found = DataSource.GetCache(null, finalAppId).List[id];// InitialDS.List[id];
            if (found.Type.Name != contentType && found.Type.StaticName != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");

            // check if it has related items or another reason to prevent deleting
            var deleteControl = CurrentContext.Entities.CanDeleteEntity(id);
            if(deleteControl.Item1 || force)
                CurrentContext.Entities.DeleteEntity(id, force);
            else
                throw new InvalidOperationException("Item " + id +
                                                    " cannot be deleted. It is used by other items: " + deleteControl.Item2);
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
            if (appId.HasValue)
                AppId = appId.Value;
            var entity = CurrentContext.Entities.GetEntity(entityGuid);
            Delete(contentType, entity.EntityID, force: force);
        }


        #endregion


        #region History

	    //[HttpGet]
	    //public List<DbVersioning.EntityHistoryItem> History(int appId, int entityId)
	    //{
     //       SetAppIdAndUser(appId);
     //       var versions = CurrentContext.Versioning.GetEntityHistory(entityId);
	    //    return versions;
	    //}

	    //[HttpGet]
	    //public dynamic HistoryDetails(int appId, int entityId, int changeId)
	    //{
     //       SetAppIdAndUser(appId);
     //       var result = CurrentContext.Versioning.GetEntityVersionValues(entityId, changeId, null, null);
	    //    return result;
	    //}

	    //[HttpGet]
	    //public bool HistoryRestore(int appId, int entityId, int changeId)
	    //{
	    //    var DefaultCultureDimension = 0;
     //       throw  new Exception("this is not tested yet!");
     //       SetAppIdAndUser(appId);
     //       CurrentContext.Versioning.RestoreEntityVersion(entityId, changeId, DefaultCultureDimension);
	    //    return true;
     //   }
        #endregion
    }
}