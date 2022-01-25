﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntitiesController
    {
        /// <summary>
        /// Get all entities of a specific type in the app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <remarks>
        /// Needs edit-permissions, as the item-list can also be accessed from the toolbar in certain cases.
        /// Will do permission checks internally.
        /// </remarks>
        IEnumerable<Dictionary<string, object>> List(int appId, string contentType);

        /// <summary>
        /// Delete an item from the admin-UI
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="id"></param>
        /// <param name="appId"></param>
        /// <param name="force"></param>
        /// <remarks>
        /// Needs edit-permissions, as the item-list can also be accessed from the toolbar in certain cases.
        /// Will do permission checks internally.
        /// </remarks>
        void Delete(string contentType, int id, int appId, bool force = false, int? parentId = null, string parentField = null);

        /// <summary>
        /// Delete an item from the admin-UI
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="guid"></param>
        /// <param name="appId"></param>
        /// <param name="force"></param>
        /// <remarks>
        /// Needs edit-permissions, as the item-list can also be accessed from the toolbar in certain cases.
        /// Will do permission checks internally.
        /// </remarks>
        void Delete(string contentType, Guid guid, int appId, bool force = false, int? parentId = null, string parentField = null);

        /// <summary>
        /// Used to be GET ContentExport/DownloadEntityAsJson
        /// </summary>
        /// <remarks>
        /// Will do permission checks internally.
        /// </remarks>

        HttpResponseMessage Json(int appId, int id, string prefix, bool withMetadata);

        /// <summary>
        /// This seems to be for XML import of a list
        /// Used to be POST ContentImport/EvaluateContent
        /// </summary>
        ContentImportResultDto XmlPreview(ContentImportArgsDto args);
        
        /// <summary>
        /// This seems to be for XML import of a list
        /// Used to be POST ContentImport/ImportContent
        /// </summary>
        ContentImportResultDto XmlUpload(ContentImportArgsDto args);
        
        /// <summary>
        /// This is the single-item json import
        /// Used to be POST ContentImport/Import
        /// </summary>
        bool Upload(EntityImportDto args);

        /// <summary>
        /// New feature in 11.03 - Usage Statistics
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <remarks>
        /// New in 2sxc 11.03
        /// </remarks>
        dynamic Usage(int appId, Guid guid);
    }
}