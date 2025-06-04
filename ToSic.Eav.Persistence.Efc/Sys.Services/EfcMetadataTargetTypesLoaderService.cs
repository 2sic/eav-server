using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Persistence.Efc;

public class EfcMetadataTargetTypesLoaderService(LazySvc<EavDbContext> dbLazy) : ServiceBase("Eav.MdTTyp"), ITargetTypesLoader
{
    /// <summary>
    /// this is only needed once per application cycle, as the result is fully cached
    /// </summary>
    /// <returns></returns>
    public ImmutableDictionary<int, string> GetTargetTypes()
    {
        // Must debug why this simple code seems to take 1 second
        var l = Log.Fn<ImmutableDictionary<int, string>>(timer: true);
        var db = dbLazy.Value;
        
        l.A($"got db connection");
        var targetTypes = db.TsDynDataTargetTypes
            .ToList();

        l.A($"got {targetTypes.Count} assignment object types");
        var dic = targetTypes
            .ToImmutableDictionary(a => a.TargetTypeId, a => a.Name);
        return l.Return(dic, $"{dic.Count}");
    }
}