using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Query;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class EntityRuntime: RuntimeBase
    {
        internal EntityRuntime(AppRuntime app, Log parentLog): base (app, parentLog) { }

        #region Get

        public IEnumerable<Eav.Interfaces.IEntity> All => App.Cache.List;

        public Eav.Interfaces.IEntity Get(int entityId) => App.Cache.List.One(entityId);

        public Eav.Interfaces.IEntity Get(Guid entityGuid) => App.Cache.List.One(entityGuid);// .FirstOrDefault(e => e.EntityGuid == entityGuid);

        public IEnumerable<Eav.Interfaces.IEntity> Get(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: App.AppId, upstream: App.Cache, valueCollectionProvider: App.Data.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.List;
        }

        #endregion


    }
}
