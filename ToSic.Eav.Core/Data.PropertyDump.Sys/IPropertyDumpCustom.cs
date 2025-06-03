using ToSic.Eav.Data.Debug;

namespace ToSic.Eav.Data.PropertyLookup;

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
    /// Old name as renamed before dropping its use, so it can be found in the commented out code.
    /// This name and related code marked with #DropUseOfDumpProperties should be removed ca. 2025-10 or v21
    /// </summary>
    /// <param name="specs"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [PrivateApi]
    List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path);

}