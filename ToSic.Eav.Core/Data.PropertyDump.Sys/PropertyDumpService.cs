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
        if (source == null || !dumpers.Any())
            return (null, 0);

        var bestDumper = dumpers
            .Select(d => new
            {
                Dumper = d,
                Ranking = d.IsCompatible(source)
            })
            .OrderByDescending(set => set.Ranking)
            .First();

        return (bestDumper.Ranking > 0 ? bestDumper.Dumper : null, bestDumper.Ranking);
    }

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path)
    {
        var l = Log.Fn<List<PropertyDumpItem>>();

        //if (target is IPropertyLookupDump selfDumping)
        //    return selfDumping._Dump(specs, path);

        var bestDumper = GetBestDumper(target);
        if (bestDumper is { Dumper: not null, Ranking: > 0 })
        {
            var result = bestDumper.Dumper.Dump(target, specs, path, this);
            return l.Return(result, $"using dumper {bestDumper.Dumper.GetType().Name}, got {result.Count}");
        }

        // ATM do this as a last resort, since we want the rest to work first...
        if (target is IPropertyLookupDump selfDumping)
            return selfDumping._Dump(specs, path);

        return l.Return([], "can't dump, no provider and not self-dumping");
    }
}
