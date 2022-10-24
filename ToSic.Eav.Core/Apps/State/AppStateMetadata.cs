using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps
{
    public class AppStateMetadata
    {
        /// <summary>
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="target"></param>
        internal AppStateMetadata(AppState owner, AppThingsIdentifiers target)
        {
            Owner = owner;
            Target = target;
        }

        private AppState Owner { get; }
        private AppThingsIdentifiers Target { get; }


        public IEntity AppConfiguration
            => (_appConfigSynched ?? (_appConfigSynched = BuildSynchedMetadata(Owner, AppLoadConstants.TypeAppConfig))).Value;
        private SynchronizedObject<IEntity> _appConfigSynched;

        internal static SynchronizedObject<IEntity> BuildSynchedMetadata(AppState parent, string staticName)
        {
            var synched = new SynchronizedObject<IEntity>(parent,
                () => parent.Metadata.FirstOrDefaultOfType(staticName)/* .FirstOrDefault(md => md.Type.NameId == staticName)*/);
            return synched;
        }

        internal static SynchronizedObject<IEntity> BuildSynchedItem(AppState parent, string staticName)
        {
            var synched = new SynchronizedObject<IEntity>(parent,
                () => parent.List.FirstOrDefaultOfType(staticName) /*.FirstOrDefault(md => md.Type.NameId == staticName) */);
            return synched;
        }


        /// <summary>
        /// The App-Settings or App-Resources
        /// </summary>
        public IEntity MetadataItem => (_appItemSynced ?? (_appItemSynced = BuildSynchedMetadata(Owner, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;

        public IEntity SystemItem => (_appSystemSynched ?? (_appSystemSynched = BuildSynchedItem(Owner, Target.SystemType))).Value;
        private SynchronizedObject<IEntity> _appSystemSynched;

        // 2022-01-24 2dm - we used to have "ResourcesCustom" and "SettingsCustom" but I believe we won't need this in v13 any more
        // But leave till end of June 2022 in case someone already used this and we're not sure what happened...
        // 2022-03-11 2dm Revised - we need this on the primary App and Global App ! 
        ///// <summary>
        ///// The custom settings - usually a type "Settings" or "Resources" which is on the site-app or global-app
        ///// </summary>
        //public IEntity Custom
        //    => (_siteSettingsCustom ?? (_siteSettingsCustom = MakeSyncListOfType(Target.CustomType)))
        //        ?.List
        //        ?.FirstOrDefault();
        //private SynchronizedEntityList _siteSettingsCustom;
        public IEntity CustomItem => (_appCustomSynced ?? (_appCustomSynced = BuildSynchedItem(Owner, Target.CustomType))).Value;
        private SynchronizedObject<IEntity> _appCustomSynced;

        #region Private Helpers

        ///// <summary>
        ///// All SystemSettings entities - on the content-app, there could be two which are relevant
        ///// 1. The one with an empty SettingsEntityScope
        ///// </summary>
        //private SynchronizedEntityList SysEntitiesInApp => _sysSettingEntities ?? (_sysSettingEntities = MakeSyncListOfType(Target.SystemType));
        //private SynchronizedEntityList _sysSettingEntities;

        //private SynchronizedEntityList MakeSyncListOfType(string typeName)
        //    => new SynchronizedEntityList(Owner, () => Owner.List.OfType(typeName).ToImmutableArray());


        #endregion
    }
}
