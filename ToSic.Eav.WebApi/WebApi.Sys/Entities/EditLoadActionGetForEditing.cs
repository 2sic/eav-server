using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Entities;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EditLoadActionGetForEditing(AppWorkContextService appWorkCtxSvc, EntityAssembler entityAssembler)
    : ServiceBase("Api.Entity", connect: [appWorkCtxSvc, entityAssembler])
{
    // Note: reworked this 2026-05-15 2dm to make the objects immutable, hope no side effects #ImmutableIsTheNewBlack
    public List<BundleWithHeaderOptional<IEntity>> Run(LowCodeActionContext actionCtx, List<ItemIdentifier> items)
    {
        var appWorkCtxPlus = actionCtx.Get<IAppWorkCtxPlus>(EditLoadContextConstants.AppCtxWork);

        items = ReplaceSimpleTypeNames(appWorkCtxPlus, items);

        var list = items
            .Select(p =>
            {
                var ent = p.EntityId != 0 || p.DuplicateEntity.HasValue
                    ? GetEditableEditionAndMaybeCloneIt(appWorkCtxPlus, p)
                    : null;
                return new BundleWithHeaderOptional<IEntity>
                {
                    Header = p,
                    Entity = ent
                };
            })
            .ToList();

        // make sure the header has the right "new" guid as well - as this is the primary one to work with
        // it is really important to use the header guid, because sometimes the entity does not exist - so it doesn't have a guid either
        list = list
            .Select(bundle =>
            {
                // Skip these, they are ok
                if (bundle.Header!.Guid != default)
                    return bundle;
                
                var hasEntity = bundle.Entity != null;
                var useEntityGuid = hasEntity && bundle.Entity!.EntityGuid != default;
                var newGuid = useEntityGuid
                    ? bundle.Entity!.EntityGuid
                    : Guid.NewGuid();
                return new()
                {
                    Header = bundle.Header! with { Guid = newGuid },
                    Entity = hasEntity && !useEntityGuid
                        ? entityAssembler.CreateFrom(bundle.Entity!, guid: newGuid)
                        : bundle.Entity
                };
            })
            .ToList();
        
        //var itemsWithEmptyHeaderGuid = list
        //    .Where(i => i.Header!.Guid == default)
        //    .ToArray(); // must do toArray, to prevent re-checking after setting the guid

        //foreach (var bundle in itemsWithEmptyHeaderGuid)
        //{
        //    var hasEntity = bundle.Entity != null;
        //    var useEntityGuid = hasEntity && bundle.Entity!.EntityGuid != default;
        //    bundle.Header = bundle.Header! with
        //    {
        //        Guid = useEntityGuid
        //            ? bundle.Entity!.EntityGuid
        //            : Guid.NewGuid()
        //    };
        //    if (hasEntity && !useEntityGuid)
        //        bundle.Entity = entityAssembler.CreateFrom(bundle.Entity!, guid: bundle.Header.Guid);
        //}

        // Update header with ContentTypeName in case it wasn't there before
        list = list
            .Select(bundle => bundle.Header!.ContentTypeName == null && bundle.Entity != null
                ? bundle
                : bundle with { Header = bundle.Header! with { ContentTypeName = bundle.Entity!.Type.NameId } }
            )
            .ToList();
        //foreach (var itm in list.Where(i => i.Header!.ContentTypeName == null && i.Entity != null))
        //    itm.Header = itm.Header! with { ContentTypeName = itm.Entity!.Type.NameId }; 

        // Add EditInfo for read-only data
        list = list
            .Select(bundle => bundle with { Header = bundle.Header! with { EditInfo = new(bundle.Entity) } })
            .ToList();
        //foreach (var bundle in list)
        //    bundle.Header = bundle.Header! with { EditInfo = new(bundle.Entity) };

        return list;
    }


    private IEntity GetEditableEditionAndMaybeCloneIt(IAppWorkCtxPlus appWorkCtxPlus, ItemIdentifier p)
    {
        var appState = appWorkCtxPlus.AppReader;
        var found = appState.List.GetOrThrow(p.ContentTypeName!, p.DuplicateEntity ?? p.EntityId);
        // if there is a draft, use that for editing - not the original
        found = appState.GetDraft(found) ?? found;

        // If we want the original (not a copy for new) then stop here
        if (!p.DuplicateEntity.HasValue)
            return found;

        // TODO: 2023-02-25 seems that EntityId is reset, but RepositoryId isn't - not sure why or if this is correct
        var copy = entityAssembler.CreateFrom(found, id: 0, guid: Guid.Empty);
        return copy;
    }

    /// <summary>
    /// clean up content-type names in case it's using the nice-name instead of the static name...
    /// </summary>
    private List<ItemIdentifier> ReplaceSimpleTypeNames(IAppWorkCtxPlus appWorkCtxPlus, List<ItemIdentifier> items)
    {
        var result = items
            .Select(itm =>
            {
                if (string.IsNullOrEmpty(itm.ContentTypeName))
                    return itm;
                
                var ct = appWorkCtxPlus.AppReader.TryGetContentType(itm.ContentTypeName!);
                if (ct == null)
                {
                    return !itm.ContentTypeName!.StartsWith("@")
                        ? throw new("Can't find content type " + itm.ContentTypeName)
                        : null;
                }
                if (ct.NameId != itm.ContentTypeName) // not using the static name...fix
                    return itm with {ContentTypeName = ct.NameId};
                return itm;
            })
            .Where(itm => itm != null!)
            .ToList();
        
        //foreach (var itm in items.Where(i => !string.IsNullOrEmpty(i.ContentTypeName)).ToArray())
        //{
        //    var ct = appWorkCtxPlus.AppReader.TryGetContentType(itm.ContentTypeName!);
        //    if (ct == null)
        //    {
        //        if (!itm.ContentTypeName!.StartsWith("@"))
        //            throw new("Can't find content type " + itm.ContentTypeName);
        //        items.Remove(itm);
        //        continue;
        //    }
        //    if (ct.NameId != itm.ContentTypeName) // not using the static name...fix
        //        itm.ContentTypeName = ct.NameId;
        //}

        return result;
    }

}