using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class EntityRuntime: RuntimeBase
    {
        internal EntityRuntime(AppRuntime app): base (app) { }

        #region Get

        public IEnumerable<ToSic.Eav.Interfaces.IEntity> All => App.Cache.LightList;

        public ToSic.Eav.Interfaces.IEntity Get(int entityId) => App.Cache.List[entityId];

        public ToSic.Eav.Interfaces.IEntity Get(Guid entityGuid) => App.Cache.LightList.FirstOrDefault(e => e.EntityGuid == entityGuid);

        public IEnumerable<ToSic.Eav.Interfaces.IEntity> Get(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: App.AppId, upstream: App.Cache, valueCollectionProvider: App.Data.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.LightList;
        }

        #endregion


    }
}
