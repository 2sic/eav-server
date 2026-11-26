using System.Collections.ObjectModel;

namespace ToSic.Eav.Apps.Sys.Caching;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Only relevant for integration / alternate caching systems")]
public class ZoneCachePresetHelper()
{
    /// <summary>
    /// Get the zones from a loader and prepare as a read-only dictionary.
    /// </summary>
    /// <remarks>
    /// Also adds dummy entries for the zone `PresetZoneId` with the `PresetApp` and the `GlobalPresetApp`.
    /// </remarks>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public IReadOnlyDictionary<int, Zone> AddPresetZone(IDictionary<int, Zone> realZones)
    {
        // Load from DB (this will also ensure that Primary Apps are created)
        //var realZones = zoneLoader.Zones();

        var presetZone = GeneratePresetZone();

        realZones.Add(KnownAppsConstants.PresetZoneId, presetZone);

        return new ReadOnlyDictionary<int, Zone>(realZones);
    }

    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    private static Zone GeneratePresetZone()
    {
        // Add the Preset-Zone to the list - important, otherwise everything fails
        var presetApps = new Dictionary<int, string>
        {
            { KnownAppsConstants.PresetAppId, KnownAppsConstants.PresetName },
            { KnownAppsConstants.GlobalPresetAppId, KnownAppsConstants.GlobalPresetName },
        };

        var presetZone = new Zone(KnownAppsConstants.PresetZoneId,
            KnownAppsConstants.PresetAppId,
            KnownAppsConstants.PresetAppId,
            new ReadOnlyDictionary<int, string>(presetApps),
            [
                new()
                {
                    Active = true,
                    Id = 0,
                    EnvironmentKey = "en-us",
                    Key = "en-us",
                    Name = "English",
                    ParentId = null
                }
            ]);
        return presetZone;
    }
}
