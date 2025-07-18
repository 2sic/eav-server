﻿using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.WebApi.Sys.Dto;
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IEntityController
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
    /// <param name="appId"></param>
    /// <param name="id"></param>
    /// <param name="guid"></param>
    /// <param name="force"></param>
    /// <param name="parentId"></param>
    /// <param name="parentField"></param>
    /// <remarks>
    /// Id or guid is required.
    /// Needs edit-permissions, as the item-list can also be accessed from the toolbar in certain cases.
    /// Will do permission checks internally.
    /// </remarks>
    void Delete(string contentType, int appId, int? id = null, Guid? guid = null, bool force = false, int? parentId = null,
        string? parentField = null);

    /// <summary>
    /// Used to be GET ContentExport/DownloadEntityAsJson
    /// </summary>
    /// <remarks>
    /// Will do permission checks internally.
    /// </remarks>
    THttpResponseType Json(int appId, int id, string prefix, bool withMetadata);

    /// <summary>
    /// Used to be GET ContentExport/ExportContent
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="language"></param>
    /// <param name="defaultLanguage"></param>
    /// <param name="contentType"></param>
    /// <param name="recordExport"></param>
    /// <param name="resourcesReferences"></param>
    /// <param name="languageReferences"></param>
    /// <param name="selectedIds"></param>
    /// <returns></returns>
    THttpResponseType Download(
        int appId, 
        string language, 
        string defaultLanguage, 
        string contentType,
        ExportSelection recordExport, 
        ExportResourceReferenceMode resourcesReferences,
        ExportLanguageResolution languageReferences, 
        string? selectedIds = null);

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

    ///// <summary>
    ///// New feature in 11.03 - Usage Statistics
    ///// </summary>
    ///// <param name="appId"></param>
    ///// <param name="guid"></param>
    ///// <returns></returns>
    ///// <remarks>
    ///// New in 2sxc 11.03
    ///// </remarks>
    //dynamic Usage(int appId, Guid guid);
}