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
        public IEntity MetadataItem => (_appItemSynced ?? (_appItemSynced = AppStateSettings.BuildSynchedMetadata(Owner, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;

        public IEntity ScopeAny => SysEntitiesInApp.List.FirstOrDefault();

        // 2022-01-24 2dm - we used to have "ResourcesCustom" and "SettingsCustom" but I believe we won't need this in v13 any more
        // But leave till end of June 2022 in case someone already used this and we're not sure what happened...
        ///// <summary>
        ///// The custom settings - usually a type "Settings" or "Resources" which is on the site-app or global-app
        ///// </summary>
        //public IEntity Custom
        //    => (_siteSettingsCustom ?? (_siteSettingsCustom = MakeSyncListOfType(Target.CustomType)))
        //        .List
        //        .FirstOrDefault();
        //private SynchronizedEntityList _siteSettingsCustom;

        #region Private Helpers

        /// <summary>
        /// All SystemSettings entities - on the content-app, there could be two which are relevant
        /// 1. The one with an empty SettingsEntityScope
        /// </summary>
        private SynchronizedEntityList SysEntitiesInApp => _sysSettingEntities ?? (_sysSettingEntities = MakeSyncListOfType(Target.SystemType));
        private SynchronizedEntityList _sysSettingEntities;

        private SynchronizedEntityList MakeSyncListOfType(string typeName) 
            => new SynchronizedEntityList(Owner, () => Owner.List.OfType(typeName).ToImmutableArray());


        #endregion
    }
}
