using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static System.StringComparison;
using static ToSic.Eav.Configuration.ConfigurationConstants;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This object creates caches for Settings or Resources
    /// It will handle all kinds of thing-lists incl. SystemSettings / SystemResources, Settings, Resources etc.
    /// </summary>
    [PrivateApi]
    public class AppStateStackCache
    {
        public readonly AppThingsIdentifiers Target;

        public AppState Parent { get; }

        internal AppStateStackCache(AppState parent, AppThingsIdentifiers target)
        {
            Target = target;
            Parent = parent;
        }

        /// <summary>
        /// The App-Settings or App-Resources
        /// </summary>
        public IEntity AppItem => (_appItemSynced ?? (_appItemSynced = AppStateSettings.BuildSynchedMetadata(Parent, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;


        private IEntity SelectAppOrSiteItem(IImmutableList<IEntity> list, bool siteScope) =>
            siteScope 
                ? list.FirstOrDefault(e => SysSettingsScopeValueSite.Equals(e.GetBestValue<string>(SysSettingsFieldScope, null), InvariantCultureIgnoreCase))
                : list.FirstOrDefault(e => string.IsNullOrEmpty(e.GetBestValue<string>(SysSettingsFieldScope, null)));

        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        [PrivateApi("WIP 12.03")]
        public IEntity ForApp => SelectAppOrSiteItem(ThingEntitiesInApp.List, false);

        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// Only relevant on the primary content-app
        /// </summary>
        [PrivateApi("WIP 12.03")]
        public IEntity ForSite => SelectAppOrSiteItem(ThingEntitiesInApp.List, true);

        /// <summary>
        /// All SystemSettings entities - on the content-app, there could be two which are relevant
        /// 1. The one with an empty SettingsEntityScope
        /// </summary>
        private SynchronizedEntityList ThingEntitiesInApp => _sysSettingEntities ?? (_sysSettingEntities = MakeSyncListOfType(Target.SystemType));
        private SynchronizedEntityList _sysSettingEntities;

        private SynchronizedEntityList MakeSyncListOfType(string typeName)
            => new SynchronizedEntityList(Parent, () => Parent.Index.Values.Where(e => e.Type.Is(typeName)).ToImmutableArray());

        [PrivateApi("WIP 12.03")]
        public IEntity Custom
            => (_siteSettingsCustom ?? (_siteSettingsCustom = MakeSyncListOfType(Target.CustomType)))
                .List
                .FirstOrDefault();
        private SynchronizedEntityList _siteSettingsCustom;

    }
}
