﻿using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IFieldController
    {
        /// <summary>
        /// Add a field to the content type
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="staticName"></param>
        /// <param name="type">the primary type, like "string" or "entity"</param>
        /// <param name="inputType">the input-type to be used - like "string-default"</param>
        /// <param name="index">position in the field-list</param>
        /// <returns></returns>
        int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index);

        /// <summary>
        /// Request available data-types in this app
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        string[] DataTypes(int appId);


        /// <summary>
        /// Delete a field / attribute
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        bool Delete(int appId, int contentTypeId, int attributeId);


        /// <summary>
        /// Get the fields of a content-type
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="staticName"></param>
        /// <returns></returns>
        IEnumerable<ContentTypeFieldDto> All(int appId, string staticName);


        /// <summary>
        /// Get a list of all known input types in this app.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// <remarks>
        /// It's important to note that each app could have its own additional input types.
        /// </remarks>
        List<InputTypeInfo> InputTypes(int appId);

        /// <summary>
        /// Rename an field
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="attributeId"></param>
        /// <param name="newName"></param>
        void Rename(int appId, int contentTypeId, int attributeId, string newName);

        /// <summary>
        /// Change the order of the fields
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        bool Sort(int appId, int contentTypeId, string order);

        /// <summary>
        /// Update the input-type information on a field
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="attributeId"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        bool InputType(int appId, int attributeId, string inputType);
    }
}