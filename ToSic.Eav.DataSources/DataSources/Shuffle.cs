using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
    [PublicApi]
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.Shuffle, ToSic.Eav.DataSources",
        Type = DataSourceType.Sort, 
        DynamicOut = false, 
        ExpectsDataOfType = "38e7822b-1049-4539-bb3f-f99949b1b1d1",
        Icon = "shuffle",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Shuffle")]
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
            Provide(GetList);
		    ConfigMask(TakeKey, "[Settings:Take||0]");
        }


        private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

	        Log.Add($"will shuffle and take:{Take}");
            return ShuffleInternal(In["Default"].List, Take, Log);
	    }

        #region Experiment based on http://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
        static readonly Random Generator = new Random();

        private static IEnumerable<T> ShuffleInternal<T>(IEnumerable<T> sequence, int take, ILog log)
        {
            var wrapLog = log.Call("ShuffleInternal");
            var retArray = sequence.ToArray();
            
            // check if there is actually any data
            if (!retArray.Any())
            {
                wrapLog("0 items found to shuffle");
                return retArray;
            }

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

            wrapLog((maxTake).ToString());
            return retArray.Take(maxTake);
        }
        #endregion

    }
}