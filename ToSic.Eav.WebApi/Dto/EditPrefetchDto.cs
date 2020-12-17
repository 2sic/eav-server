using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.WebApi.Dto
{
    public class EditPrefetchDto
    {
        /// <summary>
        /// Prefetched links for hyperlink fields
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Links { get; set; }

        /// <summary>
        /// Prefetched entities for entity picker
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<EntityForPickerDto> Entities { get; set; }

        /// <summary>
        /// Dictionary where each field contains a list of ADAM items
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<AdamItemDto>> Adam { get; set; }
    }
}
