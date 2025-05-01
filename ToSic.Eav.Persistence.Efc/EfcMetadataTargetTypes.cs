namespace ToSic.Eav.Persistence.Efc;

public class EfcMetadataTargetTypes(LazySvc<EavDbContext> dbLazy) : ServiceBase("Eav.MdTTyp"), ITargetTypes
{
    private const string ReservedType = "Reserved";

    public int GetId(string targetTypeName)
    {
        var found= TargetTypes.FirstOrDefault(mt => mt.Value == targetTypeName);
        if (!found.Equals(default(KeyValuePair<int, string>))) return found.Key;

        // not found yet, check if we got an int and should just return that
        if (int.TryParse(targetTypeName, out var id)) return id;
        throw new ArgumentException($"Tried to find metadata-target name but failed for '{targetTypeName}'", nameof(targetTypeName));
    }

    public string GetName(int typeId)
    {
        if (!TargetTypes.TryGetValue(typeId, out var result))
            throw new ArgumentException($"Tried to get TargetType name of '{typeId}' but couldn't find it in the list.", nameof(typeId));

        // if it's a reserved key, then future lookups would fail completely (as it exists many times), so in that case 
        // better safely just return the ID
        // note that this isn't perfect for data export/import, but for editing round-trips it's fine
        return result != ReservedType ? result : typeId.ToString();
    }

    public ImmutableDictionary<int, string> TargetTypes => _targetTypes ??= GetTargetTypes();
    private static ImmutableDictionary<int, string> _targetTypes;

    /// <summary>
    /// this is only needed once per application cycle, as the result is fully cached
    /// </summary>
    /// <returns></returns>
    protected virtual ImmutableDictionary<int, string> GetTargetTypes()
    {
        // Must debug why this simple code seems to take 1 second
        var l = Log.Fn<ImmutableDictionary<int, string>>(timer: true);
        var db = dbLazy.Value;
        l.A($"got db connection");
        var targetTypes = db.TsDynDataTargetTypes.ToList();
        l.A($"got {targetTypes.Count} assignment object types");
        var dic = targetTypes
            .ToImmutableDictionary(a => a.TargetTypeId, a => a.Name);
        return l.Return(dic);
    }


}