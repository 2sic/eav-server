using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Logging.Simple;
using static System.String;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerController : Eav3WebApiBase
    {
        public EntityPickerController(Log parentLog = null) : base(parentLog)
        {
            Log.Rename("EntPck");
        }

        /// <summary>
        /// Returns a list of entities, optionally filtered by AttributeSetId.
        /// </summary>
        [HttpGet]
        [HttpPost]
        public IEnumerable<dynamic> GetAvailableEntities([FromUri]int appId, [FromBody] string[] items, [FromUri] string contentTypeName = null, [FromUri] int? dimensionId = null)
        {
            Log.Add($"Get entities for a#{appId}, itms⋮{items?.Length}, type:{contentTypeName}, lang#{dimensionId}");
            var dimensionIds = dimensionId ?? 0;

            Interfaces.IContentType contentType = null;
            if (!IsNullOrEmpty(contentTypeName))
                contentType = DataSource.GetCache(null, appId).GetContentType(contentTypeName);

            var dsrc = DataSource.GetInitialDataSource(null, appId);
            var temp = dsrc["Default"].LightList;

            // optionally filter by type
            if (contentType != null)
            {
                Log.Add($"filter by type:{contentType.Name}");
                temp = temp.Where(l => l.Type == contentType);
            }
            else
                Log.Add("won't filter by type");

            // optionally filter by IDs
            if (items != null && items.Length > 0)
            {
                Log.Add("filter by ids");
                var guids = items.Select(Guid.Parse);
                temp = temp.Where(e => guids.Contains(e.EntityGuid));
            }
            else
                Log.Add("won't filter by IDs");

            var entities = (from l in temp
                           select new
                           {
                               Id = l.EntityId,
                               Value = l.EntityGuid,
                               Text = GetTitle(l, dimensionIds)
                           }).OrderBy(l => l.Text.ToString()).ToList();

            Log.Add($"found⋮{entities.Count}");
            return entities;
        }

        private string GetTitle(Interfaces.IEntity l, int dimensionIds)
        {
            string title;

            // if the title is an entity-picker, try to find the inner-title 
            // of the chosen title-item
            if (l.Title != null && l.Title.Type == "Entity")
            {
                var val = l.GetBestValue(Constants.EntityFieldTitle) as Data.EntityRelationship;
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