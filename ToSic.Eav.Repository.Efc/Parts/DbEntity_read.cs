using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {

        private IQueryable<ToSicEavEntities> EntityQuery
            => DbContext.SqlDb.ToSicEavEntities
                .Include(e => e.RelationshipsWithThisAsParent)
                .Include(e => e.RelationshipsWithThisAsChild)
                .Include(e => e.ToSicEavValues)
                    .ThenInclude(v => v.ToSicEavValuesDimensions)
                        .ThenInclude(d => d.Dimension);


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
            => EntityQuery.Single(e => e.EntityId == entityId);

        internal ToSicEavEntities GetDbEntity(int entityId, string includes)
            => IncludeMultiple(EntityQuery, includes).Single(e => e.EntityId == entityId);

        /// <summary>
        /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public ToSicEavEntities GetMostCurrentDbEntity(Guid entityGuid)
            // GetEntity should never return a draft entity that has a published version
            => GetEntitiesByGuid(entityGuid).Single(e => !e.PublishedEntityId.HasValue);



        internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid)
            => EntityQuery.Where(e => e.EntityGuid == entityGuid && !e.ChangeLogDeleted.HasValue &&
                !e.AttributeSet.ChangeLogDeleted.HasValue && e.AttributeSet.AppId == DbContext.AppId);


        internal IQueryable<ToSicEavEntities> GetEntitiesByType(ToSicEavAttributeSets set)
        => EntityQuery.Where(e => e.AttributeSet == set);


        /// <summary>
        /// Test whether Entity exists on current App and is not deleted
        /// </summary>
        internal bool EntityExists(Guid entityGuid) => GetEntitiesByGuid(entityGuid).Any();

    }
}
