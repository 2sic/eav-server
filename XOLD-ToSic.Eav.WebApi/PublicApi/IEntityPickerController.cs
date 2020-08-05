using System.Collections.Generic;
using System.Web.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntityPickerController
    {
        IEnumerable<EntityForPickerDto> GetAvailableEntities([FromUri]int appId, [FromBody] string[] items, [FromUri] string contentTypeName = null, [FromUri] int? dimensionId = null);
    }
}