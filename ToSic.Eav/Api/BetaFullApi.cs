using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Api
{
    /// <summary>
    /// This should become the single point of contact with every layer working with the EAV
    /// 
    /// The core reason I'm creating it is because the BLL contains a mix of 
    /// - data (entity framework) access and 
    /// - cache (eav) access
    /// 
    /// and this layer should only be living on the cache - or cleanly forward to a layer above
    /// 
    /// It doesn't have a clear concept yet, so it's called Beta... to ensure people know it's still work in progress
    /// Note that it should itself NEVER access the DB but always go through the BLL layer
    /// </summary>
    public class BetaFullApi
    {
        #region Simple Properties
        public int AppId { get; internal set; }
        public int ZoneId { get; internal set; }

        public ICache Cache { get; internal set; }

        public IDataSource AppData { get; internal set; }

        private IMetaDataSource _metadata;
        public IMetaDataSource Metadata => _metadata ?? (_metadata = DataSource.GetMetaDataSource(ZoneId, AppId));

        private BetaFullApi _systemApp;
        public BetaFullApi SystemApp => _systemApp ?? (_systemApp = new BetaFullApi(Constants.DefaultZoneId, Constants.MetaDataAppId));

        private EavDataController _context;

        private EavDataController Context
        {
            get
            {
                if(_context == null )
                    throw new Exception("Context not set");
                return _context;
            }
            set { _context = value; }
        }

        #endregion

        public BetaFullApi(int? zoneId, int? appId, EavDataController cntxt = null)
        {
            // get cache, and automatically use this to look up the non-null appid / zoneid
            AppData = DataSource.GetInitialDataSource(zoneId, appId);
            Cache = AppData.Cache;// DataSource.GetCache(zoneId, appId);
            AppId = Cache.AppId;
            ZoneId = Cache.ZoneId;
            Context = cntxt;
        }


        public void Metadata_AddOrUpdate(int targetType, int targetId, string metadataTypeName,
            Dictionary<string, object> values)
        {
            // experimental - not in use yet - try to find the attribute...
            //var generalSettings = Metadata.GetAssignedEntities(Constants.AssignmentObjectTypeIdFieldProperties, targetId, metadataTypeName);
            //if (generalSettings != null)
            //{
            //    if (generalSettings.Count() == 1)
            //        Context.Entities.UpdateEntity(generalSettings.First().EntityId, values);
            //    else
            //        throw new Exception("Tried to update metadata, but found more than 1 item which could be updated; aborting");
            //}
            //else
            //{
            //    var contentType = DataSource.GetCache(Context.ZoneId, Context.AppId).GetContentType("@All");
            //    var entity = Context.Entities.AddEntity(contentType.AttributeSetId, values, null, null);

            //}

        }


        public IEnumerable<IEntity> GetEntitiesOfType(string contentTypeName)
        {
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: AppId, upstream: Cache, valueCollectionProvider: AppData.ConfigurationProvider); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.LightList;
        }

        public IEnumerable<IEntity> GetInputTypes(bool includeGlobalDefinitions)
        {
            var inputsOfThisApp = GetEntitiesOfType(Constants.TypeForInputTypeDefinition).ToList();

            if (includeGlobalDefinitions)
            {
                var systemDef = new BetaFullApi(null, Constants.MetaDataAppId, Context);
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
