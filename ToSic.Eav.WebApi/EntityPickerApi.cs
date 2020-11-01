using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Dto;
using static System.String;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerApi : HasLog
    {
        public EntityPickerApi(ILog parentLog) : base("Api.EntPck", parentLog)
        {
        }

        /// <summary>
        /// Returns a list of entities, optionally filtered by contentType.
        /// </summary>
        public IEnumerable<EntityForPickerDto> GetAvailableEntities(int appId, string[] items, string contentTypeName, bool withDrafts, int? dimensionId)
        {
            Log.Add($"Get entities for a#{appId}, itms⋮{items?.Length}, type:{contentTypeName}, lang#{dimensionId}");
            var dimensionIds = dimensionId ?? 0;

            var appRead = new AppRuntime().Init(State.Identity(null, appId), withDrafts, Log);

            IContentType contentType = null;
            if (!IsNullOrEmpty(contentTypeName))
            {
                contentType = appRead.ContentTypes.Get(contentTypeName);
                Log.Add($"tried to get '{contentTypeName}' - found: {contentType != null}");
            }

            IEnumerable<IEntity> temp;

            // optionally filter by type
            if (contentType != null)
            {
                Log.Add($"filter by type:{contentType.Name}");
                temp = appRead.Entities.Get(contentTypeName);
            }
            else
            {
                temp = appRead.Entities.OnlyContent;
                Log.Add("won't filter by type because it's null");
            }

            // optionally filter by IDs
            if (items != null && items.Length > 0)
            {
                Log.Add("filter by ids");
                var guids = items.Select(Guid.Parse);
                temp = temp.Where(e => guids.Contains(e.EntityGuid));
            }
            else
                Log.Add("won't filter by IDs");

            var entities = temp.Select(l => new EntityForPickerDto
                {
                    Id = l.EntityId,
                    Value = l.EntityGuid,
                    Text = GetTitle(l, dimensionIds)
                })
                .OrderBy(l => l.Text.ToString())
                .ToList();

            Log.Add($"found⋮{entities.Count}");
            return entities;
        }

        private string GetTitle(IEntity l, int dimensionIds)
        {
            string title;

            // if the title is an entity-picker, try to find the inner-title 
            // of the chosen title-item
            if (l.Title != null && l.Title.Type == "Entity")
            {
                var val = l.GetBestValue(Constants.EntityFieldTitle) as IEnumerable<IEntity>;
                title = val?.FirstOrDefault()?.GetBestTitle();
            }
            else
                // default: just get the preferred title
                title = l.Title?[dimensionIds]?.ToString();

            return IsNullOrWhiteSpace(title)
                ? "(no Title, " + l.EntityId + ")"
                : title;
        }

    }
}