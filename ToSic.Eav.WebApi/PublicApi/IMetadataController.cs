using System.Collections.Generic;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IMetadataController
    {
        IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyType, string key, string contentType, int appId);
    }
}