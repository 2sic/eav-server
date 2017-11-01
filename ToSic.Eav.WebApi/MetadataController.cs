using System;
using System.Collections.Generic;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public class MetadataController : Eav3WebApiBase
    {
        public MetadataController(Log parentLog = null) : base(parentLog)
        {
            Log.Rename("MetaDC");
        }
        #region AssignedEntities - these are entities which have been assigned to something else
        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        //public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, Guid keyGuid, string contentType, int? appId = null)
        //      {
        //          if (appId.HasValue)
        //              AppId = appId.Value;
        //          var entityList = MetaDS.GetAssignedEntities(assignmentObjectTypeId, keyGuid, contentType);
        //          return Serializer.Prepare(entityList);
        //      }

        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyType, string key, string contentType, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            IEnumerable<ToSic.Eav.Interfaces.IEntity> entityList;
            if (keyType == "guid")
            {
                var guidkey = Guid.Parse(key);
                entityList = MetaDs.GetMetadata(assignmentObjectTypeId, guidkey, contentType);                
            }
            else if (keyType == "string")
            {
                entityList = MetaDs.GetMetadata(assignmentObjectTypeId, key, contentType);                
            }
            else
            {
                throw new Exception("keytype unknown:" + keyType);
            }

            return Serializer.Prepare(entityList);
        }
        #endregion

    }
}