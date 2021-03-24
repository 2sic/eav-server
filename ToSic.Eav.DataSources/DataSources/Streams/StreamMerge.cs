using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that merges all streams on the `In` into one `Out` stream
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Merge Stream",
        Icon = "merge_type",
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.StreamMerge, ToSic.Eav.DataSources",
        DynamicOut = false, 
	    HelpLink = "https://r.2sxc.org/DsStreamMerge")]

    public sealed class StreamMerge: DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.StMrge";

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public StreamMerge()
		{
            Provide(GetList);
		}

        private ImmutableArray<IEntity> GetList()
        {
            var streams = In
                .OrderBy(pair => pair.Key)
                .Where(v => v.Value?.Immutable != null)
                .Select(v => v.Value.Immutable);

            return streams
                .Aggregate(new List<IEntity>() as IEnumerable<IEntity>, (current, stream) => current.Concat(stream))
                .ToImmutableArray();
        }
	}
}