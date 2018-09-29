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
        public MetadataController(Log parentLog = null) : base(parentLog, "Api.MetaCn")
        {}

        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyType, string key, string contentType, int/*?*/ appId /*= null*/)
        {
            //if (appId.HasValue)
            //    AppId = appId.Value;

            IEnumerable<IEntity> entityList = null;

            var appRun = new AppRuntime(appId, Log);

            switch (keyType)
            {
                case "guid":
                    if(Guid.TryParse(key, out var guidKey))
                        entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, guidKey, contentType);
                    break;
                case "string":
                    entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, key, contentType);
                    break;
                case "number":
                    if(int.TryParse(key, out var keyInt))
                        entityList = appRun.Package.GetMetadata(assignmentObjectTypeId, keyInt, contentType);
                    break;
                default:
                    throw new Exception("keytype unknown:" + keyType);
            }

            if(entityList == null)
                throw new Exception($"was not able to convert '{key}' to keytype {keyType}, must cancel");

            return GetSerializerWithGuidEnabled().Prepare(entityList);
        }

    }
}