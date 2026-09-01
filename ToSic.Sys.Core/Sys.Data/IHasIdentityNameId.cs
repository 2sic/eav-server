namespace ToSic.Sys.Data;

/// <summary>
/// Special interface to ensure consistency across the code base.
/// It's meant to ensure that any data which has an identity can also provide a string-based ID from that (could be a Guid.ToString()) or a real unique name.
/// </summary>
/// <remarks>
/// * Was public till v22, but not certain if it's a good idea, made private again.
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasIdentityNameId
{
    /// <summary>
    /// Primary identifier of an object which has this property.
    /// It will be unique and used as an ID where needed.
    /// </summary>
    public string NameId { get; }
}