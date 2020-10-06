using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caching.CacheInfo;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    public partial class App
    {
        #region Dynamic Out

        private readonly StreamDictionary _out;
        protected bool RequiresRebuildOfOut = true;
        
        /// <inheritdoc/>
        public override IDictionary<string, IDataStream> Out
        {
            get
            {
                Configuration.Parse();
                if (!RequiresRebuildOfOut) return _out;

                // if the rebuilt is required because the app or zone are not default, then attach it first
                if (AppSwitch != 0 || ZoneSwitch != 0)
                    AttachOtherDataSource();

                // now create all streams
                CreateAppOutWithAllStreams();
                RequiresRebuildOfOut = false;
                return _out;
            }
        }
        #endregion

        /// <summary>
        /// Create a stream for each data-type
        /// </summary>
        private void CreateAppOutWithAllStreams()
        {
            var wrapLog = Log.Call();
            IDataStream upstream;
            try
            {
                // auto-attach to cache of current system?
                if (!In.ContainsKey(Constants.DefaultStreamName))
                    AttachOtherDataSource();
                upstream = In[Constants.DefaultStreamName];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Trouble with the App DataSource - must have a Default In-Stream with name " + Constants.DefaultStreamName + ". It has " + In.Count + " In-Streams.");
            }

            var upstreamDataSource = upstream.Source;
            _out.Clear();
            _out.Add(Constants.DefaultStreamName, upstreamDataSource.Out[Constants.DefaultStreamName]);

            // now provide all data streams for all data types; only need the cache for the content-types list, don't use it as the source...
            // because the "real" source already applies filters like published
            var listOfTypes = Apps.State.Get(this).ContentTypes;
            var dataSourceFactory = new DataSource(Log);
            var showDrafts = GetShowDraftStatus();
            var typeList = "";
            foreach (var contentType in listOfTypes)
            {
                var typeName = contentType.Name;
                if (typeName == Constants.DefaultStreamName || typeName.StartsWith("@") || _out.ContainsKey(typeName))
                    continue;
                typeList += typeName + ",";

                var deferredStream = new DataStreamWithCustomCaching(
                    () => new CacheInfoAppAndMore("AppTypeStream" + AppRootCacheKey.AppCacheKey(this), Apps.State.Get(this), $"Name={typeName}&Drafts={showDrafts}"),
                    this,
                    typeName,
                    () => BuildTypeStream(dataSourceFactory, upstreamDataSource, typeName)[Constants.DefaultStreamName].List,
                    true);
                _out.Add(typeName, deferredStream);
            }

            Log.Add($"Added with drafts:{showDrafts} streams: {typeList}");

            wrapLog(null);
        }

        /// <summary>
        /// Ask the current configuration system if the current user should see drafts
        /// </summary>
        /// <returns></returns>
        private bool GetShowDraftStatus()
        {
            var lookupShowDrafts = Configuration.Parse(new Dictionary<string, string>
            {
                {
                    QueryConstants.ParamsShowDraftKey,
                    $"[{QueryConstants.ParamsLookup}:{QueryConstants.ParamsShowDraftKey}||[{LookUpConstants.InstanceContext}:{QueryConstants.ParamsShowDraftKey}||false]]"
                }
            });
            if (!bool.TryParse(lookupShowDrafts.First().Value, out var showDrafts)) showDrafts = false;
            return showDrafts;
        }

        /// <summary>
		/// Build an EntityTypeFilter for this content-type to provide as a stream
		/// </summary>
        private EntityTypeFilter BuildTypeStream(DataSource dataSourceFactory, IDataSource upstreamDataSource, string typeName)
        {
            var wrapLog = Log.Call<EntityTypeFilter>($"..., ..., {typeName}");
            var ds = dataSourceFactory.GetDataSource<EntityTypeFilter>(this, upstreamDataSource,
                Configuration.LookUps);
            ds.TypeName = typeName;
            ds.Guid = Guid; // tell the inner source that it has the same ID as this one, as we're pretending it's the same source
            return wrapLog("ok", ds);
        }
    }
}
