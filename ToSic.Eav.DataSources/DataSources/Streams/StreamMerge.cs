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
	/// A DataSource that merges all streams on the `In` into one `Out` stream
	/// </summary>
	/// <remarks>
	/// History
	/// * v12.10 added new Out streams `Distinct` removes duplicates, `And` keeps items which are in _all_ streams and `Xor` keeps items which are only in one stream
	/// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Merge Streams",
        UiHint = "Combine multiple streams into one",
        Icon = Icons.MergeLeft,
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.StreamMerge, ToSic.Eav.DataSources",
        DynamicOut = false,
        DynamicIn = true,
	    HelpLink = "https://r.2sxc.org/DsStreamMerge")]

    public sealed class StreamMerge: DataSource
	{
        /// <summary>
        /// Name of stream which offers only distinct items (filter duplicates)
        /// </summary>
        /// <remarks>
        /// New in v12.10
        /// </remarks>
        [PrivateApi] public const string DistinctStream = "Distinct";
        [PrivateApi] public const string AndStream = "And";
        [PrivateApi] public const string XorStream = "Xor";


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public StreamMerge(MyServices services) : base(services, $"{DataSourceConstants.LogPrefix}.StMrge")
		{
            ProvideOut(GetList);
            ProvideOut(GetDistinct, DistinctStream);
            ProvideOut(GetAnd, AndStream);
            ProvideOut(GetXor, XorStream);
		}

        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            var streams = GetValidInStreams();
            var result = streams
                .SelectMany(stm => stm)
                .ToImmutableList();

            return (result, result.Count.ToString());
        });

        private List<IEnumerable<IEntity>> GetValidInStreams() => Log.Func(() =>
        {
            if (_validInStreams != null) return (_validInStreams, "cached");

            _validInStreams = In
                .OrderBy(pair => pair.Key)
                .Where(v => v.Value?.List != null)
                .Select(v => v.Value.List)
                .ToList();

            return (_validInStreams, _validInStreams.Count.ToString());
        });
        
        private List<IEnumerable<IEntity>> _validInStreams;

        private IImmutableList<IEntity> GetDistinct() => Log.Func(() =>
        {
            var result = List.Distinct().ToImmutableList();
            return (result, result.Count.ToString());
        });

        private IImmutableList<IEntity> GetAnd() => Log.Func(() =>
        {
            var streams = GetValidInStreams();
            var streamCount = streams.Count;
            var first = streams.FirstOrDefault();
            var firstList = first?.ToList(); // must be separate, because we 
            if (streamCount == 0 || firstList == null || !firstList.Any())
                return (ImmutableList<IEntity>.Empty, "no real In");

            if (streamCount == 1) return (firstList.ToImmutableList(), "Just 1 In");

            var others = streams
                .Skip(1)
                .ToList();

            var itemsInOthers = others.SelectMany(s => s).Distinct().ToList();

            var final = firstList
                .Where(e => itemsInOthers.Contains(e))
                .ToImmutableList();

            return (final, final.Count.ToString());
        });

        private IImmutableList<IEntity> GetXor() => Log.Func(() =>
        {
            var result = List
                .GroupBy(e => e)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToImmutableList();

            return (result, result.Count.ToString());
        });

    }
}