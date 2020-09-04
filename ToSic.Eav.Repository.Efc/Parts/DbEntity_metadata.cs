using System;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
        /// Only used for delete / clean-up
        /// </summary>
        private IQueryable<ToSicEavEntities> GetAssignedEntities(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null, string includes = null)
        {
            var origQuery = DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.AssignmentObjectTypeId == assignmentObjectTypeId
                   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
                   && e.ChangeLogDeleted == null);
            if (!string.IsNullOrEmpty(includes))
                origQuery = IncludeMultiple(origQuery, includes);
            return origQuery;
        }

    }
}
