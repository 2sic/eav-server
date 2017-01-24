using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using static System.String;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerController : ApiController
    {
        /// <summary>
        /// Returns a list of entities, optionally filtered by AttributeSetId.
        /// </summary>
        [HttpGet]
        [HttpPost]
        public IEnumerable<dynamic> GetAvailableEntities([FromUri]int appId, [FromBody] string[] items, [FromUri] string contentTypeName = null, [FromUri] int? dimensionId = null)
        {
            var dimensionIds = (dimensionId.HasValue ? dimensionId : 0).Value;

            IContentType contentType = null;
            if (!IsNullOrEmpty(contentTypeName))
                contentType = DataSource.GetCache(null, appId).GetContentType(contentTypeName);

            var dsrc = DataSource.GetInitialDataSource(null, appId);
            var temp = dsrc["Default"].LightList;

            // optionally filter by type
            if (contentType != null)
                temp = temp.Where(l => l.Type == contentType);

            // optionally filter by IDs
            if (items != null && items.Length > 0)
            {
                var guids = items.Select(Guid.Parse);
                temp = temp.Where(e => guids.Contains(e.EntityGuid));
            }

            var entities = (from l in temp
                           select new
                           {
                               Id = l.EntityId,
                               Value = l.EntityGuid,
                               Text = GetTitle(l, dimensionIds) // l.Title?[dimensionIds] == null || IsNullOrWhiteSpace(l.Title[dimensionIds].ToString()) ? "(no Title, " + l.EntityId + ")" : l.Title[dimensionIds]
                           }).OrderBy(l => l.Text.ToString()).ToList();

            return entities;
        }

        private string GetTitle(IEntity l, int dimensionIds)
        {
            string title;

            // if the title is an entity-picker, try to find the inner-title 
            // of the chosen title-item
            if (l.Title.Type == "Entity")
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