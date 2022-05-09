﻿using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppSettingsStack
    {
        public List<KeyValuePair<string, IPropertyLookup>> GetStack(AppThingsIdentifiers target, IEntity viewPart)
        {
            var wrapLog = Log.Call<List<KeyValuePair<string, IPropertyLookup>>>(target.Target.ToString());

            Log.Add($"Has View: {viewPart != null}");
            // "View" Settings/Resources - always add, no matter if null, so the key always exists
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                new KeyValuePair<string, IPropertyLookup>(PartView, viewPart)
            };

            // All in the App and below
            sources.AddRange(GetOrGenerate(target).FullStack(Log));
            return wrapLog($"Has {sources.Count}", sources);
        }

        public const string PiggyBackId = "app-stack-";

        private AppStateStackCache GetOrGenerate(AppThingsIdentifiers target)
        {
            var wrapLog = Log.Call<AppStateStackCache>(target.Target.ToString());
            return wrapLog(null, Owner.PiggyBack.GetOrGenerate(PiggyBackId + target.Target, () => Get(target)));
        }

        private AppStateStackCache Get(AppThingsIdentifiers target)
        {
            var wrapLog = Log.Call<AppStateStackCache>();

            // Site should be skipped on the global zone
            Log.Add($"Owner: {Owner.Show()}");
            var site = Owner.ZoneId == Constants.DefaultZoneId ? null : _appStates.GetPrimaryApp(Owner.ZoneId, Log);
            Log.Add($"Site: {site?.Show()}");
            var global = _appStates.Get(Constants.GlobalIdentity);
            Log.Add($"Global: {global?.Show()}");
            var preset = _appStates.GetPresetApp();
            Log.Add($"Preset: {preset?.Show()}");

            // Find the ancestor, but only use it if it's not the preset
            var appAncestor = Owner.ParentApp.AppState;
            var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == Constants.PresetAppId ? null : appAncestor;
            Log.Add($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {Constants.PresetAppId}");

            var stackCache = new AppStateStackCache(Owner, ancestorIfNotPreset, site, global, preset, target);

            return wrapLog("created", stackCache);
        }
    }
}
