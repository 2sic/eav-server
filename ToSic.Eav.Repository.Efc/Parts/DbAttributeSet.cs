using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Enums;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeSet : BllCommandBase
    {
        public DbAttributeSet(DbDataController dc) : base(dc, "Db.AttSet") { }

        private IQueryable<ToSicEavAttributeSets> GetSetCoreQuery(int? appId = null)
            => DbContext.SqlDb.ToSicEavAttributeSets
                .Include(a => a.ToSicEavAttributesInSets)
                .ThenInclude(a => a.Attribute)
                .Where(a => a.AppId == (appId ?? DbContext.AppId) && !a.ChangeLogDeleted.HasValue);

        /// <summary>
        /// Get a List of all AttributeSets
        /// </summary>
        internal List<ToSicEavAttributeSets> GetDbAttribSets()
            => GetSetCoreQuery().ToList();


        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        internal ToSicEavAttributeSets GetDbAttribSet(int attributeSetId)
            => GetSetCoreQuery().SingleOrDefault(a => a.AttributeSetId == attributeSetId);

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public ToSicEavAttributeSets GetDbAttribSet(string staticName)
            => GetSetCoreQuery().SingleOrDefault(a => a.StaticName == staticName);


        private ToSicEavAttributeSets GetDbAttribSetWithEitherName(string name)
        {
            var appId = DbContext.AppId;

            try
            {
                var found = GetSetCoreQuery()
                    .Where(s => s.StaticName == name)
                    .ToList();

                // if not found, try the non-static name as fallback
                if (found.Count == 0)
                    found = GetSetCoreQuery()
                        .Where(s => s.Name == name)
                        .ToList();

                if (found.Count != 1)
                    throw new Exception($"too many or too few content types found for the content-type {name} - found {found.Count}");

                return found.First();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Unable to get Content-Type/AttributeSet with StaticName \"{name}\" in app {appId}", ex);
            }
        }

        public int GetIdWithEitherName(string name) => GetDbAttribSetWithEitherName(name).AttributeSetId;


        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        internal bool DbAttribSetExists(int appId, string staticName)
            => GetSetCoreQuery(appId).Any(a => a.StaticName == staticName);


        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<ToSicEavAttributeSets> GetDbAttribSets(int appId, AttributeScope? scope)
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

        

        internal ToSicEavAttributeSets PrepareDbAttribSet(string name, string description, string staticName, string scope, bool skipExisting, int? appId)
        {
            if (string.IsNullOrEmpty(staticName))
                staticName = Guid.NewGuid().ToString();

            var targetAppId = appId ?? DbContext.AppId;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (DbContext.AttribSet.DbAttribSetExists(targetAppId, staticName))
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

            return newSet;
        }
        
    }
}
