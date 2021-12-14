using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps
{
    public partial class AppStateSettings
    {


        /// <summary>
        /// The App-Settings or App-Resources
        /// </summary>
        public IEntity MetadataItem => (_appItemSynced ?? (_appItemSynced = AppStateSettings.BuildSynchedMetadata(Parent, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;

        ///// <summary>
        ///// The SystemSettings/SystemResources on this App which are scoped to this specific App.
        ///// Used on the main App and on the Global-App (for global settings, which are automatically global)
        ///// </summary>
        //public IEntity ScopeApp => SysEntitiesInApp.List
        //    .FirstOrDefault(e => string.IsNullOrEmpty(e.GetBestValue<string>(SysSettingsFieldScope, null)));

        ///// <summary>
        ///// The SystemSettings/SystemResources on this App which are scoped to this entire Site.
        ///// Only relevant on the primary content-app or on the global-app
        ///// </summary>
        //public IEntity ScopeSite => SysEntitiesInApp.List
        //    .FirstOrDefault(e => SysSettingsScopeValueSite.Equals(e.GetBestValue<string>(SysSettingsFieldScope, null), InvariantCultureIgnoreCase));

        public IEntity ScopeAny => SysEntitiesInApp.List.FirstOrDefault();


        /// <summary>
        /// The custom settings - usually a type "Settings" or "Resources" which is on the site-app or global-app
        /// </summary>
        public IEntity Custom
            => (_siteSettingsCustom ?? (_siteSettingsCustom = MakeSyncListOfType(Target.CustomType)))
                .List
                .FirstOrDefault();
        private SynchronizedEntityList _siteSettingsCustom;

        #region Full Stack


        #endregion

        #region Private Helpers

        /// <summary>
        /// All SystemSettings entities - on the content-app, there could be two which are relevant
        /// 1. The one with an empty SettingsEntityScope
        /// </summary>
        private SynchronizedEntityList SysEntitiesInApp => _sysSettingEntities ?? (_sysSettingEntities = MakeSyncListOfType(Target.SystemType));
        private SynchronizedEntityList _sysSettingEntities;

        private SynchronizedEntityList MakeSyncListOfType(string typeName)
            => new SynchronizedEntityList(Parent, () => Parent.Index.Values.Where(e => e.Type.Is(typeName)).ToImmutableArray());


        #endregion
    }
}
