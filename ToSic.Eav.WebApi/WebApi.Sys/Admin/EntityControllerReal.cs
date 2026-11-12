using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.ImportExport;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityControllerReal(
    LazySvc<IContextOfSiteService> ctxSvc,
    LazySvc<IAppsCatalog> appsCatalog,
    LazySvc<ContentExportApi> contentExport,
    LazySvc<ContentImportApi> contentImport,
    AppWorkQuick<WorkTypePermissions> typePermissions,
    AppWorkQuick<WorkEntityDelete> entDelete,
    LazySvc<IUser> user,
    IResponseMaker responseMaker)
    : ServiceBase("Api.EntityRl",
        connect: [ctxSvc, appsCatalog, contentExport, contentImport, entDelete, user, responseMaker]),
        IEntityController
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
    public THttpResponseType Json(int appId, int id, string prefix, bool withMetadata)
        => contentExport.Value.Init(appId).DownloadEntityAsJson(user.Value, id, prefix, withMetadata);


    /// <inheritdoc/>
    public THttpResponseType Download(
        int appId,
        string language,
        string defaultLanguage,
        string contentType,
        ExportSelection recordExport,
        ExportResourceReferenceMode resourcesReferences,
        ExportLanguageResolution languageReferences, 
        string? selectedIds = null)
    {
        var (content, fileName) = contentExport.Value.Init(appId)
            .ExportContent(
                user.Value,
                language, defaultLanguage, contentType,
                recordExport, resourcesReferences,
                languageReferences,
                selectedIds
            );

        return responseMaker.File(fileContent: content, fileName: fileName);
    }


    /// <inheritdoc/>
    public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
        => contentImport.Value.Init(args.AppId).XmlPreview(args);


    /// <inheritdoc/>
    public ContentImportResultDto XmlUpload(ContentImportArgsDto args)
        => contentImport.Value.Init(args.AppId).XmlImport(args);


    /// <inheritdoc/>
    public bool Upload(EntityImportDto args)
        => contentImport.Value.Init(args.AppId).Import(args);

}
