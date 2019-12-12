using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public class MetadataController : HasLog
    {
        public MetadataController(ILog parentLog) : base("Api.MetaCn", parentLog)
        {}

        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Get(int appId, int targetType, string keyType, string key, string contentType = null)
        {
            IEnumerable<IEntity> entityList = null;

            var appRun = new AppRuntime(appId, Log);

            switch (keyType)
            {
                case "guid":
                    if(Guid.TryParse(key, out var guidKey))
                        entityList = appRun.AppState.Get(targetType, guidKey, contentType);
                    break;
                case "string":
                    entityList = appRun.AppState.Get(targetType, key, contentType);
                    break;
                case "number":
                    if(int.TryParse(key, out var keyInt))
                        entityList = appRun.AppState.Get(targetType, keyInt, contentType);
                    break;
                default:
                    throw new Exception("keytype unknown:" + keyType);
            }

            if(entityList == null)
                throw new Exception($"was not able to convert '{key}' to keytype {keyType}, must cancel");

            return Helpers.Serializers.GetSerializerWithGuidEnabled().Prepare(entityList);
        }

    }
}