using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;
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
        UiHint = "Find the first stream which has data",
        Icon = Icons.Merge,
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.StreamFallback, ToSic.Eav.DataSources",
        DynamicOut = false,
        DynamicIn = true,
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
            return foundStream?.List.ToImmutableArray() ?? ImmutableArray<IEntity>.Empty;
        }

	    private IDataStream FindIdealFallbackStream()
	    {
            var wrapLog = Log.Fn<IDataStream>();

            Configuration.Parse();

            // Check if there is a default-stream in with content - if yes, try to return that
            if (In.HasStreamWithItems(Constants.DefaultStreamName))
                return wrapLog.Return(In[Constants.DefaultStreamName], "found default");

            // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
	        var streamList = In
                .Where(x => x.Key != Constants.DefaultStreamName)
                .OrderBy(x => x.Key);
            
	        foreach (var stream in streamList)
	            if (stream.Value.List.Any())
	            {
	                ReturnedStreamName = stream.Key;
                    return wrapLog.Return(stream.Value, $"will return stream:{ReturnedStreamName}");
                }

	        return wrapLog.ReturnNull("didn't find any stream, will return empty");
	    }
	}
}