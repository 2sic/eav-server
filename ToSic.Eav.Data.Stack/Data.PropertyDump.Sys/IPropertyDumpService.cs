using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.PropertyDump.Sys;

public interface IPropertyDumpService
{
    /// <summary>
    /// Find the best dumper and return with ranking.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    (IPropertyDumper Dumper, int Ranking) GetBestDumper(object source);

    List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path);
}