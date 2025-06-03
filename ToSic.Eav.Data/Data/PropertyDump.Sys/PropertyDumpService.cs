using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.PropertyDump.Sys;
public class PropertyDumpService(IEnumerable<IPropertyDumper> dumpers): ServiceBase("Eav.DpmSvc"), IPropertyDumpService
{
    /// <summary>
    /// Find the best dumper and return with ranking.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public (IPropertyDumper? Dumper, int Ranking) GetBestDumper(object source)
    {
        var l = Log.Fn<(IPropertyDumper? Dumper, int Ranking)>("bestDumper");
        if (source == null || !dumpers.Any())
            return l.Return((null, 0), "no source or no dumpers");

        var bestDumper = dumpers
            .Select(d => new
            {
                Dumper = d,
                Ranking = d.IsCompatible(source)
            })
            .OrderByDescending(set => set.Ranking)
            .First();

        var useDumper = bestDumper.Ranking > 0 ? bestDumper.Dumper : null;
        return l.Return((useDumper, bestDumper.Ranking), $"Use dumper: {useDumper?.GetType().Name != null}; Ranking: {bestDumper.Ranking}");
    }

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path)
    {
        var l = Log.Fn<List<PropertyDumpItem>>();

        // If it's self dumping for an important reason, use that.
        if (target is IPropertyDumpCustom selfDumping)
            return selfDumping._DumpProperties(specs, path, this);

        var bestDumper = GetBestDumper(target);
        if (bestDumper is { Dumper: not null, Ranking: > 0 })
        {
            var result = bestDumper.Dumper.Dump(target, specs, path, this);
            return l.Return(result, $"got {result.Count}");
        }

        return l.Return([], "can't dump, no provider and not self-dumping");
    }
}
