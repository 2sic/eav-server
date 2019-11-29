using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that merges two streams
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.StreamMerge, ToSic.Eav.DataSources",
        Type = DataSourceType.Logic, 
        DynamicOut = false, 
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-StreamMerge")]

    public sealed class StreamMerge: BaseDataSource
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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
		}

        private IEnumerable<IEntity> GetList()
        {
            var streams = In.OrderBy(pair => pair.Key).Where(v => v.Value?.List != null).Select(v => v.Value.List);

            return streams.Aggregate(new List<IEntity>() as IEnumerable<IEntity>, (current, strm) => current.Concat(strm));
        }
	}
}