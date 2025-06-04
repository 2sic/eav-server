using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.PropertyDump.Sys;

/// <summary>
/// Describes objects which will themselves implement property dumping.
/// This is often used for objects which need additional services or other context to do their work.
/// It's also used for generic object wrappers, since not every object should automatically become a property dumper.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyDumpCustom
{
    // Note 2025-06-03 @2dm: This method was temporarily called _DumpNameWipDroppingMostCases
    // and then most cases were disabled. This way it's easy to find them if it turns out we need more implementations.
    // All commented out code is also marked with #DropUseOfDumpProperties
    // Should probably be removed in 2025-10 or v21, so we can remove the #DropUseOfDumpProperties comments.

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