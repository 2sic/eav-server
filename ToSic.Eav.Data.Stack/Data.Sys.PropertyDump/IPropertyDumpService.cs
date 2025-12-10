using ToSic.Eav.Data.Sys.PropertyLookup;

namespace ToSic.Eav.Data.Sys.PropertyDump;

public interface IPropertyDumpService
{
    /// <summary>
    /// Find the best dumper and return with ranking.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    (IPropertyDumper? Dumper, int Ranking) GetBestDumper(object? source);

    List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path);
}