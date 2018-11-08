using System.Collections.Generic;
using System.Web.Http;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntityPickerController
    {
        IEnumerable<object> GetAvailableEntities([FromUri]int appId, [FromBody] string[] items, [FromUri] string contentTypeName = null, [FromUri] int? dimensionId = null);
    }
}