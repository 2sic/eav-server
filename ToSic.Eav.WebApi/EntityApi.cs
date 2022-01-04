using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Errors;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.WebApi.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
    public class EntityApi: HasLog
    {
        #region DI Constructor & Init

        public EntityApi(AppRuntime appRuntime, Lazy<AppManager> appManagerLazy, Lazy<IConvertToEavLight> entitiesToDicLazy) : base("Api.Entity")
        {
            _appRuntime = appRuntime;
            _appManagerLazy = appManagerLazy;
            _entitiesToDicLazy = entitiesToDicLazy;
        }
        private readonly AppRuntime _appRuntime;
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly Lazy<IConvertToEavLight> _entitiesToDicLazy;
        public AppRuntime AppRead;

        public EntityApi Init(int appId, bool showDrafts, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            AppRead = _appRuntime.Init(appId, showDrafts, Log);
            return this;
        }

        #endregion

        #region Lazy Helpers

        /// <summary>
        /// The serializer, so it can be configured from outside if necessary
        /// </summary>
        private IConvertToEavLight EntityToDic
        {
            get
            {
                if (_entitiesToDictionary != null) return _entitiesToDictionary;
                _entitiesToDictionary = _entitiesToDicLazy.Value;
                _entitiesToDictionary.WithGuid = true;
                (_entitiesToDictionary as ConvertToEavLight)?.Init(Log);
                return _entitiesToDictionary;
            }
        }
        private IConvertToEavLight _entitiesToDictionary;

        #endregion

        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
        public IEnumerable<IDictionary<string, object>> GetEntities(string contentType) 
            => EntityToDic.Convert(AppRead.Entities.Get(contentType));

        public List<BundleWithHeader<IEntity>> GetEntitiesForEditing(List<ItemIdentifier> items)
        {
            ReplaceSimpleTypeNames(items);

            var list = items.Select(p =>
            {
                var ent = p.EntityId != 0 || p.DuplicateEntity.HasValue
                        ? GetEditableEditionAndMaybeCloneIt(p)
                        : null;
                return new BundleWithHeader<IEntity>()
                {
                    Header = p,
                    Entity = ent
                };
            }).ToList();

            // make sure the header has the right "new" guid as well - as this is the primary one to work with
            // it is really important to use the header guid, because sometimes the entity does not exist - so it doesn't have a guid either
            var itemsToCorrect = list.Where(i => i.Header.Guid == Guid.Empty).ToArray(); // must do toArray, to prevent re-checking after setting the guid
            foreach (var bundle in itemsToCorrect)
            {
                var hasEntity = bundle.Entity != null;
                var useExistingGuid = hasEntity && bundle.Entity.EntityGuid != Guid.Empty;
                bundle.Header.Guid = useExistingGuid
                    ? bundle.Entity.EntityGuid
                    : Guid.NewGuid();
                if (hasEntity && !useExistingGuid)
                    (bundle.Entity as Entity).SetGuid(bundle.Header.Guid);
            }

            // Update header with ContentTypeName in case it wasn't there before
            foreach (var itm in list.Where(i => i.Header.ContentTypeName == null && i.Entity != null))
                itm.Header.ContentTypeName = itm.Entity.Type.StaticName;

            // Add EditInfo for read-only data
            foreach (var bundle in list) 
                bundle.Header.EditInfo = new EditInfoDto(bundle.Entity);

            return list;
        }


        private IEntity GetEditableEditionAndMaybeCloneIt(ItemIdentifier p)
        {
            var found = AppRead.AppState.List.GetOrThrow(p.ContentTypeName, p.DuplicateEntity ?? p.EntityId);
            // if there is a draft, only allow editing that
            found = found.GetDraft() ?? found;

            if (!p.DuplicateEntity.HasValue) return found;

            var copy = EntityBuilder.FullClone(found, found.Attributes, null);
            copy.SetGuid(Guid.Empty);
            copy.ResetEntityId();
            return copy;
        }

        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id">Entity ID</param>
        /// <param name="force">try to force-delete</param>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(string contentType, int id, bool force = false, int? parentId = null, string parentField = null) 
            => _appManagerLazy.Value.Init(AppRead, AppRead.Log).Entities.Delete(id, contentType, force, false, parentId, parentField);

        /// <summary>
        /// Delete the entity specified by GUID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="entityGuid">Entity GUID</param>
        /// <param name="force"></param>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(string contentType, Guid entityGuid, bool force = false, int? parentId = null, string parentField = null) 
            => Delete(contentType, AppRead.Entities.Get(entityGuid).EntityId, force, parentId, parentField);


        /// <summary>
        /// clean up content-type names in case it's using the nice-name instead of the static name...
        /// </summary>
        /// <param name="items"></param>
        private void ReplaceSimpleTypeNames(List<ItemIdentifier> items)
        {
            foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
            {
                var ct = AppRead.ContentTypes.Get(itm.ContentTypeName);
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
        }

        // 2020-12-08 2dm - unused code, disable for now, delete ca. Feb 2021
        public EntityApi InitOrThrowBasedOnGrants(IContextOfSite context, IAppIdentity app, string contentType, List<Eav.Security.Grants> requiredGrants, ILog parentLog)
        {
            var permCheck = _appManagerLazy.Value.ServiceProvider.Build<MultiPermissionsTypes>().Init(context, app, contentType, parentLog);
            if (!permCheck.EnsureAll(requiredGrants, out var error))
                throw HttpException.PermissionDenied(error);
            return Init(app.AppId, true, parentLog);
        }

        public IEnumerable<Dictionary<string, object>> GetEntitiesForAdmin(string contentType)
        {
            var wrapLog = Log.Call(useTimer: true);
            EntityToDic.ConfigureForAdminUse();
            var originals = AppRead.Entities.Get(contentType).ToList();
            var list = EntityToDic.Convert(originals).ToList();

            var timer = Log.Call(null, "truncate dictionary", useTimer: true);
            var result = list
                .Select(li => li.ToDictionary(x1 => x1.Key, x2 => Truncate(x2.Value, 50)))
                .ToList();
            timer("ok");

            wrapLog(result.Count.ToString());
            return result;
        }


        private object Truncate(object value, int length)
        {
            if (!(value is string asTxt))
                return value;

            if (asTxt.Length > length)
                asTxt = asTxt.Substring(0, length);
            return asTxt;
        }
    }
}
