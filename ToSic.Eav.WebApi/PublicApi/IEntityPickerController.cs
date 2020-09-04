using System.Collections.Generic;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
using FromUriAttribute = Microsoft.AspNetCore.Mvc.FromRouteAttribute;
#endif
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntityPickerController
    {
        IEnumerable<EntityForPickerDto> GetAvailableEntities([FromUri]int appId, [FromBody] string[] items, [FromUri] string contentTypeName = null, [FromUri] int? dimensionId = null);
    }
}