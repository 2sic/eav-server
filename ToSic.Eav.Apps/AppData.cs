using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The Data object on an App. It's also a data-source of type <see cref="Eav.DataSources.App"/>,
    /// so it has many streams, one for each content-type so you can use it in combination with other DataSources. <br/>
    /// The special feature is that it also has methods for data-manipulation,
    /// including Create, Update and Delete
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public sealed class AppData: Eav.DataSources.App, IAppData
    {

        #region Constructor stuff

        [PrivateApi]
        public override string LogId => "Ds.EavApp";

        public AppData(Lazy<SimpleDataController> dataController, Lazy<IUser> userLazy)
        {
            _lazyDataController = dataController;
            _userLazy = userLazy;
        }

        public AppData Init(IZoneMapper zoneMapper, ISite site)
        {
            _zoneMapper = zoneMapper;
            _site = site;
            return this;
        }

        private IZoneMapper _zoneMapper;
        private ISite _site;

        #endregion

        // todo: this functionality should be moved into the SimpleDataController
        private string GetDefaultLanguage()
        {
            var usesLanguages = _zoneMapper.CulturesWithState(_site.Id, ZoneId)
                .Any(c => c.Active);
            return usesLanguages
                ? _site.DefaultLanguage
                : "";
        }

        [PrivateApi] internal string CurrentUserName => _userLazy.Value.IdentityToken;

        /// <summary>
        /// Get a correctly instantiated instance of the simple data controller.
        /// </summary>
        /// <returns>An data controller to create, update and delete entities</returns>
        private SimpleDataController DataController() =>
            _dataController ?? (_dataController = _lazyDataController.Value.Init(ZoneId, AppId, GetDefaultLanguage(), Log));

        private SimpleDataController _dataController;
        private readonly Lazy<SimpleDataController> _lazyDataController;
        private readonly Lazy<IUser> _userLazy;

        /// <inheritdoc />
        public IEntity Create(string contentTypeName,
            Dictionary<string, object> values, 
            string userName = null,
            ITarget target = null)
        {
            var wrapLog = Log.Call<IEntity>(contentTypeName);
            var ids = DataController().Create(contentTypeName, new List<Dictionary<string, object>> {values}, target);
            var id = ids.FirstOrDefault();
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            // try to find it again
            var created = Immutable.One(id);
            return wrapLog(null, created);
        }

        /// <inheritdoc />
        public IEnumerable<IEntity> Create(string contentTypeName, 
            IEnumerable<Dictionary<string, object>> multiValues, 
            string userName = null)
        {
            var wrapLog = Log.Call<IEnumerable<IEntity>>(null, $"app create many ({multiValues.Count()}) new entities of type:{contentTypeName}");
            var ids = DataController().Create(contentTypeName, multiValues);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            var created = Immutable.Where(e => ids.Contains(e.EntityId)).ToList();
            return wrapLog(null, created);
        }

        /// <inheritdoc />
        public void Update(int entityId, Dictionary<string, object> values,
            string userName = null)
        {
            var wrapLog = Log.Call($"app update i:{entityId}");
            DataController().Update(entityId, values);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            wrapLog(null);
        }


        /// <inheritdoc />
        public void Delete(int entityId, string userName = null)
        {
            var wrapLog = Log.Call($"app delete i:{entityId}");
            DataController().Delete(entityId);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            wrapLog(null);
        }

        /// <summary>
        /// All 2sxc data is always snapshot, so read will only run a query once and keep it till the objects are killed.
        /// If we do updates or perform other changes, we must clear the current snapshot so subsequent access will result
        /// in the new data. 
        /// </summary>
        private void FlushDataSnapshot()
        {
            // Purge the list and parent lists - must happen first, as otherwise the list-access will be interrupted
            PurgeList(true);
            RequiresRebuildOfOut = true;
        }
    }
}