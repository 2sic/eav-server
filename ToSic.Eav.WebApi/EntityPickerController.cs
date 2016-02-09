using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ToSic.Eav.WebApi
{
    public class EntityPickerController : ApiController
    {
        /// <summary>
        /// Returns a list of entities, optionally filtered by AttributeSetId.
        /// </summary>
        [HttpGet]
        public IEnumerable<dynamic> GetAvailableEntities(int appId, string contentTypeName = null, int? dimensionId = null)
        {
            var dimensionIds = (dimensionId.HasValue ? dimensionId : 0).Value;

            IContentType contentType = null;
            if (!String.IsNullOrEmpty(contentTypeName))
                contentType = DataSource.GetCache(null, appId).GetContentType(contentTypeName);

            var dsrc = DataSource.GetInitialDataSource(null, appId);
            var entities = (from l in dsrc["Default"].List
                           where contentType == null || l.Value.Type == contentType
                           select new
                           {
                               Id = l.Value.EntityId,
                               Value = l.Value.EntityGuid,
                               Text = l.Value.Title == null || l.Value.Title[dimensionIds] == null || string.IsNullOrWhiteSpace(l.Value.Title[dimensionIds].ToString()) ? "(no Title, " + l.Key + ")" : l.Value.Title[dimensionIds]
                           }).OrderBy(l => l.Text.ToString()).ToList();

            return entities;
        }

    }
}