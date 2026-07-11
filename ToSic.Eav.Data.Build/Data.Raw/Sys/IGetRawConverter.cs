namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// WIP v22 2dm
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IGetRawConverter: IConvertibleToRawEntity
{
    /// <summary>
    /// Get a raw-entity converter for this object.
    /// </summary>
    /// <returns>An instance of the converter or if it should use the default, returns null.</returns>
    IRawEntityConverter GetConverter();
}