using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static System.StringComparison;
using static ToSic.Eav.Configuration.ConfigurationConstants;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {

        public AppState Parent { get; }

        internal AppStateSettings(AppState parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        [PrivateApi("WIP 12.03")]
        public IEntity SystemSettings
        {
            get
            {
                var list = SystemSettingsList.List;
                return list.Count < 2
                    ? list.FirstOrDefault()
                    : list.FirstOrDefault(e => string.IsNullOrEmpty(e.GetBestValue<string>(SysSettingsFieldScope, null)));
            }
        }

        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        [PrivateApi("WIP 12.03")]
        public IEntity SystemSettingsEntireSite
        {
            get
            {
                var list = SystemSettingsList.List;
                return list.Count < 2
                    ? list.FirstOrDefault()
                    : list.FirstOrDefault(e => SysSettingsScopeValueSite.Equals(e.GetBestValue<string>(SysSettingsFieldScope, null), InvariantCultureIgnoreCase));
            }
        }

        private SynchronizedEntityList SystemSettingsList
            => _systemSettingsList
               ?? (_systemSettingsList = new SynchronizedEntityList(Parent, () => Parent.Index.Values
                   .Where(e => e.Type.Is(TypeSystemSettings))
                   .ToImmutableArray()));
        private SynchronizedEntityList _systemSettingsList;


        [PrivateApi("WIP 12.03")]
        public IEntity CustomSettings
            => (_siteSettingsCustom
                ?? (_siteSettingsCustom = new SynchronizedEntityList(Parent,
                    () => Parent.Index.Values.Where(e => e.Type.Is(TypeCustomSettings)).ToImmutableArray())))
                .List
                .FirstOrDefault();
        private SynchronizedEntityList _siteSettingsCustom;
        
    }
}
