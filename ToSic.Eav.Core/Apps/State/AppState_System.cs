using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static System.StringComparison;
using static ToSic.Eav.Configuration.ConfigurationConstants;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        public AppStateSettings SettingsInApp => _settingsInApp ?? (_settingsInApp = new AppStateSettings(this));
        private AppStateSettings _settingsInApp;
        
        ///// <summary>
        ///// The simple list of <em>all</em> entities, used everywhere
        ///// </summary>
        //[PrivateApi("WIP 12.03")]
        //public IEntity SystemSettingsApp
        //{
        //    get
        //    {
        //        var list = SystemSettingsList.List;
        //        return list.Count < 2 
        //            ? list.FirstOrDefault() 
        //            : list.FirstOrDefault(e => string.IsNullOrEmpty(e.GetBestValue<string>(ContentTypeSettingsScope, null)));
        //    }
        //}

        ///// <summary>
        ///// The simple list of <em>all</em> entities, used everywhere
        ///// </summary>
        //[PrivateApi("WIP 12.03")]
        //public IEntity SystemSettingsSite
        //{
        //    get
        //    {
        //        var list = SystemSettingsList.List;
        //        return list.Count < 2 
        //            ? list.FirstOrDefault() 
        //            : list.FirstOrDefault(e => ContentTypeSettingsScopeSite.Equals(e.GetBestValue<string>(ContentTypeSettingsScope, null), InvariantCultureIgnoreCase));
        //    }
        //}

        //private SynchronizedEntityList SystemSettingsList
        //    => _systemSettingsList
        //       ?? (_systemSettingsList = new SynchronizedEntityList(this, () => Index.Values
        //           .Where(e => e.Type.Is(ContentTypeSettings))
        //           .ToImmutableArray()));
        //private SynchronizedEntityList _systemSettingsList;


        //[PrivateApi("WIP 12.03")]
        //public IEntity SiteSettingsCustom
        //    => (_siteSettingsCustom
        //        ?? (_siteSettingsCustom = new SynchronizedEntityList(this,
        //            () => Index.Values.Where(e => e.Type.Is(ContentTypeSiteSettings)).ToImmutableArray())))
        //        .List
        //        .FirstOrDefault();
        //private SynchronizedEntityList _siteSettingsCustom;

        //[PrivateApi("WIP 12.03")]
        //public IEntity GlobalSettingsCustom
        //    => (_globalSettingsCustom
        //        ?? (_globalSettingsCustom = new SynchronizedEntityList(this,
        //            () => Index.Values.Where(e => e.Type.Is(ContentTypeGlobalSettings)).ToImmutableArray())))
        //        .List
        //        .FirstOrDefault();
        //private SynchronizedEntityList _globalSettingsCustom;

    }
}
