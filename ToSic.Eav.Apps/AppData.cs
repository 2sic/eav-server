﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;

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
        public override string LogId => "DS.AppCUD";

        public AppData(LazyInit<SimpleDataController> dataController, IAppStates appStates): base(appStates) 
            => DataController = dataController.SetInit(dc => dc.Init(Log).Init(ZoneId, AppId, false));

        #endregion

        /// <summary>
        /// Get a correctly instantiated instance of the simple data controller once needed.
        /// </summary>
        /// <returns>An data controller to create, update and delete entities</returns>
        //private SimpleDataController DataController() => _dataController ?? (_dataController = _lazyDataController.Ready.Init(ZoneId, AppId));
        //private SimpleDataController _dataController;
        private LazyInit<SimpleDataController> DataController { get; }

        /// <inheritdoc />
        public IEntity Create(string contentTypeName,
            Dictionary<string, object> values, 
            string userName = null,
            ITarget target = null)
        {
            var wrapLog = Log.Call<IEntity>(contentTypeName);
            if (!string.IsNullOrEmpty(userName)) ProvideOwnerInValues(values, userName); // userName should be in 2sxc user IdentityToken format (eg 'dnn:user=N')
            var ids = DataController.Ready.Create(contentTypeName, new List<Dictionary<string, object>> {values}, target);
            var id = ids.FirstOrDefault();
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            // try to find it again (AppState.List contains also draft items)
            var created = AppState.List.One(id);
            return wrapLog(null, created);
        }

        private static void ProvideOwnerInValues(Dictionary<string, object> values, string userIdentityToken)
        {
            // userIdentityToken is not simple 'userName' string, but 2sxc user IdentityToken structure (eg 'dnn:user=N')
            if (values.Any(v => v.Key.ToLowerInvariant() == Attributes.EntityFieldOwner)) return;
            values.Add(Attributes.EntityFieldOwner, userIdentityToken);
        }

        /// <inheritdoc />
        public IEnumerable<IEntity> Create(string contentTypeName, 
            IEnumerable<Dictionary<string, object>> multiValues, 
            string userName = null)
        {
            var wrapLog = Log.Call<IEnumerable<IEntity>>(null, $"app create many ({multiValues.Count()}) new entities of type:{contentTypeName}");
            
            if (!string.IsNullOrEmpty(userName))
                foreach (var values in multiValues)
                    ProvideOwnerInValues(values, userName); // userName should be in 2sxc user IdentityToken format (eg 'dnn:user=N')

            var ids = DataController.Ready.Create(contentTypeName, multiValues);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            var created = List.Where(e => ids.Contains(e.EntityId)).ToList();
            return wrapLog(null, created);
        }

        /// <inheritdoc />
        public void Update(int entityId, Dictionary<string, object> values, string userName = null)
        {
            var wrapLog = Log.Call($"app update i:{entityId}");
            // userName is not used (to change owner of updated entity).
            DataController.Ready.Update(entityId, values);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            FlushDataSnapshot();
            wrapLog(null);
        }


        /// <inheritdoc />
        public void Delete(int entityId, string userName = null)
        {
            var wrapLog = Log.Call($"app delete i:{entityId}");
            // userName is not used (to change owner of deleted entity).
            DataController.Ready.Delete(entityId);
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

        /// <inheritdoc />
        public IEnumerable<IEntity> GetMetadata<TKey>(int targetType, TKey key, string contentTypeName = null) 
            => AppState.GetMetadata(targetType, key, contentTypeName);

        /// <inheritdoc />
        public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null) 
            => AppState.GetMetadata(targetType, key, contentTypeName);

        /// <inheritdoc />
        public IEnumerable<IEntity> GetCustomMetadata<TKey>(TKey key, string contentTypeName = null)
            => AppState.GetMetadata(TargetTypes.Custom, key, contentTypeName);
    }
}