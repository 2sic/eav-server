using System.Text.Json.Serialization;
using ToSic.Eav.Data;

namespace ToSic.Eav.Metadata.Targets;

/// <summary>
/// Reference to target. Usually used on <see cref="IEntity"/> to define what thing it provides additional metadata for.
/// Basically it contains all the references necessary to identify what it belongs to.
/// </summary>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Target : ITarget
{
    /// <inheritdoc/>
    [JsonIgnore]
    public bool IsMetadata => TargetType != (int)TargetTypes.None;

    /// <inheritdoc/>
    public int TargetType { get; set; } = (int)TargetTypes.None;

    /// <inheritdoc/>
    public int? KeyNumber { get; set; }

    /// <inheritdoc/>
    public Guid? KeyGuid { get; set; }

    /// <inheritdoc/>
    public string? KeyString { get; set; }


    /// <summary>
    /// Constructor for a new MetadataTarget, which is empty.
    /// </summary>
    public Target() { }

    /// <summary>
    /// Constructor for a new MetadataTarget, which is empty.
    /// </summary>
    [PrivateApi]
    public Target(int targetType, string? title, string? keyString = default, int? keyNumber = default, Guid? keyGuid = default)
    {
        TargetType = targetType;
        Title = title ?? "";
        KeyString = keyString;
        KeyNumber = keyNumber;
        KeyGuid = keyGuid;
    }
    /// <summary>
    /// Constructor for a new MetadataTarget. The object is try-used for every key type to automatically apply to string/int/guid.
    /// </summary>
    [PrivateApi]
    public Target(int targetType, string? title, object? key): this(targetType, title, key as string, key as int?, key as Guid?)
    { }

    /// <summary>
    /// Constructor to copy an existing MetadataFor object. 
    /// </summary>
    [PrivateApi("not sure if this should be public, since we don't have a proper cloning standard")]
    public Target(ITarget originalToCopy, Guid? keyGuid = default)
    {
        TargetType = originalToCopy.TargetType;
        KeyString = originalToCopy.KeyString;
        KeyNumber = originalToCopy.KeyNumber;
        KeyGuid = keyGuid ?? originalToCopy.KeyGuid;
        Title = originalToCopy.Title;
    }

    [JsonIgnore]
    [PrivateApi("WIP v13")]
    public string Title { get; init; } = "";

    public string[] Recommendations { get; set; } = [];

    public override string ToString() => 
        base.ToString() + $" - Type: {TargetType}; $: '{KeyString}', #: '{KeyNumber}'; Guid: '{KeyGuid}'; Title: '{Title}'";
}