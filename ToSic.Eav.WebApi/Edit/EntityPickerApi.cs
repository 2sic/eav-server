using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerApi : HasLog
    {
        #region DI Constructor

        public EntityPickerApi(AppRuntime appRuntime, IZoneCultureResolver cultureResolver, IUser user) : base("Api.EntPck")
        {
            _cultureResolver = cultureResolver;
            _user = user;
            AppRuntime = appRuntime;
        }
        public AppRuntime AppRuntime { get; }
        private readonly IZoneCultureResolver _cultureResolver;
        private readonly IUser _user;

        #endregion

        /// <summary>
        /// Returns a list of entities, optionally filtered by contentType.
        /// </summary>
        public IEnumerable<EntityForPickerDto> GetAvailableEntities(int appId, string[] items, string contentTypeName, bool withDrafts)
        {
            Log.A($"Get entities for a#{appId}, items⋮{items?.Length}, type:{contentTypeName}");

            AppRuntime.Init(appId, withDrafts, Log);
            IContentType contentType = null;
            if (!IsNullOrEmpty(contentTypeName))
            {
                contentType = AppRuntime.AppState.GetContentType(contentTypeName);
                Log.A($"tried to get '{contentTypeName}' - found: {contentType != null}");
                if (contentType == null)
                {
                    Log.A("Since a type was specified and not found, will return empty list");
                    return new List<EntityForPickerDto>();
                }
            }

            IEnumerable<IEntity> temp;

            // optionally filter by type
            if (contentType != null)
            {
                Log.A($"filter by type:{contentType.Name}");
                temp = AppRuntime.Entities.Get(contentTypeName);
            }
            else
            {
                temp = AppRuntime.Entities.OnlyContent(_user.IsSystemAdmin); // only super user should also get Configuration
                Log.A("won't filter by type because it's null");
            }

            // optionally filter by IDs
            if (items != null && items.Length > 0)
            {
                Log.A("filter by ids");
                var guids = items.Select(Guid.Parse);
                temp = temp.Where(e => guids.Contains(e.EntityGuid));
            }
            else
                Log.A("won't filter by IDs");

            var languagePriorities = _cultureResolver.SafeLanguagePriorityCodes();

            var entities = temp.Select(l => new EntityForPickerDto
                {
                    Id = l.EntityId,
                    Value = l.EntityGuid,
                    Text = GetTitle(l, languagePriorities)
                })
                .OrderBy(l => l.Text.ToString())
                .ToList();

            Log.A($"found⋮{entities.Count}");
            return entities;
        }

        private static string GetTitle(IEntity l, string[] dimensions)
        {
            var title = l.GetBestTitle(dimensions);
            return IsNullOrWhiteSpace(title) ? "(no Title, " + l.EntityId + ")" : title;
        }

    }
}