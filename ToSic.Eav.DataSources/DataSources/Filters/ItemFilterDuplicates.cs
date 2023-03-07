using System.Collections.Generic;
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
		public ItemFilterDuplicates(MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.Duplic")
        {
            Provide(GetUnique);
            Provide(GetDuplicates, DuplicatesStreamName);
		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetUnique() => Log.Func(() =>
        {
            if (!In.HasStreamWithItems(Constants.DefaultStreamName)) 
                return (EmptyList, "no in stream with name");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(this), "error");

            var result = source
                .Distinct()
                .ToImmutableList();

            return (result, "ok");
        });


        /// <summary>
        /// Find and return only the duplicate items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetDuplicates() => Log.Func(() =>
        {
            if (!In.HasStreamWithItems(Constants.DefaultStreamName)) 
                return (EmptyList, "no in-stream with name");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(this), "error");

            var result = source
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToImmutableList();

            return (result, "ok");
        });
    }
}