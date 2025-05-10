namespace ToSic.Eav.Serialization;

/// <summary>
/// Specs for serializing the type. ATM only used for Metadata!
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class TypeSerialization
{
    /// <summary>
    /// Old property for internal use...
    /// </summary>
    public bool Serialize { get; set; }

    /// <summary>
    /// Old property for internal use...
    /// </summary>
    public bool WithDescription { get; set; }


    /// <summary>
    /// WIP, values probably
    /// - (empty) / "default" don't include
    /// - "object" - the object model
    /// - "flat" = TypeId = "guid"; TypeName = "";
    /// </summary>
    public string SerializeAs { get; set; }

    /// <summary>
    /// Single name - would be used as prefix and add "Id", "Name", "Description" etc.
    /// CSV - would be used exactly as is for only those variations
    /// </summary>
    public string PropertyNames { get; set; }

    public bool? SerializeId { get; set; }

    public bool? SerializeName { get; set; }

    public bool? SerializeDescription { get; set; }

}