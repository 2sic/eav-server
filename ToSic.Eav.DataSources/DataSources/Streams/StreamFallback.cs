using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that returns the first stream which has content
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Stream Fallback",
        Icon = "call_merge",
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.StreamFallback, ToSic.Eav.DataSources",
        DynamicOut = false, 
	    HelpLink = "https://r.2sxc.org/DsStreamFallback")]

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
			Provide(GetStreamFallback);
		}

        private IImmutableList<IEntity> GetStreamFallback()
        {
            var foundStream = FindIdealFallbackStream();
            return foundStream?.Immutable ?? ImmutableArray<IEntity>.Empty;
        }

	    private IDataStream FindIdealFallbackStream()
	    {
            Configuration.Parse();

            // Check if there is a default-stream in with content - if yes, try to return that
            if (In.HasStreamWithItems(Constants.DefaultStreamName))
            {
	            Log.Add("found default, will return that");
	            return In[Constants.DefaultStreamName];
	        }

	        // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
	        var streamList = In.Where(x => x.Key != Constants.DefaultStreamName).OrderBy(x => x.Key);
	        foreach (var stream in streamList)
	            if (stream.Value.Immutable.Any())
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