using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    public static class AppStateExtensions
    {

        public static bool IsInherited(this AppState appState)
            => appState.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

        public static bool HasParentApp(this AppState states)
        {
            var parentAppGuid = states?.ParentApp?.AppState?.NameId;
            return (!string.IsNullOrEmpty(parentAppGuid) && parentAppGuid != Constants.PresetName);
        }
    }
}
