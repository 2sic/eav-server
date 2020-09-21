using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEditController
    {
        IEnumerable<EntityForPickerDto> EntityPicker(int appId, string[] items, string contentTypeName = null, int? dimensionId = null);
    }
}