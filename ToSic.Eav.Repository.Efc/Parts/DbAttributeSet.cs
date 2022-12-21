using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbAttributeSet : BllCommandBase
    {
        public DbAttributeSet(DbDataController dc) : base(dc, "Db.AttSet") { }

        private IQueryable<ToSicEavAttributeSets> GetSetCoreQuery(int? appId = null)
            => DbContext.SqlDb.ToSicEavAttributeSets
                .Include(a => a.ToSicEavAttributesInSets)
                .ThenInclude(a => a.Attribute)
                .Where(a => a.AppId == (appId ?? DbContext.AppId) && !a.ChangeLogDeleted.HasValue);

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


        internal int GetId(string name)
        {
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

                return found.First().AttributeSetId;
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Unable to get Content-Type/AttributeSet with StaticName \"{name}\" in app {DbContext.AppId}", ex);
            }
        }

        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        private bool DbAttribSetExists(int appId, string staticName)
            => GetSetCoreQuery(appId).Any(a => a.StaticName == staticName);

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        internal ToSicEavAttributeSets PrepareDbAttribSet(string name, /*string description,*/ string nameId, string scope, bool skipExisting, int? appId)
        {
            if (string.IsNullOrEmpty(nameId))
                nameId = Guid.NewGuid().ToString();

            var targetAppId = appId ?? DbContext.AppId;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (DbContext.AttribSet.DbAttribSetExists(targetAppId, nameId))
            {
                if (skipExisting)
                    return null;
                throw new Exception("An AttributeSet with StaticName \"" + nameId + "\" already exists.");
            }

            var newSet = new ToSicEavAttributeSets
            {
                Name = name,
                StaticName = nameId,
                Scope = scope,
                ChangeLogCreated = DbContext.Versioning.GetChangeLogId(),
                AppId = targetAppId
            };

            DbContext.SqlDb.Add(newSet);

            return newSet;
        }
        
    }
}
