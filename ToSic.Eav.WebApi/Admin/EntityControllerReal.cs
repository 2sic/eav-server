using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Internal.Options;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Infrastructure;
using ServiceBase = ToSic.Lib.Services.ServiceBase;
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.Admin;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityControllerReal(
    LazySvc<IContextOfSite> context,
    LazySvc<IAppsCatalog> appsCatalog,
    LazySvc<EntityApi> entityApi,
    LazySvc<ContentExportApi> contentExport,
    LazySvc<ContentImportApi> contentImport,
    LazySvc<IUser> user,
    IResponseMaker responseMaker)
    : ServiceBase("Api.EntityRl",
        connect: [context, appsCatalog, entityApi, contentExport, contentImport, user, responseMaker]), IEntityController
{
    public const string LogSuffix = "Entity";


    /// <inheritdoc/>
    public IEnumerable<Dictionary<string, object>> List(int appId, string contentType)
        => entityApi.Value.InitOrThrowBasedOnGrants(context.Value, appsCatalog.Value.AppIdentity(appId), contentType, GrantSets.ReadSomething)
            .GetEntitiesForAdmin(contentType);


    /// <inheritdoc/>
    public void Delete(string contentType, int appId, int? id, Guid? guid, bool force = false, int? parentId = null, string parentField = null)
    {
        var catalog = appsCatalog.Value;
        if (id.HasValue) entityApi.Value.InitOrThrowBasedOnGrants(context.Value, catalog.AppIdentity(appId), contentType, GrantSets.DeleteSomething)
            .Delete(contentType, id.Value, force, parentId, parentField);
        else if (guid.HasValue) entityApi.Value.InitOrThrowBasedOnGrants(context.Value, catalog.AppIdentity(appId), contentType, GrantSets.DeleteSomething)
            .Delete(contentType, guid.Value, force, parentId, parentField);
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
        string selectedIds = null)
    {
        var (content, fileName) = contentExport.Value.Init(appId).ExportContent(
            user.Value,
            language, defaultLanguage, contentType,
            recordExport, resourcesReferences,
            languageReferences, selectedIds);

        return responseMaker.File(fileContent: content, fileName: fileName);
    }


    /// <inheritdoc/>
    public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
        => contentImport.Value.Init(args.AppId).XmlPreview(args);


    /// <inheritdoc/>
    public ContentImportResultDto XmlUpload(ContentImportArgsDto args)
        => contentImport.Value.Init(args.AppId).XmlImport(args);


    /// <inheritdoc/>
    public bool Upload(EntityImportDto args) => contentImport.Value.Init(args.AppId).Import(args);


    /// <inheritdoc/>
    //public dynamic Usage(int appId, Guid guid) => _entityBackend.Value.Init(Log).Usage(appId, guid);
}
