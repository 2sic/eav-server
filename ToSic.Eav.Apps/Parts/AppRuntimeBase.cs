using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase: AppBase
    {
        public bool ShowDrafts { get; }

        protected AppRuntimeBase(IAppIdentity app, bool showDrafts, ILog parentLog)
        {
            Init(app, new CodeRef(), parentLog, "App.Base");
            ShowDrafts = showDrafts;
        }

        protected AppRuntimeBase(IDataSource data, bool showDrafts, ILog parentLog) 
            : this(data as IAppIdentity, showDrafts, parentLog)
        {
            _data = data;
        }


        #region Data & Cache

        public IDataSource Data => _data ?? (_data = new DataSource(Log).GetPublishing(this, showDrafts: ShowDrafts));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState => State.Get(this);


        #endregion


    }
}
