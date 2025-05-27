using ToSic.Eav.Metadata;
using static System.StringComparer;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// A ready-to-use <see cref="IRawEntity"/> which receives all the data in the constructor.
///
/// Use this for scenarios where you don't want to create your own IRawEntity but wish to return this kind of typed object.
/// Typical use case is when you implement <see cref="IHasRawEntity{T}"/>
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RawEntity: RawEntityBase
{
    public RawEntity()
    {
    }

    public RawEntity(Dictionary<string, object> values)
    {
        _values = values;
    }

    public IDictionary<string, object> Values
    {
            
        get => _values ??= new Dictionary<string, object>(InvariantCultureIgnoreCase);
        set => _values = value?.ToInvariant() ?? _values;
    }
    private IDictionary<string, object> _values;

    /// <inheritdoc />
    public override IDictionary<string, object> Attributes(RawConvertOptions options) => _values;


    /// <summary>
    /// WIP experimental v18.02 - trying to get content-type metadata into the raw entity
    /// </summary>
    public IMetadataOf Metadata { get; set; }
}