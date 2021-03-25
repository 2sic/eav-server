using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Shuffle / Randomize the order of items in a Stream.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "Shuffle",
        UiHint = "Mix/randomize the order of items",
        Icon = "shuffle",
        Type = DataSourceType.Sort, 
        GlobalName = "ToSic.Eav.DataSources.Shuffle, ToSic.Eav.DataSources",
        DynamicOut = false, 
        ExpectsDataOfType = "38e7822b-1049-4539-bb3f-f99949b1b1d1",
        HelpLink = "https://r.2sxc.org/DsShuffle")]
	public sealed class Shuffle: DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.Shuffl";

        private const string TakeKey = "Take";

        /// <summary>
        /// Amount of items to take / return when shuffling. Defaults to 0.
        /// </summary>
		public int Take
        {
            get
            {
                int.TryParse(Configuration[TakeKey], out var tk);
                return tk;
            }
            set => Configuration[TakeKey] = value.ToString();
        }


        #endregion
        [PrivateApi] 
        private static readonly bool DebugShuffleDs = false;

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
        public Shuffle()
		{
            Provide(GetShuffle);
		    ConfigMask(TakeKey, "[Settings:Take||0]");
        }


        private IImmutableList<IEntity> GetShuffle()
	    {
            Configuration.Parse();
            Log.Add($"will shuffle and take:{Take}");
            return ShuffleInternal(In[Constants.DefaultStreamName].Immutable, Take, Log);
	    }

        #region Shuffle based on http://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
        static readonly Random Generator = new Random();

        private static IImmutableList<T> ShuffleInternal<T>(IImmutableList<T> sequence, int take, ILog log)
        {
            var wrapLog = log.Call<IImmutableList<T>>();
            
            // check if there is actually any data
            if (!sequence.Any())
                return wrapLog("0 items found to shuffle", sequence);

            var retArray = sequence.ToArray();
            var maxIndex = retArray.Length; // not Length -1, as the random-generator will always be below this
            var maxTake = maxIndex;// retArray.Length;

            if (take > 0 && maxTake > take) maxTake = take;

            log.Add($"take:{take}, maxTake:{maxTake}");
            if (take > 0 && maxTake > take) maxTake = take;
            // changed this a bit because of #1815
            // var realTake = maxTake - 1; // either length of array, or take, but -1 as zero-based
            // go through array, shuffling the items - but only for as many times as we want to take
            for (var i = 0; i < maxTake; i++)
            {
                var swapIndex = Generator.Next(i, maxIndex);   // get num between index and max
                if (DebugShuffleDs)
                    log.Add($"i:{i}, maxI:{maxIndex}, maxT:{maxTake} swap:{swapIndex} - will put {swapIndex} on {i}");
                var temp = retArray[i];                 // get item at index i...
                retArray[i] = retArray[swapIndex];      // set index i to new item
                retArray[swapIndex] = temp;             // place temp-item to swap-slot
            }

            var result = retArray
                .Take(maxTake)
                .ToImmutableArray(); // .ToList();
            return wrapLog(maxTake.ToString(), result);
        }
        #endregion

    }
}