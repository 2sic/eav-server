﻿using System.Collections.Generic;
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
		public StreamMerge(Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.StMrge")
		{
            Provide(GetList);
            Provide(DistinctStream, GetDistinct);
            Provide(AndStream, GetAnd);
            Provide(XorStream, GetXor);
		}

        private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();
            var streams = GetValidInStreams();

            var result = streams
                .SelectMany(stm => stm)
                .ToImmutableArray();

            // 2021-11-09 2dm - old code, believe the new SelectMany is more efficient
            //return streams
            //    .Aggregate(new List<IEntity>() as IEnumerable<IEntity>, (current, stream) => current.Concat(stream))
            //    .ToImmutableArray();

            return wrapLog.Return(result, result.Length.ToString());
        }

        private List<IEnumerable<IEntity>> GetValidInStreams()
        {
            if (_validInStreams != null) return _validInStreams;

            var wrapLog = Log.Fn<List<IEnumerable<IEntity>>>();
            
            _validInStreams = In
                .OrderBy(pair => pair.Key)
                .Where(v => v.Value?.List != null)
                .Select(v => v.Value.List)
                .ToList();

            return wrapLog.Return(_validInStreams, _validInStreams.Count.ToString());
        }
        
        private List<IEnumerable<IEntity>> _validInStreams;

        private ImmutableArray<IEntity> GetDistinct()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();
            var result = List.Distinct().ToImmutableArray();
            return wrapLog.Return(result, result.Length.ToString());
        }

        private ImmutableArray<IEntity> GetAnd()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();

            var streams = GetValidInStreams();
            var streamCount = streams.Count;
            var first = streams.FirstOrDefault();
            var firstList = first?.ToList(); // must be separate, because we 
            if (streamCount == 0 || firstList == null || !firstList.Any())
                return wrapLog.Return(ImmutableArray.Create<IEntity>(), "no real In");

            if (streamCount == 1) return wrapLog.Return(firstList.ToImmutableArray(), "Just 1 In");

            var others = streams
                .Skip(1)
                .ToList();

            var itemsInOthers = others.SelectMany(s => s).Distinct().ToList();

            var final = firstList
                .Where(e => itemsInOthers.Contains(e))
                .ToImmutableArray();

            return wrapLog.Return(final, final.Length.ToString());
        }

        private ImmutableArray<IEntity> GetXor()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();

            var result = List
                .GroupBy(e => e)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToImmutableArray();

            return wrapLog.Return(result, result.Length.ToString());
        }

    }
}