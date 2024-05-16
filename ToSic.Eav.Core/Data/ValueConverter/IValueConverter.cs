namespace ToSic.Eav.Data;

/// <summary>
/// Marks objects that can convert values - like "file:22" to "/images/logo.jpg" and back.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IValueConverter
{
    /// <summary>
    /// Resolve a value to a reference which is managed by the environment
    /// </summary>
    /// <param name="value">the full value, like "image/logo.jpg"</param>
    /// <returns>The reference, like "file:22".</returns>
    string ToReference(string value);

    /// <summary>
    /// Resolve a reference to a value using the environment resolver
    /// </summary>
    /// <param name="reference">Reference code (or something else) - if not a code, will not resolve</param>
    /// <param name="itemGuid">
    ///     Guid of the item/entity which was using the reference. <br/>
    ///     The Guid is used when security setting only allow resolving within the own item.
    ///     This ensures that external requests cannot just number through all possible IDs. <br/>
    ///     If you use Guid.Empty or don't supply it, it will usually work, except on systems where the security has been extra-hardened. 
    /// </param>
    /// <returns>The value, like the url.</returns>
    string ToValue(string reference, Guid itemGuid = default);
}