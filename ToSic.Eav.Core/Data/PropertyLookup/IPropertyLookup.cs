using ToSic.Eav.Data.Debug;

namespace ToSic.Eav.Data.PropertyLookup;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyLookup: IPropertyLookupReduced, IPropertyLookupDump;


[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyLookupReduced
{
    /// <summary>
    /// Internal helper to get a property with additional information for upstream processing. 
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path);

}

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyLookupDump
{
    [PrivateApi]
    List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path);

}