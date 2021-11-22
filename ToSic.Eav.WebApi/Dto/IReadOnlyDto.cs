using Newtonsoft.Json;

namespace ToSic.Eav.WebApi.Dto
{
    /// <summary>
    /// Extends common DTOs to inform about it being read-only, and why
    /// </summary>
    public interface IReadOnlyDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        bool? IsReadOnly { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string IsReadOnlyReason { get; set; }
    }
}
