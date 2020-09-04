using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {

        private IQueryable<ToSicEavEntities> EntityQuery
        {
            get
            {
                DbContext.Log.Add(nameof(EntityQuery));
                return DbContext.SqlDb.ToSicEavEntities
                    .Include(e => e.RelationshipsWithThisAsParent)
                    .Include(e => e.RelationshipsWithThisAsChild)
                    .Include(e => e.ToSicEavValues)
                    .ThenInclude(v => v.ToSicEavValuesDimensions)
                    .ThenInclude(d => d.Dimension);
            }
        }


        private IQueryable<ToSicEavEntities> IncludeMultiple(IQueryable<ToSicEavEntities> origQuery, string additionalTables)
        {
            additionalTables.Split(',').ToList().ForEach(a => origQuery = origQuery.Include(a.Trim()));
            return origQuery;
        }

        /// <summary>
        /// Get a single Entity by EntityId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        internal ToSicEavEntities GetDbEntity(int entityId)
        {
            var wraplog = DbContext.Log.Call<ToSicEavEntities>($"Get {entityId}");
            var found = EntityQuery.Single(e => e.EntityId == entityId);
            return wraplog("ok", found);
        }

        private ToSicEavEntities GetDbEntity(int entityId, string includes)
            => IncludeMultiple(EntityQuery, includes).Single(e => e.EntityId == entityId);

        /// <summary>
        /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        internal ToSicEavEntities GetMostCurrentDbEntity(Guid entityGuid)
            // GetEntity should never return a draft entity that has a published version
        {
            var x = GetEntitiesByGuid(entityGuid);
            return x.Single(e => !e.PublishedEntityId.HasValue);
        }


        internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid)
            => EntityQuery.Where(e => e.EntityGuid == entityGuid
                                      && !e.ChangeLogDeleted.HasValue
                                      && !e.AttributeSet.ChangeLogDeleted.HasValue
                                      && e.AppId == DbContext.AppId);


        /// <summary>
        /// Test whether Entity exists on current App and is not deleted
        /// </summary>
        internal bool EntityExists(Guid entityGuid) => GetEntitiesByGuid(entityGuid).Any();

    }
}
