﻿using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps
{
    public static class AppStatesExtensions
    {
        public static AppState GetPresetOrNull(this IAppStates states) =>
            (states as AppStates)?.Cache.Has(PresetIdentity) ?? false
                ? states.Get(PresetIdentity)
                : null;

        public static AppState GetPresetApp(this IAppStates states) => states.Get(PresetIdentity);
    }
}