using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSources.DataSourceConstants;
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

    public sealed class StreamFallback : DataSource
	{

        #region Debug-Properties
        [PrivateApi]
        public string ReturnedStreamName { get; private set; }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public StreamFallback(MyServices services) : base(services, $"{LogPrefix}.FallBk")
		{
			Provide(GetStreamFallback);
		}

        private IImmutableList<IEntity> GetStreamFallback()
        {
            var foundStream = FindIdealFallbackStream();
            return foundStream?.List.ToImmutableList() ?? EmptyList;
        }

        private IDataStream FindIdealFallbackStream() => Log.Func(() =>
        {
            Configuration.Parse();

            // Check if there is a default-stream in with content - if yes, try to return that
            if (In.HasStreamWithItems(StreamDefaultName))
                return (In[StreamDefaultName], "found default");

            // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
            var streamList = In
                .Where(x => x.Key != StreamDefaultName)
                .OrderBy(x => x.Key);

            foreach (var stream in streamList)
                if (stream.Value.List.Any())
                {
                    ReturnedStreamName = stream.Key;
                    return (stream.Value, $"will return stream:{ReturnedStreamName}");
                }

            return (null, "didn't find any stream, will return empty");
        });
    }
}