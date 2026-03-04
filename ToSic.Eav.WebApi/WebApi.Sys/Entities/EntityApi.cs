using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys.Ancestors;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Sys.OData;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Sys.Entities;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityApi(
    AppWorkContextService appWorkCtxSvc,
    GenWorkPlus<WorkEntities> workEntities,
    GenWorkDb<WorkEntityDelete> entDelete,
    Generator<IConvertToEavLight> entitiesToDicLazy,
    Generator<MultiPermissionsTypes, MultiPermissionsTypes.Options> multiPermissionsTypes)
    : ServiceBase("Api.Entity",
        connect: [appWorkCtxSvc, workEntities, entDelete, entitiesToDicLazy.SetInit(etd => etd.WithGuid = true), multiPermissionsTypes])
{

    #region DI Constructor & Init

    public EntityApi Init(int appId, bool? showDrafts = default)
    {
        _appWorkCtxPlus = appWorkCtxSvc.ContextPlus(appId, showDrafts);
        return this;
    }

    private IAppWorkCtxPlus _appWorkCtxPlus = null!;

    #endregion

    /// <summary>
    /// Get all Entities of specified Type
    /// </summary>
    public IEnumerable<IDictionary<string, object>> GetEntities(IAppReader appReader, string contentType, bool showDrafts, Uri? fullRequest)
    {
        var list = workEntities.New(appReader, showDrafts).Get(contentType, fullRequest: fullRequest);
        var converter= entitiesToDicLazy.New();

        if (fullRequest is not null)
        {
            (converter as ConvertToEavLight).DoIfNotNull(c =>
                c.AddSelectFields([.. SystemQueryOptionsParser.Parse(fullRequest).Select]));
        }
        return converter.Convert(list)!;
    }
    
    /// <summary>
    /// Delete the entity specified by ID.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="id">Entity ID</param>
    /// <param name="force">try to force-delete</param>
    /// <param name="parentId">parent entity containing this item in a field/list</param>
    /// <param name="parentField">parent field containing this item</param>
    /// <exception cref="ArgumentNullException">Entity does not exist</exception>
    /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
    public void Delete(string contentType, int id, bool force = false, int? parentId = null, string? parentField = null)
        => entDelete.New(_appWorkCtxPlus.AppReader).Delete(id, contentType, force, false, parentId, parentField);

    /// <summary>
    /// Delete the entity specified by GUID.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="entityGuid">Entity GUID</param>
    /// <param name="parentId">parent entity containing this item in a field/list</param>
    /// <param name="parentField">parent field containing this item</param>
    /// <param name="force"></param>
    /// <exception cref="ArgumentNullException">Entity does not exist</exception>
    /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
    public void Delete(string contentType, Guid entityGuid, bool force = false, int? parentId = null, string? parentField = null) 
        => Delete(contentType, workEntities.New(_appWorkCtxPlus.AppReader).Get(entityGuid)!.EntityId, force, parentId, parentField);


    // 2020-12-08 2dm - unused code, disable for now, delete ca. Feb 2021
    public EntityApi InitOrThrowBasedOnGrants(IContextOfSite context, IAppIdentity app, string contentType, List<Grants> requiredGrants)
    {
        var permCheck = multiPermissionsTypes.New(new() { SiteContext = context, App = app, ContentTypes = [contentType] });
        if (!permCheck.EnsureAll(requiredGrants, out var error))
            throw HttpException.PermissionDenied(error);
        return Init(app.AppId);
    }

    public List<Dictionary<string, object>> GetEntitiesForAdmin(string contentType, bool excludeAncestor = false)
    {
        var l = Log.Fn<List<Dictionary<string, object>>>(timer: true);

        var ofType = workEntities.New(_appWorkCtxPlus)
            .Get(contentType)
            .ToList();

        // in the successor app, we can get an additional AppConfiguration, AppSettings or AppResources from the ancestor app
        // that we can optionally exclude from the results
        var afterAncestorFilter = excludeAncestor
            ? ofType
                .Where(e => !e.HasAncestor())
                .ToList()
            : ofType;

        // Convert all to dictionary
        var entityToDic = entitiesToDicLazy.New();
        ((ConvertToEavLight)entityToDic).ConfigureForAdminUse();
        var list = entityToDic.Convert(afterAncestorFilter)
            .ToList();

        // Truncate all values to 50 chars
        var result = Log.Quick(message: "truncate dictionary", timer: true,
            func: () => list
                .Select(eLight => eLight.ToDictionary(pair => pair.Key, pair => Truncate(pair.Value, 50)))
                .ToList()
        );
        return l.Return(result!, result.Count.ToString());
    }


    private object? Truncate(object? value, int length)
        => value is not string asTxt
            ? value
            : asTxt.Length > length
                ? asTxt.Substring(0, length)
                : asTxt;
}