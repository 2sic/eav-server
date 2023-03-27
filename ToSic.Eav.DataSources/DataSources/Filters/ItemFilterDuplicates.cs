using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
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
        NameId = "ToSic.Eav.DataSources.ItemFilterDuplicates, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { StreamDefaultName },
	    HelpLink = "https://r.2sxc.org/DsFilterDuplicates")]

    public sealed class ItemFilterDuplicates: Eav.DataSource.DataSourceBase
	{
	    internal const string DuplicatesStreamName = "Duplicates";


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public ItemFilterDuplicates(MyServices services): base(services, $"{LogPrefix}.Duplic")
        {
            ProvideOut(GetUnique);
            ProvideOut(GetDuplicates, DuplicatesStreamName);
		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetUnique() => Log.Func(() =>
        {
            if (!In.HasStreamWithItems(StreamDefaultName)) 
                return (EmptyList, "no in stream with name");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

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
            if (!In.HasStreamWithItems(StreamDefaultName)) 
                return (EmptyList, "no in-stream with name");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            var result = source
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToImmutableList();

            return (result, "ok");
        });
    }
}