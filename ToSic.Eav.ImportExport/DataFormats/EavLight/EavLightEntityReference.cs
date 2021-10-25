using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataFormats.EavLight
{
    /// <summary>
    /// DTO for a relationship pointer to other entities.
    /// 
    /// Used in preparing Entities for Basic-JSON serialization.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("DTO objects are only publicly documented but can change with time. You usually will not need them in your code. ")]
    public class EavLightEntityReference
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Title;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
    }
}
