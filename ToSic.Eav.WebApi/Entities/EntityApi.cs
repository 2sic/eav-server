using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.Security.Internal;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Errors;
using ToSic.Eav.WebApi.Formats;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntityApi: ServiceBase
{

    #region DI Constructor & Init

    public EntityApi(
        AppWorkContextService appWorkCtxSvc,
        GenWorkPlus<WorkEntities> workEntities,
        GenWorkDb<WorkEntityDelete> entDelete,
        LazySvc<IConvertToEavLight> entitiesToDicLazy, 
        EntityBuilder entityBuilder, 
        Generator<MultiPermissionsTypes> multiPermissionsTypes) : base("Api.Entity")
    {
        ConnectServices(
            _appWorkCtxSvc = appWorkCtxSvc,
            _workEntities = workEntities,
            _entDelete = entDelete,
            _entitiesToDicLazy = entitiesToDicLazy,
            _entityBuilder = entityBuilder,
            _multiPermissionsTypes = multiPermissionsTypes
        );
    }

    private readonly AppWorkContextService _appWorkCtxSvc;
    private readonly GenWorkPlus<WorkEntities> _workEntities;
    private readonly GenWorkDb<WorkEntityDelete> _entDelete;

    private readonly LazySvc<IConvertToEavLight> _entitiesToDicLazy;
    private readonly EntityBuilder _entityBuilder;
    private readonly Generator<MultiPermissionsTypes> _multiPermissionsTypes;


    public EntityApi Init(int appId, bool? showDrafts = default)
    {
        _appWorkCtxPlus = _appWorkCtxSvc.ContextPlus(appId, showDrafts);
        return this;
    }

    private IAppWorkCtxPlus _appWorkCtxPlus;

    #endregion

    #region Lazy Helpers

    /// <summary>
    /// The serializer, so it can be configured from outside if necessary
    /// </summary>
    private IConvertToEavLight EntityToDic
    {
        get
        {
            if (_entToDic != null) return _entToDic;
            _entToDic = _entitiesToDicLazy.Value;
            _entToDic.WithGuid = true;
            return _entToDic;
        }
    }
    private IConvertToEavLight _entToDic;

    #endregion

    /// <summary>
    /// Get all Entities of specified Type
    /// </summary>
    public IEnumerable<IDictionary<string, object>> GetEntities(string contentType)
        => EntityToDic.Convert(_workEntities.New(_appWorkCtxPlus).Get(contentType));

    /// <summary>
    /// Get all Entities of specified Type
    /// </summary>
    public IEnumerable<IDictionary<string, object>> GetEntities(IAppStateInternal appState, string contentType, bool showDrafts) 
        => EntityToDic.Convert(_workEntities.New(appState, showDrafts).Get(contentType));

    public List<BundleWithHeader<IEntity>> GetEntitiesForEditing(List<ItemIdentifier> items)
    {
        ReplaceSimpleTypeNames(items);

        var list = items.Select(p =>
        {
            var ent = p.EntityId != 0 || p.DuplicateEntity.HasValue
                ? GetEditableEditionAndMaybeCloneIt(p)
                : null;
            return new BundleWithHeader<IEntity>
            {
                Header = p,
                Entity = ent
            };
        }).ToList();

        // make sure the header has the right "new" guid as well - as this is the primary one to work with
        // it is really important to use the header guid, because sometimes the entity does not exist - so it doesn't have a guid either
        var itemsWithEmptyHeaderGuid = list
            .Where(i => i.Header.Guid == default)
            .ToArray(); // must do toArray, to prevent re-checking after setting the guid

        foreach (var bundle in itemsWithEmptyHeaderGuid)
        {
            var hasEntity = bundle.Entity != null;
            var useEntityGuid = hasEntity && bundle.Entity.EntityGuid != default;
            bundle.Header.Guid = useEntityGuid
                ? bundle.Entity.EntityGuid
                : Guid.NewGuid();
            if (hasEntity && !useEntityGuid)
                bundle.Entity = _entityBuilder.CreateFrom(bundle.Entity, guid: bundle.Header.Guid);
            //bundle.Entity = _entityBuilder.ResetIdentifiers(bundle.Entity, newGuid: bundle.Header.Guid);
            //(bundle.Entity as Entity).SetGuid(bundle.Header.Guid);
        }

        // Update header with ContentTypeName in case it wasn't there before
        foreach (var itm in list.Where(i => i.Header.ContentTypeName == null && i.Entity != null))
            itm.Header.ContentTypeName = itm.Entity.Type.NameId;

        // Add EditInfo for read-only data
        foreach (var bundle in list) 
            bundle.Header.EditInfo = new EditInfoDto(bundle.Entity);

        return list;
    }


    private IEntity GetEditableEditionAndMaybeCloneIt(ItemIdentifier p)
    {
        var appState = _appWorkCtxPlus.AppState;
        var found = appState.List.GetOrThrow(p.ContentTypeName, p.DuplicateEntity ?? p.EntityId);
        // if there is a draft, use that for editing - not the original
        found = appState.GetDraft(found) ?? found;

        // If we want the original (not a copy for new) then stop here
        if (!p.DuplicateEntity.HasValue) return found;

        // TODO: 2023-02-25 seems that EntityId is reset, but RepositoryId isn't - not sure why or if this is correct
        var copy = _entityBuilder.CreateFrom(found, id: 0, guid: Guid.Empty);
        return copy;
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
    public void Delete(string contentType, int id, bool force = false, int? parentId = null, string parentField = null)
        => _entDelete.New(_appWorkCtxPlus.AppState).Delete(id, contentType, force, false, parentId, parentField);

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
    public void Delete(string contentType, Guid entityGuid, bool force = false, int? parentId = null, string parentField = null) 
        => Delete(contentType, _workEntities.New(_appWorkCtxPlus.AppState).Get(entityGuid).EntityId, force, parentId, parentField);


    /// <summary>
    /// clean up content-type names in case it's using the nice-name instead of the static name...
    /// </summary>
    /// <param name="items"></param>
    private void ReplaceSimpleTypeNames(List<ItemIdentifier> items)
    {
        foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
        {
            var ct = _appWorkCtxPlus.AppState.GetContentType(itm.ContentTypeName);
            if (ct == null)
            {
                if (!itm.ContentTypeName.StartsWith("@"))
                    throw new Exception("Can't find content type " + itm.ContentTypeName);
                items.Remove(itm);
                continue;
            }
            if (ct.NameId != itm.ContentTypeName) // not using the static name...fix
                itm.ContentTypeName = ct.NameId;
        }
    }

    // 2020-12-08 2dm - unused code, disable for now, delete ca. Feb 2021
    public EntityApi InitOrThrowBasedOnGrants(IContextOfSite context, IAppIdentity app, string contentType, List<Eav.Security.Grants> requiredGrants)
    {
        var permCheck = _multiPermissionsTypes.New().Init(context, app, contentType);
        if (!permCheck.EnsureAll(requiredGrants, out var error))
            throw HttpException.PermissionDenied(error);
        return Init(app.AppId);
    }

    public List<Dictionary<string, object>> GetEntitiesForAdmin(string contentType, bool excludeAncestor = false)
    {
        var wrapLog = Log.Fn<List<Dictionary<string, object>>>(timer: true);
        EntityToDic.ConfigureForAdminUse();
        var originals = _workEntities.New(_appWorkCtxPlus).Get(contentType).ToList();

        // in the successor app, we can get an additional AppConfiguration, AppSettings or AppResources from the ancestor app
        // that we can optionally exclude from the results
        if (excludeAncestor) originals = 
            originals.Where(e => !e.HasAncestor()).ToList();

        var list = EntityToDic.Convert(originals).ToList();

        var result = Log.Func(null, message: "truncate dictionary", timer: true, func: () => list
            .Select(eLight => eLight.ToDictionary(pair => pair.Key, pair => Truncate(pair.Value, 50)))
            .ToList());
        return wrapLog.Return(result, result.Count.ToString());
    }


    private object Truncate(object value, int length)
    {
        if (!(value is string asTxt))
            return value;

        if (asTxt.Length > length)
            asTxt = asTxt.Substring(0, length);
        return asTxt;
    }
}