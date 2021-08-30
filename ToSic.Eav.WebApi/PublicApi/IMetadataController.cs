using System.Collections.Generic;

namespace ToSic.Eav.WebApi.PublicApi
{
    /// <summary>
    /// Metadata Controller to get Entities assigned to something
    /// </summary>
    public interface IMetadataController
    {
        ///// <summary>
        ///// Retrieve entities which are assigned to something
        ///// </summary>
        ///// <param name="assignmentObjectTypeId">A key which specifies what target type it is. </param>
        ///// <param name="keyType">A string specifying what the key is - can be "string", "guid" "number"</param>
        ///// <param name="key">The key (string, guid, number) as a string</param>
        ///// <param name="contentType">A optional parameter to filter only results by a specific type</param>
        ///// <param name="appId">The AppId we're addressing</param>
        ///// <returns></returns>
        //IEnumerable<Dictionary<string, object>> GetAssignedEntities(int assignmentObjectTypeId, string keyType, string key, string contentType, int appId);

        /// <summary>
        /// Retrieve entities which are assigned to something
        /// </summary>
        /// <param name="appId">The AppId we're addressing</param>
        /// <param name="targetType">A key which specifies what target type it is. </param>
        /// <param name="keyType">A string specifying what the key is - can be "string", "guid" "number"</param>
        /// <param name="key">The key (string, guid, number) as a string</param>
        /// <param name="contentType">A optional parameter to filter only results by a specific type</param>
        /// <returns></returns>
        IEnumerable<IDictionary<string, object>> Get(int appId, int targetType, string keyType, string key, string contentType = null);

    }
}