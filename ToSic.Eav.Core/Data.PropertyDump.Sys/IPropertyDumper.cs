using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data.PropertyDump.Sys;
public interface IPropertyDumper
{
    /// <summary>
    /// Let the property dumper determine if it can handle the target object.
    /// Larger numbers mean more compatible, so 0 is not compatible, 1 is compatible, 2 is very compatible, etc.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public int IsCompatible(object target);

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path, IPropertyDumpService dumpService);
}
