using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeSet : BllCommandBase
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
            => DbContext.SqlDb.ToSicEavAttributeSets
            .Include(a => a.ToSicEavAttributesInSets)
                .ThenInclude(a => a.Attribute)
            .Where(a => a.AppId == DbContext.AppId && !a.ChangeLogDeleted.HasValue)
            .ToList();


        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public ToSicEavAttributeSets GetAttributeSet(int attributeSetId)
            => DbContext.SqlDb.ToSicEavAttributeSets
            .SingleOrDefault(
                a => a.AttributeSetId == attributeSetId
                     && a.AppId == DbContext.AppId
                     && !a.ChangeLogDeleted.HasValue);

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public ToSicEavAttributeSets GetAttributeSet(string staticName)
            => DbContext.SqlDb.ToSicEavAttributeSets.SingleOrDefault(
                a => a.StaticName == staticName
                     && a.AppId == DbContext.AppId
                     && !a.ChangeLogDeleted.HasValue);

        public ToSicEavAttributeSets GetAttributeSetWithEitherName(string name)
        {
            var appId = DbContext.AppId;

            try
            {
                var found = DbContext.SqlDb.ToSicEavAttributeSets.Where(s =>
                        s.AppId == appId
                        && s.StaticName == name
                        && !s.ChangeLogDeleted.HasValue)
                    .ToList();

                // if not found, try the non-static name as fallback
                if (found.Count == 0)
                    found = DbContext.SqlDb.ToSicEavAttributeSets.Where(s =>
                            s.AppId == appId
                            && s.Name == name
                            && !s.ChangeLogDeleted.HasValue)
                        .ToList();

                if (found.Count != 1)
                    throw new Exception("too many or to few content types found");

                return found.First();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to get AttributeSet with StaticName \"" + name + "\" in app " + appId /* + " and Scope \"" + scopeFilter + "\"."*/, ex);
            }
        }

        public int GetAttributeSetIdWithEitherName(string name) 
            => GetAttributeSetWithEitherName(name).AttributeSetId;


        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        internal bool AttributeSetExists(int appId, string staticName)
            => DbContext.SqlDb.ToSicEavAttributeSets.Any(
                a => !a.ChangeLogDeleted.HasValue
                     && a.AppId == appId
                     && a.StaticName == staticName);


        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<ToSicEavAttributeSets> GetAttributeSets(int appId, AttributeScope? scope)
        {
            var result = DbContext.SqlDb.ToSicEavAttributeSets
                .Where(a => a.AppId == appId && !a.ChangeLogDeleted.HasValue);

            if (scope != null)
            {
                var scopeString = scope.ToString();
                result = result.Where(a => a.Scope == scopeString);
            }

            return result;
        }

        

        internal ToSicEavAttributeSets PrepareSet(string name, string description, string staticName, string scope, /*bool autoSave,*/ bool skipExisting, int? appId)
        {
            if (string.IsNullOrEmpty(staticName))
                staticName = Guid.NewGuid().ToString();

            var targetAppId = appId ?? DbContext.AppId;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (DbContext.AttribSet.AttributeSetExists(targetAppId, staticName))
            {
                if (skipExisting)
                    return null;
                throw new Exception("An AttributeSet with StaticName \"" + staticName + "\" already exists.");
            }

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

            if (DbContext.AttribSet.ContentTypes.ContainsKey(DbContext.AppId))
                DbContext.AttribSet.ContentTypes.Remove(DbContext.AppId);

            //if (autoSave)
            //    DbContext.SqlDb.SaveChanges();

            return newSet;
        }
        
    }
}
