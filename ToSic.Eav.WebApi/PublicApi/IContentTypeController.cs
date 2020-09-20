using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface ITypeController
    {
        ///// <summary>
        ///// Add a field to the content type
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="contentTypeId"></param>
        ///// <param name="staticName"></param>
        ///// <param name="type">the primary type, like "string" or "entity"</param>
        ///// <param name="inputType">the input-type to be used - like "string-default"</param>
        ///// <param name="sortOrder">position in the field-list</param>
        ///// <returns></returns>
        //int AddField(int appId, int contentTypeId, string staticName, string type, string inputType, int sortOrder);

        /// <summary>
        /// Create a ghost-content-type (very advanced feature)
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sourceStaticName"></param>
        /// <returns></returns>
        bool AddGhost(int appId, string sourceStaticName);

        ///// <summary>
        ///// Request available data-types in this app
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <returns></returns>
        //string[] DataTypes(int appId);

        /// <summary>
        /// Delete a content-type from an app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="staticName"></param>
        /// <returns></returns>
        bool Delete(int appId, string staticName);

        ///// <summary>
        ///// Delete a field / attribute
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="contentTypeId"></param>
        ///// <param name="attributeId"></param>
        ///// <returns></returns>
        //bool DeleteField(int appId, int contentTypeId, int attributeId);

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

        ///// <summary>
        ///// Get the fields of a content-type
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="staticName"></param>
        ///// <returns></returns>
        //IEnumerable<ContentTypeFieldDto> GetFields(int appId, string staticName);

        ///// <summary>
        ///// Get a single content-type.
        ///// Warn / todo: this seems to be a duplicate name for the Get, just with name instead of ID
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="contentTypeStaticName"></param>
        ///// <param name="scope"></param>
        ///// <returns></returns>
        //ContentTypeDto GetSingle(int appId, string contentTypeStaticName, string scope = null);

        ///// <summary>
        ///// Get a list of all known input types in this app.
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// It's important to note that each app could have its own additional input types.
        ///// </remarks>
        //List<InputTypeInfo> InputTypes(int appId);

        ///// <summary>
        ///// Rename an field
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="contentTypeId"></param>
        ///// <param name="attributeId"></param>
        ///// <param name="newName"></param>
        //void Rename(int appId, int contentTypeId, int attributeId, string newName);

        ///// <summary>
        ///// Change the order of the fields
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="contentTypeId"></param>
        ///// <param name="newSortOrder"></param>
        ///// <returns></returns>
        //bool Reorder(int appId, int contentTypeId, string newSortOrder);

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

        ///// <summary>
        ///// Update the input-type information on a field
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="attributeId"></param>
        ///// <param name="inputType"></param>
        ///// <returns></returns>
        //bool UpdateInputType(int appId, int attributeId, string inputType);
    }
}