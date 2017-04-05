using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class EntityRuntime: BaseRuntime
    {
        internal EntityRuntime(AppRuntime app): base (app) { }

        #region Get

        public IEnumerable<IEntity> All => _app.Cache.LightList;

        public IEntity Get(int entityId) => _app.Cache.List[entityId];

        public IEntity Get(Guid entityGuid) => _app.Cache.LightList.FirstOrDefault(e => e.EntityGuid == entityGuid);

        public IEnumerable<IEntity> Get(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: _app.AppId, upstream: _app.Cache, valueCollectionProvider: _app.Cache.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.LightList;
        }

        #endregion


    }
}
