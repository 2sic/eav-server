using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.Services;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerApi : ServiceBase
    {
        #region DI Constructor

        public EntityPickerApi(AppRuntime appRuntime, IZoneCultureResolver cultureResolver, IUser user) : base("Api.EntPck")
        {
            ConnectServices(
                _cultureResolver = cultureResolver,
                _user = user,
                AppRuntime = appRuntime
            );
        }
        public AppRuntime AppRuntime { get; }
        private readonly IZoneCultureResolver _cultureResolver;
        private readonly IUser _user;

        #endregion

        /// <summary>
        /// Returns a list of entities, optionally filtered by contentType.
        /// </summary>
        // 2dm 2023-01-22 #maybeSupportIncludeParentApps
        public IEnumerable<EntityForPickerDto> GetForEntityPicker(int appId, string[] items, string contentTypeName, bool withDrafts) => Log.Func(l =>
        {
            l.A($"Get entities for a#{appId}, items⋮{items?.Length}, type:{contentTypeName}");

            AppRuntime.Init(appId, withDrafts);
            IContentType contentType = null;
            if (!IsNullOrEmpty(contentTypeName))
            {
                contentType = AppRuntime.AppState.GetContentType(contentTypeName);
                l.A($"tried to get '{contentTypeName}' - found: {contentType != null}");
                if (contentType == null)
                {
                    l.A("A type was specified but not found, will return empty list");
                    return new List<EntityForPickerDto>();
                }
            }

            IEnumerable<IEntity> list;

            // optionally filter by type
            if (contentType != null)
            {
                l.A($"filter by type:{contentType.Name}");
                list = AppRuntime.Entities.Get(contentTypeName);
            }
            else
            {
                l.A("won't filter by type because it's null");
                l.A($"Will restrict by scope if user is not system admin: {_user.IsSystemAdmin}");
                list = AppRuntime.Entities.OnlyContent(_user.IsSystemAdmin); // only super user should also get Configuration
            }

            // optionally filter by IDs
            if (items != null && items.Length > 0)
            {
                l.A($"filter by {items.Length} ids");
                var guids = items.Select(Guid.Parse);
                list = list.Where(e => guids.Contains(e.EntityGuid));
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

            l.A($"found⋮{entities.Count}");
            return entities;
        });

        private static string GetTitle(IEntity l, string[] dimensions)
        {
            var title = l.GetBestTitle(dimensions);
            return IsNullOrWhiteSpace(title) ? "(no Title, " + l.EntityId + ")" : title;
        }

    }
}