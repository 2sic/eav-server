using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that removes duplicate items in a Stream. Often used after a StreamMerge.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Filter duplicates",
        UiHint = "Remove items which occur multiple times",
        Icon = Icons.Filter1,
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.ItemFilterDuplicates, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamName },
	    HelpLink = "https://r.2sxc.org/DsFilterDuplicates")]

    public sealed class ItemFilterDuplicates: DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.StMrge";

        #endregion

	    internal const string DuplicatesStreamName = "Duplicates";


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public ItemFilterDuplicates()
		{
            Provide(GetUnique);
            Provide(DuplicatesStreamName, GetDuplicates);
		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetUnique()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();

            if (!In.HasStreamWithItems(Constants.DefaultStreamName)) return ImmutableArray<IEntity>.Empty;

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");

            var result = originals
                .Distinct()
                .ToImmutableArray();
            
            return wrapLog.ReturnAsOk(result);
        }


        /// <summary>
        /// Find and return only the duplicate items in the list
        /// </summary>
        /// <returns></returns>
	    private IImmutableList<IEntity> GetDuplicates()
	    {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();
	        if (!In.HasStreamWithItems(Constants.DefaultStreamName)) return ImmutableArray<IEntity>.Empty;

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");
            
            var result = originals
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToImmutableArray();
            
            return wrapLog.ReturnAsOk(result);
        }
    }
}