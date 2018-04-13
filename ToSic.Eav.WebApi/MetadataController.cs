using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Interfaces;
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


        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyType, string key, string contentType, int? appId = null)
        {
            if (appId.HasValue)
                AppId = appId.Value;

            IEnumerable<IEntity> entityList;

            // todo: if possible, move metadata lookup to appRuntime
            //var metaDs = DataSource.GetMetaDataSource(appId: AppId);
            var appRun = new AppRuntime(AppId, Log);

            bool ok;
            switch (keyType)
            {
                case "guid":
                    ok = Guid.TryParse(key, out var guidKey);
                    entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, guidKey, contentType);
                    break;
                case "string":
                    ok = true;
                    entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, key, contentType);
                    break;
                case "number":
                    ok = int.TryParse(key, out var keyInt);
                    entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, keyInt, contentType);
                    break;
                default:
                    throw new Exception("keytype unknown:" + keyType);
            }

            if(!ok)
                throw new Exception($"was not able to convert '{key}' to keytype {keyType}, must cancel");

            return Serializer.Prepare(entityList);
        }

    }
}