using System.Collections.Generic;
using System.Net.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Admin
{
    public interface ITypeController
    {
        /// <summary>
        /// Create a ghost-content-type (very advanced feature)
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sourceStaticName"></param>
        /// <returns></returns>
        bool AddGhost(int appId, string sourceStaticName);

        /// <summary>
        /// Delete a content-type from an app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="staticName"></param>
        /// <returns></returns>
        bool Delete(int appId, string staticName);

        /// <summary>
        /// Get all content-types in the app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="scope"></param>
        /// <param name="withStatistics"></param>
        /// <returns></returns>
        IEnumerable<ContentTypeDto> List(int appId, string scope = null, bool withStatistics = false);

        /// <summary>
        /// Get one content-type
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        ContentTypeDto Get(int appId, string contentTypeId, string scope = null);
        
        // 2019-11-15 2dm special change: item to be Dictionary<string, object> because in DNN 9.4
        // it causes problems when a content-type has metadata, where a value then is a deeper object
        // in future, the JS front-end should send something clearer and not the whole object
        bool Save(int appId, Dictionary<string, object> item);

        /// <summary>
        /// Mark a field as being the title field
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="attributeId"></param>
        void SetTitle(int appId, int contentTypeId, int attributeId);

        HttpResponseMessage Json(int appId, string name);

        /// <summary>
        /// Used to be POST ImportExport/ImportContent
        /// </summary>
        /// <remarks>
        /// New in 2sxc 11.07
        /// </remarks>
        /// <returns></returns>
        ImportResultDto Import(int zoneId, int appId);
    }
}