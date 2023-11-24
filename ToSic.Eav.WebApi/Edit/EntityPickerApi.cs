using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.Services;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;
using ToSic.Eav.Apps.Work;

namespace ToSic.Eav.WebApi;

public class EntityPickerApi : ServiceBase
{

    #region DI Constructor

    public EntityPickerApi(GenWorkPlus<WorkEntities> workEntities, IZoneCultureResolver cultureResolver, IUser user) : base("Api.EntPck")
    {
        ConnectServices(
            _cultureResolver = cultureResolver,
            _workEntities = workEntities,
            _user = user
        );
    }
    private readonly GenWorkPlus<WorkEntities> _workEntities;
    private readonly IZoneCultureResolver _cultureResolver;
    private readonly IUser _user;

    #endregion

    /// <summary>
    /// Returns a list of entities, optionally filtered by contentType.
    /// </summary>
    // 2dm 2023-01-22 #maybeSupportIncludeParentApps
    public List<EntityForPickerDto> GetForEntityPicker(int appId, string[] items, string contentTypeName, bool? withDrafts = default, bool allowFromAllScopes = default)
    {
        var l = Log.Fn<List<EntityForPickerDto>>($"Get entities for a#{appId}, items⋮{items?.Length}, type:{contentTypeName}");

        var appEnts = _workEntities.New(appId, showDrafts: withDrafts);
        IContentType contentType = null;
        if (!IsNullOrEmpty(contentTypeName))
        {
            contentType = appEnts.AppWorkCtx.AppState.GetContentType(contentTypeName);
            l.A($"tried to get '{contentTypeName}' - found: {contentType != null}");
            if (contentType == null)
                return l.Return(new List<EntityForPickerDto>(),
                    "A type was specified but not found, will return empty list");
        }

        List<IEntity> list;

        // optionally filter by type
        if (contentType != null)
        {
            l.A($"filter by type:{contentType.Name}");
            list = appEnts.Get(contentTypeName).ToList();
        }
        else
        {
            l.A("won't filter by type because it's null");
            l.A($"Will restrict by scope if user is not system admin: {_user.IsSystemAdmin}");
            list = allowFromAllScopes
                // Get all the data which the current user may see (filtering drafts etc.)
                ? appEnts.AppWorkCtx.Data.List.ToList()
                // Get all content only, and maybe configuration
                : appEnts.OnlyContent(withConfiguration: _user.IsSystemAdmin).ToList(); // only super user should also get Configuration
        }

        // optionally filter by IDs
        if (items != null && items.Length > 0)
        {
            l.A($"filter by {items.Length} ids");
            var guids = items.Select(Guid.Parse);
            list = list.Where(e => guids.Contains(e.EntityGuid)).ToList();
        }
        else
            l.A("won't filter by IDs");

        var languagePriorities = _cultureResolver.SafeLanguagePriorityCodes();

        var entities = list.Select(e => new EntityForPickerDto
            {
                Id = e.EntityId,
                Value = e.EntityGuid,
                Text = GetTitle(e, languagePriorities)
            })
            .OrderBy(set => set.Text.ToString())
            .ToList();

        return l.Return(entities, $"found⋮{entities.Count}");
    }

    private static string GetTitle(IEntity l, string[] dimensions)
    {
        var title = l.GetBestTitle(dimensions);
        return IsNullOrWhiteSpace(title) ? "(no Title, " + l.EntityId + ")" : title;
    }

}