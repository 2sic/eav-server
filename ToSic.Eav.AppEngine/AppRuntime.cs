using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps
{
    public class AppRuntime : AppBase
    {
        #region simple properties and constructor
        public AppRuntime(int zoneId, int appId) : base(zoneId, appId) { }

        public AppRuntime(IApp app) : base(app) { }

        // Special constructor, should be used with care as there is no Zone!
        public AppRuntime(int appId) :base (appId) { }
        #endregion

        public IDataSource Data => _data ?? (_data = DataSource.GetInitialDataSource(ZoneId, AppId));

        private IDataSource _data;

        public IEnumerable<IContentType> ContentTypes => Cache.GetContentTypes().Select(c => c.Value);

        public IEnumerable<IContentType> GetContentTypes(string scope = null, bool includeAttributeTypes = false)
        {
            //var contentTypes = Cache.GetContentTypes();
            var set = ContentTypes // contentTypes.Select(c => c.Value)
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }

        public IEnumerable<IEntity> GetEntities(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: AppId, upstream: Cache, valueCollectionProvider: Data.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.LightList;
        }

        public IEnumerable<IEntity> GetInputTypes(bool includeGlobalDefinitions)
        {
            var inputsOfThisApp = GetEntities(Constants.TypeForInputTypeDefinition).ToList();

            if (includeGlobalDefinitions)
            {
                var systemDef = new AppRuntime(Constants.MetaDataAppId);
                var systemInputTypes = systemDef.GetInputTypes(false).ToList();

                systemInputTypes.ForEach(sit => {
                    if (inputsOfThisApp.FirstOrDefault(ait => ait.Title == sit.Title) == null)
                        inputsOfThisApp.Add(sit);
                });

            }
            return inputsOfThisApp;
        }

    }
}
