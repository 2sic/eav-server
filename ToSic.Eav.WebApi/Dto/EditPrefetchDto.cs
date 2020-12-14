using System;
using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class EditPrefetchDto
    {
        public Dictionary<string, string> Links { get; set; }

        public List<EntityForPickerDto> Entities { get; set; }
    }
}
