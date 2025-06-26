namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// DTO for a relationship pointer to other entities.
/// 
/// Used in preparing Entities for Basic-JSON serialization.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("DTO objects are only publicly documented but can change with time. You usually will not need them in your code. ")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavLightEntityReference
{
    [JsonIgnore(Condition = WhenWritingNull)] public int? Id;

    [JsonIgnore(Condition = WhenWritingNull)] public string? Title;

    [JsonIgnore(Condition = WhenWritingNull)] public Guid? Guid;
}