using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that merges two streams
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.ItemFilterDuplicates, ToSic.Eav.DataSources",
        NiceName = "Filter duplicates",
        Icon = "filter_1",
        Type = DataSourceType.Logic, 
        DynamicOut = false, 
	    HelpLink = "https://r.2sxc.org/DsFilterDuplicates")]

    public sealed class ItemFilterDuplicates: DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.StMrge";

        #endregion

	    internal const string DuplicatesStreamName = "Duplicates";


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public ItemFilterDuplicates()
		{
            Provide(GetUnique);
            Provide(DuplicatesStreamName, GetDuplicates);
		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private ImmutableArray<IEntity> GetUnique()
        {
            if(!In.HasStreamWithItems(Constants.DefaultStreamName)) return ImmutableArray<IEntity>.Empty;

            var list = In[Constants.DefaultStreamName].Immutable;

            return list
                .Distinct()
                .ToImmutableArray();
        }


        /// <summary>
        /// Find and return only the duplicate items in the list
        /// </summary>
        /// <returns></returns>
	    private ImmutableArray<IEntity> GetDuplicates()
	    {
	        if (!In.HasStreamWithItems(Constants.DefaultStreamName)) return ImmutableArray<IEntity>.Empty;

            var list = In[Constants.DefaultStreamName].Immutable;

	        return list
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToImmutableArray();
	    }
    }
}