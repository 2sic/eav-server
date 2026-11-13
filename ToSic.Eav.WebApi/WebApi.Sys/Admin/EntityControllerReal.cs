using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.JsonData;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.XmlData;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.ImportExport;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityControllerReal(
    LazySvc<IContextOfSiteService> ctxSvc,
    LazySvc<IAppsCatalog> appsCatalog,
    AppWorkQuick<ImportDataJson> contentImport,
    AppWorkQuick<WorkTypePermissions> typePermissions,
    AppWorkQuick<WorkEntityDelete> entDelete,
    AppWorkQuick<ImportDataXml> contentImportXml,
    AppWorkQuick<ImportDataXmlPreview> contentImportXmlPreview,
    AppWorkQuick<ExportDataXml> exportDataXml,
    AppWorkQuick<ExportEntityJson> exportEntityJson,
    LazySvc<IUser> user,
    IResponseMaker responseMaker)
    : ServiceBase("Api.EntityRl",
        connect: [ctxSvc, appsCatalog, contentImport, entDelete, contentImportXml, contentImportXmlPreview, exportDataXml, exportEntityJson, user, responseMaker])
        // IEntityController
{
    public const string LogSuffix = "Entity";


    /// <inheritdoc/>
    public void Delete(string contentType, int appId, int? id, Guid? guid, bool force = false, int? parentId = null, string? parentField = null)
    {
        var showDrafts = typePermissions.New(appId).GetDraftPermissionAndThrowIfNotAllowed(contentType, GrantSets.DeleteSomething);
        var deleteSvc = entDelete.New(appId, showDrafts);
        if (id.HasValue)
            deleteSvc.Delete(id.Value, contentType, force: force, parentId: parentId, parentField: parentField);
        else if (guid.HasValue)
            deleteSvc.Delete([guid.Value], contentType: contentType, force: force, parentId: parentId, parentField: parentField);
        else
            throw new($"When using '{nameof(Delete)}' you must use 'id' or 'guid' parameters.");
    }


    /// <inheritdoc/>
    public FileResult Json(int appId, int id, string prefix, bool withMetadata)
        => exportEntityJson.New(appId).DownloadEntityAsJson(user.Value, id, prefix, withMetadata);


    /// <inheritdoc/>
    public FileResult Download(
        int appId,
        string language,
        string defaultLanguage,
        string contentType,
        ExportSelection recordExport,
        ExportResourceReferenceMode resourcesReferences,
        ExportLanguageResolution languageReferences, 
        string? selectedIds = null) =>
        exportDataXml.New(appId)
            .ExportContent(
                user.Value,
                language, defaultLanguage, contentType,
                recordExport, resourcesReferences,
                languageReferences,
                selectedIds
            );


    /// <inheritdoc/>
    public ContentImportResult XmlPreview(ContentFileImportOptions args)
        => contentImportXmlPreview.New(args.AppId).XmlPreview(args);


    /// <inheritdoc/>
    public ContentImportResult XmlUpload(ContentFileImportOptions args)
        => contentImportXml.New(args.AppId).XmlImport(args);


    /// <inheritdoc/>
    public bool Upload(DataFileImportOptions args)
        => contentImport.New(args.AppId).Import(args);

}
