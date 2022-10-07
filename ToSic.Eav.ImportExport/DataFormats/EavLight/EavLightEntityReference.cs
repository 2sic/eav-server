using System;
using System.Text.Json.Serialization;
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? Id;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string Title;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Guid? Guid;
    }
}
