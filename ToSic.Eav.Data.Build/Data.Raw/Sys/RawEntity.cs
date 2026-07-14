using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// A ready-to-use <see cref="IRawEntity"/> which receives all the data in the constructor.
///
/// Use this for scenarios where you don't want to create your own IRawEntity but wish to return this kind of typed object.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RawEntity: RawEntityBase, IHasMetadata
{
    public RawEntity()
    { }

    public RawEntity(Dictionary<string, object?> values)
    {
        Values = values?.ToInvariant();
    }

    public override IDictionary<string, object?> Values => field
        ??= new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// WIP experimental v18.02 - trying to get content-type metadata into the raw entity
    /// </summary>
    public IMetadata? Metadata { get; init; }
}