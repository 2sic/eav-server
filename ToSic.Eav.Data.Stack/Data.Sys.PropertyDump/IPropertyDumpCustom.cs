using ToSic.Eav.Data.Sys.PropertyLookup;

namespace ToSic.Eav.Data.Sys.PropertyDump;

/// <summary>
/// Describes objects which will themselves implement property dumping.
/// This is often used for objects which need additional services or other context to do their work.
/// It's also used for generic object wrappers, since not every object should automatically become a property dumper.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyDumpCustom
{
    /// <summary>
    /// Objects which have own implementation of the property dumping should implement this method.
    /// </summary>
    /// <param name="specs"></param>
    /// <param name="path"></param>
    /// <param name="dumpService"></param>
    /// <returns></returns>
    [PrivateApi]
    List<PropertyDumpItem> _DumpProperties(PropReqSpecs specs, string path, IPropertyDumpService dumpService);

}