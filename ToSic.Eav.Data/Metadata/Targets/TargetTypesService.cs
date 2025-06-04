using System.Collections.Immutable;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
#if NETFRAMEWORK
#endif

namespace ToSic.Eav.Metadata.Targets;
internal class TargetTypesService(LazySvc<ITargetTypesLoader> loader): ServiceBase("MDa.TTSvc", connect: [loader]), ITargetTypeService
{
    private const string ReservedType = "Reserved";
    
    // Log is usually not needed here
    private const bool EnableLog = false;

    /// <inheritdoc />
    public int GetId(string targetTypeName)
    {
        var l = Log.Fn<int>(targetTypeName, enabled: EnableLog);
        
        if (TargetTypesByName.TryGetValue(targetTypeName, out var key))
            return l.Return(key, "found by name");

        // not found yet, check if we got an int and should just return that
        if (int.TryParse(targetTypeName, out var id))
            return l.Return(id, $"converted id: {id}");

        throw l.Ex(new ArgumentException($"Can't find metadata-target '{targetTypeName}'", nameof(targetTypeName)));
    }

    /// <inheritdoc />
    public string GetName(int typeId)
    {
        var l = Log.Fn<string>(typeId.ToString(), enabled: EnableLog);
        if (!TargetTypes.TryGetValue(typeId, out var result))
            throw l.Ex(new ArgumentException($"Tried to get TargetType name of '{typeId}' but couldn't find it in the list.", nameof(typeId)));

        // if it's a reserved key, then future lookups would fail completely (as it exists many times), so in that case 
        // better safely just return the ID
        // note that this isn't perfect for data export/import, but for editing round-trips it's fine
        var final = result != ReservedType
            ? result
            : typeId.ToString();
        return l.Return(final, $"found name: {final}");
    }

    private Dictionary<string, int> TargetTypesByName => _targetTypesByName
        ??= TargetTypes
            .DistinctBy(pair => pair.Value) // Filter out duplicate names, keeping the first one
            .ToDictionary(pair => pair.Value, pair => pair.Key);
    private static Dictionary<string, int> _targetTypesByName;

    public ImmutableDictionary<int, string> TargetTypes => _targetTypesStatic ??= loader.Value.GetTargetTypes();
    private static ImmutableDictionary<int, string> _targetTypesStatic;

}
