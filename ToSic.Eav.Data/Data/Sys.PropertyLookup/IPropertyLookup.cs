namespace ToSic.Eav.Data.Sys.PropertyLookup;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyLookup
{
    /// <summary>
    /// Internal helper to get a property with additional information for upstream processing. 
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path);

}