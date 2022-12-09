﻿using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps
{
    public static class AppStateExtensions
    {

        public static bool IsInherited(this AppState appState)
            => appState.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

        public static bool HasCustomParentApp(this AppState states)
        {
            var parentAppGuid = states?.ParentApp?.AppState?.NameId;
            return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid);
        }

        public static bool AppGuidIsAPreset(string parentAppGuid) 
            => parentAppGuid.HasValue() 
               && (parentAppGuid == Constants.PresetName || parentAppGuid == Constants.GlobalPresetName);

        // TODO: @STV - try to use this where possible
        public static bool IsContentApp(this AppState appState)
            => appState.NameId == Constants.DefaultAppGuid;

        // TODO: @STV - try to use this where possible
        public static bool IsGlobalSettingsApp(this AppState appState)
            => appState.AppId == Constants.MetaDataAppId;

        // TODO: @STV - try to use this where possible
        public static bool IsSiteSettingsApp(this AppState appState)
            => appState.NameId == Constants.PrimaryAppGuid;
    }
}
