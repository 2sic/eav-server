using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class EntityRuntime: RuntimeBase
    {
        internal EntityRuntime(AppRuntime app, ILog parentLog): base (app, parentLog) { }

        #region Get

        /// <summary>
        /// All entities in the app - this also includes system entities like data-source configuration etc.
        /// </summary>
        public IEnumerable<IEntity> All => App./*Cache*/Data.List;

        /// <summary>
        /// All content-entities. It does not include system-entity items. 
        /// </summary>
        public IEnumerable<IEntity> OnlyContent =>
            App./*Cache*/Data.List.Where(e => AppConstants.ScopesContent.Contains(e.Type.Scope));

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        public IEntity Get(int entityId) => App./*Cache*/Data.List.FindRepoId(entityId);

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        public IEntity Get(Guid entityGuid) => App./*Cache*/Data.List.One(entityGuid);

        public IEnumerable<IEntity> Get(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: App.AppId, upstream: App./*Cache*/Data, configLookUp: App.Data.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.List;
        }

        #endregion


    }
}
