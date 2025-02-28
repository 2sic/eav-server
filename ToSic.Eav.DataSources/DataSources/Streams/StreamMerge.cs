using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that merges all streams on the `In` into one `Out` stream
/// </summary>
/// <remarks>
/// History
/// * v12.10 added new Out streams `Distinct` removes duplicates, `And` keeps items which are in _all_ streams and `Xor` keeps items which are only in one stream
/// </remarks>
[PublicApi]
[VisualQuery(
    NiceName = "Merge Streams",
    UiHint = "Combine multiple streams into one",
    Icon = DataSourceIcons.MergeLeft,
    Type = DataSourceType.Logic, 
    NameId = "ToSic.Eav.DataSources.StreamMerge, ToSic.Eav.DataSources",
    DynamicOut = false,
    DynamicIn = true,
    HelpLink = "https://go.2sxc.org/DsStreamMerge")]

public sealed class StreamMerge: DataSourceBase
{
    /// <summary>
    /// Name of stream which offers only distinct items (filter duplicates)
    /// </summary>
    /// <remarks>
    /// New in v12.10
    /// </remarks>
    [PrivateApi] internal const string DistinctStream = "Distinct";
    [PrivateApi] internal const string AndStream = "And";
    [PrivateApi] internal const string XorStream = "Xor";


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public StreamMerge(MyServices services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.StMrge")
    {
        ProvideOut(GetAll);
        ProvideOut(GetDistinct, DistinctStream);
        ProvideOut(GetAnd, AndStream);
        ProvideOut(GetXor, XorStream);
    }

    private IImmutableList<IEntity> GetAll()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var result = ValidInStreams
            .SelectMany(stm => stm)
            .ToImmutableList();

        return l.Return(result, result.Count.ToString());
    }

    private List<IEnumerable<IEntity>> ValidInStreams => field ??= GetValidInStreams();

    private List<IEnumerable<IEntity>> GetValidInStreams()
    {
        var l = Log.Fn<List<IEnumerable<IEntity>>>();

        var inStreams = In
            .OrderBy(pair => pair.Key)
            .Where(v => v.Value?.List != null)
            .Select(v => v.Value.List)
            .ToList();

        return l.Return(inStreams, inStreams.Count.ToString());
    }

    private IImmutableList<IEntity> GetDistinct()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var result = List.Distinct().ToImmutableList();
        return l.Return(result, result.Count.ToString());
    }

    private IImmutableList<IEntity> GetAnd()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var streams = ValidInStreams;
        var streamCount = streams.Count;
        var firstList = streams.FirstOrDefault()?.ToList(); // must be separate, because we

        // if there are no streams, or the first stream is empty, produce empty list
        if (streamCount == 0 || firstList == null || !firstList.Any())
            return l.Return(ImmutableList<IEntity>.Empty, "no real In");

        // if there is only 1 stream, return it
        if (streamCount == 1)
            return l.Return(firstList.ToImmutableList(), "Just 1 In");

        var others = streams
            .Skip(1)
            .ToList();

        var itemsInOthers = others
            .SelectMany(s => s)
            .Distinct()
            .ToList();

        var final = firstList
            .Where(e => itemsInOthers.Contains(e))
            .ToImmutableList();

        return l.Return(final, final.Count.ToString());
    }

    private IImmutableList<IEntity> GetXor()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var result = List
            .GroupBy(e => e)
            .Where(g => g.Count() == 1)
            .Select(g => g.First())
            .ToImmutableList();

        return l.Return(result, result.Count.ToString());
    }

}