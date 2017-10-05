using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
            Configuration.Add(TakeKey, "[Settings:Take||0]");

            CacheRelevantConfigurations = new[] { TakeKey };
        }


        private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

	        Log.Add($"will shuffle and take:{Take}");
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

    }
}