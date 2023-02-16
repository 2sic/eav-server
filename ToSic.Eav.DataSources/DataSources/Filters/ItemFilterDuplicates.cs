using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
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

    public sealed class ItemFilterDuplicates: DataSource
	{
	    internal const string DuplicatesStreamName = "Duplicates";


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public ItemFilterDuplicates(Dependencies services): base(services, $"{DataSourceConstants.LogPrefix}.Duplic")
        {
            Provide(GetUnique);
            Provide(DuplicatesStreamName, GetDuplicates);
		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetUnique() => Log.Func(() =>
        {
            if (!In.HasStreamWithItems(Constants.DefaultStreamName)) 
                return (ImmutableArray<IEntity>.Empty, "no in stream with name");

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = originals
                .Distinct()
                .ToImmutableArray();

            return (result, "ok");
        });


        /// <summary>
        /// Find and return only the duplicate items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetDuplicates() => Log.Func(() =>
        {
            if (!In.HasStreamWithItems(Constants.DefaultStreamName)) 
                return (ImmutableArray<IEntity>.Empty, "no in-stream with name");

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = originals
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToImmutableArray();

            return (result, "ok");
        });
    }
}