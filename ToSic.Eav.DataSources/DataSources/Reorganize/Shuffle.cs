﻿using ToSic.Eav.DataSource.Sys;
using static ToSic.Eav.DataSource.DataSourceConstants;


namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Shuffle / Randomize the order of items in a Stream.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Shuffle",
    UiHint = "Mix/randomize the order of items",
    Icon = DataSourceIcons.Shuffle,
    Type = DataSourceType.Sort, 
    NameId = "ToSic.Eav.DataSources.Shuffle, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [InStreamDefaultRequired],
    ConfigurationType = "38e7822b-1049-4539-bb3f-f99949b1b1d1",
    HelpLink = "https://go.2sxc.org/DsShuffle")]
public sealed class Shuffle: DataSourceBase
{
    #region Configuration-properties (no config)

    private const int DefaultTakeAll = 0;

    /// <summary>
    /// Amount of items to take / return when shuffling.
    /// Defaults to `0` meaning take-all.
    /// </summary>
    [Configuration(Fallback = DefaultTakeAll)]
    public int Take
    {
        get => Configuration.GetThis(DefaultTakeAll);
        set => Configuration.SetThisObsolete(value);
    }


    #endregion
    [PrivateApi] 
    private static readonly bool DebugShuffleDs = false;

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public Shuffle(Dependencies services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Shuffl")
    {
        ProvideOut(GetShuffle);
    }


    private IImmutableList<IEntity> GetShuffle()
    {
        var l = Log.Fn<IImmutableList<IEntity>>($"Take: {Take}");
        Configuration.Parse();

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        return l.ReturnAsOk(ShuffleInternal(source.ToList(), Take));
    }

    #region Shuffle based on http://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
    static readonly Random Generator = new();

    private IImmutableList<T> ShuffleInternal<T>(List<T> sequence, int take)
    {
        var l = Log.Fn<IImmutableList<T>>();
        // check if there is actually any data
        if (!sequence.Any())
            return l.Return(sequence.ToImmutableOpt(), "0 items found to shuffle");

        var retArray = sequence.ToArray();
        var maxIndex = retArray.Length; // not Length -1, as the random-generator will always be below this
        var maxTake = maxIndex;// retArray.Length;

        if (take > 0 && maxTake > take) maxTake = take;

        l.A($"take:{take}, maxTake:{maxTake}");
        if (take > DefaultTakeAll && maxTake > take) maxTake = take;
        // changed this a bit because of #1815
        // var realTake = maxTake - 1; // either length of array, or take, but -1 as zero-based
        // go through array, shuffling the items - but only for as many times as we want to take
        for (var i = 0; i < maxTake; i++)
        {
            var swapIndex = Generator.Next(i, maxIndex);   // get num between index and max
            if (DebugShuffleDs)
                l.A($"i:{i}, maxI:{maxIndex}, maxT:{maxTake} swap:{swapIndex} - will put {swapIndex} on {i}");
            var temp = retArray[i];                 // get item at index i...
            retArray[i] = retArray[swapIndex];      // set index i to new item
            retArray[swapIndex] = temp;             // place temp-item to swap-slot
        }

        var result = retArray
            .Take(maxTake)
            .ToImmutableOpt();
        return l.Return(result, maxTake.ToString());
    }
    #endregion

}