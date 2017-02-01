using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	// [PipelineDesigner]
	public sealed class Shuffle: BaseDataSource
	{
        #region Configuration-properties (no config)
        private const string TakeKey = "Take";

        /// <summary>
        /// Amount of items to take - NOT IMPLEMENTED YET
        /// </summary>
		public int Take
        {
            get
            {
                int tk;
                int.TryParse(Configuration[TakeKey], out tk);
                return tk;
            }
            set { Configuration[TakeKey] = value.ToString(); }
        }


        #endregion



        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        public Shuffle()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
            Configuration.Add(TakeKey, "[Settings:Take||0]");

            CacheRelevantConfigurations = new[] { TakeKey };
        }


        private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

            return ShuffleInternal(In["Default"].LightList, Take);

	        //   return In["Default"].LightList.Skip(itemsToSkip).Take(PageSize).ToList();
	    }

        #region Experiment based on http://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
        static readonly Random Generator = new Random();

        private static IEnumerable<T> ShuffleInternal<T>(IEnumerable<T> sequence, int take)
        {
            var retArray = sequence.ToArray();

            // go through array, starting at the last-index
            for (var i = retArray.Length - 1; i > take; i--)
            {
                var swapIndex = Generator.Next(0, i);   // get num between 0 and index
                if (swapIndex == i) continue;           // don't replace with itself
                var temp = retArray[i];                 // get item at index i...
                retArray[i] = retArray[swapIndex];      // set index i to new item
                retArray[swapIndex] = temp;             // place temp-item to swap-slot
            }

            return retArray;
        }
        #endregion

        #region Experiment based on https://gist.github.com/mikedugan/8249637
        public static void ShuffleMethod<T>(T[] array)
        {
            var random = new Random();
            for (int i = array.Length; i > 1; i--)
            {
                // Pick random element to swap.
                int j = random.Next(i); // 0 <= j <= i-1
                                        // Swap.
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
        }
        #endregion
    }
}