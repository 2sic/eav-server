using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Query;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that returns the first stream which has content
	/// </summary>
    [PublicApi]
	[Query.VisualQuery(GlobalName = "ToSic.Eav.DataSources.StreamFallback, ToSic.Eav.DataSources",
        Type = DataSourceType.Logic, 
        DynamicOut = false, 
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-StreamFallback")]

    public sealed class StreamFallback : DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.Fallbk";

        #endregion

        #region Debug-Properties
        [PrivateApi]
        public string ReturnedStreamName { get; private set; }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public StreamFallback()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
		}

        private IEnumerable<IEntity> GetList()
        {
            var foundStream = FindIdealFallbackStream();

            return foundStream != null ? foundStream.List : new List<IEntity>();
        }

	    private IDataStream FindIdealFallbackStream()
	    {
            EnsureConfigurationIsLoaded();

            // Check if there is a default-stream in with content - if yes, try to return that
	        if (In.ContainsKey(Constants.DefaultStreamName) && In[Constants.DefaultStreamName].List.Any())
	        {
	            Log.Add("found default, will return that");
	            return In[Constants.DefaultStreamName];
	        }

	        // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
	        var streamList = In.Where(x => x.Key != Constants.DefaultStreamName).OrderBy(x => x.Key);
	        foreach (var stream in streamList)
	            if (stream.Value.List.Any())
	            {
	                ReturnedStreamName = stream.Key;
	                Log.Add($"will return stream:{ReturnedStreamName}");
	                return stream.Value;
	            }

	        Log.Add("didn't find any stream, will return empty");
	        return null;
	    }
	}
}