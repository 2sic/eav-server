using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
    [DataSourceProperties(Type = DataSourceType.Sort, DynamicOut = false, EnableConfig = true, 
        ExpectsDataOfType = "38e7822b-1049-4539-bb3f-f99949b1b1d1",
        Icon = "shuffle",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Shuffle")]
	public sealed class Shuffle: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.Shuffl";

        private const string TakeKey = "Take";

        /// <summary>
        /// Amount of items to take 
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



        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        public Shuffle()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
            Configuration.Add(TakeKey, "[Settings:Take||0]");

            CacheRelevantConfigurations = new[] { TakeKey };
        }


        private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

	        Log.Add($"will shuffle and take:{Take}");
            return ShuffleInternal(In["Default"].List, Take);
	    }

        #region Experiment based on http://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
        static readonly Random Generator = new Random();

        private static IEnumerable<T> ShuffleInternal<T>(IEnumerable<T> sequence, int take)
        {
            var retArray = sequence.ToArray();
            var maxIndex = retArray.Length; // not Length -1, as the random-generator will always be below this
            var maxTake = retArray.Length;
            if (take > 0 && maxTake > take) maxTake = take;
            maxTake = maxTake - 1; // either length of array, or take, but -1 as zero-based
            // go through array, shuffling the items - but only for as many times as we want to take
            for (var i = 0; i < maxTake; i++)
            {
                var swapIndex = Generator.Next(i, maxIndex);   // get num between index and max
                var temp = retArray[i];                 // get item at index i...
                retArray[i] = retArray[swapIndex];      // set index i to new item
                retArray[swapIndex] = temp;             // place temp-item to swap-slot
            }

            return retArray.Take(maxTake + 1);
        }
        #endregion

    }
}