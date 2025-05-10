namespace ToSic.Lib.Data;

/// <summary>
/// This is for any object which does something, but caries with it an original object which is the type
/// of the hosting system (environment)
/// </summary>
/// <typeparam name="T"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
[PrivateApi]
public interface IWrapper<out T>
{
    /// <summary>
    /// The underlying, original object. Helpful for external objects which need the real, underlying item.
    /// <br/>
    /// Avoid using this property for use inside the real object, but create another property for this, so it's easier to spot dependencies.
    /// </summary>
    T? GetContents();
}