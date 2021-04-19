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
        public Dictionary<string, LinkInfoDto> Links { get; set; }

        /// <summary>
        /// Prefetched entities for entity picker
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<EntityForPickerDto> Entities { get; set; }

        /// <summary>
        /// Dictionary where each field contains a list of ADAM items
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, IEnumerable<AdamItemDto>>> Adam { get; set; }
    }
}
