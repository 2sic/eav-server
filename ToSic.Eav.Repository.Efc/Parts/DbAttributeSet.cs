using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbAttributeSet : BllCommandBase
    {
        public DbAttributeSet(DbDataController dc) : base(dc) { }

        /// <summary>caches all AttributeSets for each App</summary>
        internal Dictionary<int, Dictionary<int, IContentType>> ContentTypes = new Dictionary<int, Dictionary<int, IContentType>>();

        #region Testing / Analytics helpers
        internal void ResetCacheForTesting()
            => ContentTypes = new Dictionary<int, Dictionary<int, IContentType>>();
        #endregion


        /// <summary>
        /// Get a List of all AttributeSets
        /// </summary>
        public List<ToSicEavAttributeSets> GetAllAttributeSets()
            => DbContext.SqlDb.ToSicEavAttributeSets.Where(a => a.AppId == DbContext.AppId && !a.ChangeLogDeleted.HasValue).ToList();


        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public ToSicEavAttributeSets GetAttributeSet(int attributeSetId)
            => DbContext.SqlDb.ToSicEavAttributeSets.SingleOrDefault(
                    a => a.AttributeSetId == attributeSetId && a.AppId == DbContext.AppId && !a.ChangeLogDeleted.HasValue);

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public ToSicEavAttributeSets GetAttributeSet(string staticName)
            => DbContext.SqlDb.ToSicEavAttributeSets.SingleOrDefault(
                    a => a.StaticName == staticName && a.AppId == DbContext.AppId && !a.ChangeLogDeleted.HasValue);

        public ToSicEavAttributeSets GetAttributeSetWithEitherName(string name)
        {
            //var scopeFilter = scope?.ToString();
            var appId = DbContext.AppId /*_appId*/;

            try
            {
                //var test = Context.SqlDb.ToSicEavAttributeSets.Where(s =>
                //             s.StaticName == name && !s.ChangeLogDeleted.HasValue).ToList();
                var found = DbContext.SqlDb.ToSicEavAttributeSets.Where(s =>
                            s.AppId == appId
                            && s.StaticName == name
                            && !s.ChangeLogDeleted.HasValue
                            //&& (s.Scope == scopeFilter || scopeFilter == null)
                            ).ToList();
                // if not found, try the non-static name as fallback
                if (found.Count() == 0)
                    found = DbContext.SqlDb.ToSicEavAttributeSets.Where(s =>
                            s.AppId == appId
                            && s.Name == name
                            && !s.ChangeLogDeleted.HasValue
                            //&& (s.Scope == scopeFilter || scopeFilter == null)
                            ).ToList();

                if (found.Count() != 1)
                    throw new Exception("too many or to fewe content types found");

                return found.First();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to get AttributeSet with StaticName \"" + name + "\" in app " + appId /* + " and Scope \"" + scopeFilter + "\"."*/, ex);
            }
        }

        /// <summary>
        /// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
        /// </summary>
        /// <param name="attributeSetId">AttributeSetId to resolve</param>
        internal int ResolveAttributeSetId(int attributeSetId)
        {
            var usesConfigurationOfAttributeSet = DbContext.SqlDb.ToSicEavAttributeSets.Where(a => a.AttributeSetId == attributeSetId).Select(a => a.UsesConfigurationOfAttributeSet).Single();
            return usesConfigurationOfAttributeSet ?? attributeSetId;
        }

        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        public bool AttributeSetExists(string staticName, int appId)
        {
            return DbContext.SqlDb.ToSicEavAttributeSets.Any(a => !a.ChangeLogDeleted.HasValue && a.AppId == appId && a.StaticName == staticName);
        }



        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<ToSicEavAttributeSets> GetAttributeSets(int appId, AttributeScope? scope)
        {
            var result = DbContext.SqlDb.ToSicEavAttributeSets.Where(a => a.AppId == appId && !a.ChangeLogDeleted.HasValue);

            if (scope != null)
            {
                var scopeString = scope.ToString();
                result = result.Where(a => a.Scope == scopeString);
            }

            return result;
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. App must be saved and have an AppId
        /// </summary>
        internal void EnsureSharedAttributeSets(ToSicEavApps app, bool autoSave = true)
        {
            if (app.AppId == 0)
                throw new Exception("App must have a valid AppId");

            // todo: bad - don't want data-sources here
            var sharedAttributeSets = GetAttributeSets(Constants.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration);
            foreach (var sharedSet in sharedAttributeSets)
            {
                // Skip if attributeSet with StaticName already exists
                if (app.ToSicEavAttributeSets.Any(a => a.StaticName == sharedSet.StaticName && !a.ChangeLogDeleted.HasValue))
                    continue;

                // create new AttributeSet
                var newAttributeSet = AddContentTypeAndSave(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, false, app.AppId);
                newAttributeSet.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetId;
            }

            // Ensure new AttributeSets are created and cache is refreshed
            if (autoSave)
                DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on all Apps an Zones
        /// </summary>
        public void EnsureSharedAttributeSets()
        {
            foreach (var app in DbContext.SqlDb.ToSicEavApps)
                EnsureSharedAttributeSets(app, false);

            DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Add a new AttributeSet
        /// </summary>
        public ToSicEavAttributeSets AddContentTypeAndSave(string name, string description, string staticName, string scope, bool autoSave = true)
            => AddContentTypeAndSave(name, description, staticName, scope, autoSave, null);
        

        internal ToSicEavAttributeSets AddContentTypeAndSave(string name, string description, string staticName, string scope, bool autoSave, int? appId)
        {
            if (string.IsNullOrEmpty(staticName))
                staticName = Guid.NewGuid().ToString();

            var targetAppId = appId ?? DbContext.AppId;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (DbContext.AttribSet.AttributeSetExists(staticName, targetAppId))
                throw new Exception("An AttributeSet with StaticName \"" + staticName + "\" already exists.");

            var newSet = new ToSicEavAttributeSets
            {
                Name = name,
                StaticName = staticName,
                Description = description,
                Scope = scope,
                ChangeLogCreated = DbContext.Versioning.GetChangeLogId(),
                AppId = targetAppId
            };

            DbContext.SqlDb.Add(newSet);

            if (DbContext.AttribSet.ContentTypes.ContainsKey(DbContext.AppId /* _appId*/))
                DbContext.AttribSet.ContentTypes.Remove(DbContext.AppId /* _appId*/);

            if (autoSave)
                DbContext.SqlDb.SaveChanges();

            return newSet;
        }
        
    }
}
