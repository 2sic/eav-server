using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Dto
{
    public class EditPrefetchDto
    {
        // Note: #AdamItemDto - as of now, we must use object because System.Io.Text.Json will otherwise not convert the object correctly :(

        /// <summary>
        /// Dictionary where each field contains a list of ADAM items
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, IEnumerable</*AdamItemDto*/object>>> Adam { get; set; }

        /// <summary>
        /// Prefetched entities for entity picker
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<EntityForPickerDto> Entities { get; set; }

        /// <summary>
        /// Prefetched links for hyperlink fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, LinkInfoDto> Links { get; set; }

        /// <summary>
        /// Prefetched entities for entity picker
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<JsonEntity> SettingsEntities { get; set; }

    }
}
