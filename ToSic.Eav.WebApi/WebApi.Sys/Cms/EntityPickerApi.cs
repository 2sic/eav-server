﻿using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Users;
using static System.String;


namespace ToSic.Eav.WebApi.Sys.Cms;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityPickerApi(GenWorkPlus<WorkEntities> workEntities, IZoneCultureResolver cultureResolver, IUser user)
    : ServiceBase("Api.EntPck", connect: [cultureResolver, workEntities, user])
{
    /// <summary>
    /// Returns a list of entities, optionally filtered by contentType.
    /// </summary>
    // 2dm 2023-01-22 #maybeSupportIncludeParentApps
    public List<EntityForPickerDto> GetForEntityPicker(int appId, ICollection<string> items, string? contentTypeName, bool? withDrafts = default, bool allowFromAllScopes = default)
    {
        var l = Log.Fn<List<EntityForPickerDto>>($"Get entities for a#{appId}, items⋮{items?.Count}, type:{contentTypeName}");

        var appEnts = workEntities.New(appId, showDrafts: withDrafts);
        IContentType? contentType = null;
        if (!IsNullOrEmpty(contentTypeName))
        {
            contentType = IsNullOrWhiteSpace(contentTypeName)
                ? appEnts.AppWorkCtx.AppReader.TryGetContentType(contentTypeName!)
                : null;
            l.A($"tried to get '{contentTypeName}' - found: {contentType != null}");
            if (contentType == null)
                return l.Return([],
                    "A type was specified but not found, will return empty list");
        }

        List<IEntity> list;

        // optionally filter by type
        if (contentType != null)
        {
            l.A($"filter by type:{contentType.Name}");
            list = appEnts.Get(contentTypeName!).ToList();
        }
        else
        {
            l.A("won't filter by type because it's null");
            l.A($"Will restrict by scope if user is not system admin: {user.IsSystemAdmin}");
            list = allowFromAllScopes
                // Get all the data which the current user may see (filtering drafts etc.)
                ? appEnts.AppWorkCtx.Data.List.ToList()
                // Get all content only, and maybe configuration
                : appEnts.OnlyContent(withConfiguration: user.IsSystemAdmin).ToList(); // only super user should also get Configuration
        }

        // optionally filter by IDs
        if (items != null && items.Count > 0)
        {
            l.A($"filter by {items.Count} ids");
            var guids = items.Select(Guid.Parse);
            list = list.Where(e => guids.Contains(e.EntityGuid)).ToList();
        }
        else
            l.A("won't filter by IDs");

        var languagePriorities = cultureResolver.SafeLanguagePriorityCodes();

        var entities = list
            .Select(e => new EntityForPickerDto
            {
                Id = e.EntityId,
                Value = e.EntityGuid,
                Text = GetTitle(e, languagePriorities)
            })
            .OrderBy(set => set.Text.ToString())
            .ToList();

        return l.Return(entities, $"found⋮{entities.Count}");
    }

    private static string GetTitle(IEntity l, string?[] dimensions)
    {
        var title = l.GetBestTitle(dimensions);
        return IsNullOrWhiteSpace(title)
            ? $"(no Title, {l.EntityId})"
            : title!;
    }

}