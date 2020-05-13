using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Conversion;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Formats;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
    public class EntityApi: HasLog
    {
        //public AppManager AppManager;
        public AppRuntime AppRead;

        public EntityApi(int appId, bool showDrafts, ILog parentLog): base("Api.EntPrc", parentLog)
        {
            //AppManager = new AppManager(appId, Log);
            AppRead = new AppRuntime(appId, showDrafts, Log);
        }

        /// <summary>
        /// The serializer, so it can be configured from outside if necessary
        /// </summary>
        public EntitiesToDictionary Serializer
        {
            get
            {
                if (_entitiesToDictionary != null) return _entitiesToDictionary;
                _entitiesToDictionary = Factory.Resolve<EntitiesToDictionary>();
                _entitiesToDictionary.WithGuid = true;
                return _entitiesToDictionary;
            }
        }
        private EntitiesToDictionary _entitiesToDictionary;

        public IEntity GetOrThrow(string contentType, int id)
        {
            // must use cache, because it shows both published  unpublished
            var found = AppRead.Entities.Get(id);
            if (contentType != null && !(found.Type.Name == contentType || found.Type.StaticName == contentType))
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "'");
            return found;
        }


        public IEntity GetOrThrow(string contentType, Guid guid)
        {
            // must use cache, because it shows both published  unpublished
            var itm = AppRead.Entities.Get(guid);
            if (itm == null || (contentType != null && !(itm.Type.Name == contentType || itm.Type.StaticName == contentType)))
                throw new KeyNotFoundException("Can't find " + guid + "of type '" + contentType + "'");
            return itm;
        }


        /// <summary>
        /// this is needed by 2sxc
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetOne(string contentType, int id, string cultureCode = null)
        {
            var found = GetOrThrow(contentType, id);
            return Serializer.Convert(found);
        }

        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
        public IEnumerable<Dictionary<string, object>> GetEntities(string contentType, string cultureCode = null) 
            => Serializer.Convert(AppRead.Entities.Get(contentType));

        public List<BundleIEntity> GetEntitiesForEditing(int appId, List<ItemIdentifier> items)
        {
            ReplaceSimpleTypeNames(items);

            var list = items.Select(p =>
            {
                var ent = p.EntityId != 0 || p.DuplicateEntity.HasValue
                        ? GetEditableEditionAndMaybeCloneIt(p)
                        : null;
                return new BundleIEntity
                {
                    Header = p,
                    Entity = ent
                };
            }).ToList();

            // make sure the header has the right "new" guid as well - as this is the primary one to work with
            // it is really important to use the header guid, because sometimes the entity does not exist - so it doesn't have a guid either
            var itemsToCorrect = list.Where(i => i.Header.Guid == Guid.Empty).ToArray(); // must do toArray, to prevent re-checking after setting the guid
            foreach (var i in itemsToCorrect)
            {
                var hasEntity = i.Entity != null;
                var useExistingGuid = hasEntity && i.Entity.EntityGuid != Guid.Empty;
                i.Header.Guid = useExistingGuid
                    ? i.Entity.EntityGuid
                    : Guid.NewGuid();
                if (hasEntity && !useExistingGuid)
                    (i.Entity as Entity).SetGuid(i.Header.Guid);
            }

            foreach (var itm in list.Where(i => i.Header.ContentTypeName == null && i.Entity != null))
                itm.Header.ContentTypeName = itm.Entity.Type.StaticName;
            return list;
        }

        public IEntity GetEditableEditionAndMaybeCloneIt(ItemIdentifier p)
        {
            var found = GetOrThrow(p.ContentTypeName, p.DuplicateEntity ?? p.EntityId);
            // if there is a draft, only allow editing that
            found = found.GetDraft() ?? found;

            if (!p.DuplicateEntity.HasValue) return found;

            var copy = EntityBuilder.FullClone(found, found.Attributes, null);
            copy.SetGuid(Guid.Empty);
            copy.ResetEntityId(0);
            return copy;
        }

        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id">Entity ID</param>
        /// <param name="force">try to force-delete</param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(string contentType, int id, bool force = false)
        {
            new AppManager(AppRead, AppRead.Log).Entities.Delete(id, contentType, force);
        }

        /// <summary>
        /// Delete the entity specified by GUID.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="entityGuid">Entity GUID</param>
        /// <param name="force"></param>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(string contentType, Guid entityGuid, bool force = false)
        {
            var entity = AppRead.Entities.Get(entityGuid);
            Delete(contentType, entity.EntityId, force);
        }

        public IEnumerable<Dictionary<string, object>> GetEntitiesForAdmin(string contentType)
        {
            Serializer.ConfigureForAdminUse();

            var list = Serializer.Convert(AppRead.Entities.Get(contentType));

            var newList = list
                .Select(li
                    => li.ToDictionary(x1 => x1.Key, x2 => TruncateLongStrings(x2.Value, 50)))
                .ToList();

            return newList;
        }


        /// <summary>
        /// clean up content-type names in case it's using the nice-name instead of the static name...
        /// </summary>
        /// <param name="items"></param>
        internal void ReplaceSimpleTypeNames(List<ItemIdentifier> items)
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

        internal object TruncateLongStrings(object value, int length)
        {
            if (!(value is string asTxt))
                return value;

            if (asTxt.Length > length)
                asTxt = asTxt.Substring(0, length);
            return asTxt;
        }
    }
}
