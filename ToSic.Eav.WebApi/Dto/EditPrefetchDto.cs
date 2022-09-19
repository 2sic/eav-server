using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.WebApi.Dto
{
    public class EditPrefetchDto
    {
        /// <summary>
        /// Dictionary where each field contains a list of ADAM items
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, IEnumerable<AdamItemDto>>> Adam { get; set; }

        /// <summary>
        /// Prefetched entities for entity picker
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<EntityForPickerDto> Entities { get; set; }

        /// <summary>
        /// Prefetched links for hyperlink fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, LinkInfoDto> Links { get; set; }
    }
}
