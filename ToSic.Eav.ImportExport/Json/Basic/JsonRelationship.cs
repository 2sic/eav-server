using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Json.Basic
{
    /// <summary>
    /// A relationship pointer to other entities.
    /// 
    /// Used in preparing Entities for Basic-JSON serialization.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Just FYI")]
    public class JsonRelationship
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Title;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
    }
}
