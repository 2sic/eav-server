using System;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {


        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
        /// </summary>
        internal IQueryable<ToSicEavEntities> GetAssignedEntities(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null, string includes = null)
        {
            var origQuery = DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.AssignmentObjectTypeId == assignmentObjectTypeId
                   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
                   && e.ChangeLogDeleted == null);
            if (!string.IsNullOrEmpty(includes))
                origQuery = IncludeMultiple(origQuery, includes);
            return origQuery;
        }

        /// <summary>
        /// Get a Metadata items which enhance existing Entities, 
        /// and use the GUID to keep reference. This is extra complex, because the Guid can be in use multiple times on various apps
        /// </summary>
        internal IQueryable<ToSicEavEntities> GetEntityMetadataByGuid(int appId, Guid keyGuid, string includes = null)
        {
            var query = GetAssignedEntities(Constants.MetadataForEntity, keyGuid: keyGuid, includes: includes)
                .Where(e => e.AttributeSet.AppId == appId);
            return query;
        }

    }
}
