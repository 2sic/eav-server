using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that merges two streams
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.ItemFilterDuplicates, ToSic.Eav.DataSources",
        Type = DataSourceType.Logic, 
        DynamicOut = false, 
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ItemFilterDuplicates")]

    public sealed class ItemFilterDuplicates: BaseDataSource
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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetUnique));
			Out.Add(DuplicatesStreamName, new DataStream(this, Constants.DefaultStreamName, GetDuplicates));

		}

        /// <summary>
        /// Find and return the unique items in the list
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEntity> GetUnique()
        {
            if(!In.ContainsKey(Constants.DefaultStreamName) || In[Constants.DefaultStreamName].List == null)
                return new List<IEntity>();

            var list = In[Constants.DefaultStreamName].List;

            return list.Distinct();
        }


        /// <summary>
        /// Find and return only the duplicate items in the list
        /// </summary>
        /// <returns></returns>
	    private IEnumerable<IEntity> GetDuplicates()
	    {
	        if (!In.ContainsKey(Constants.DefaultStreamName) || In[Constants.DefaultStreamName].List == null)
	            return new List<IEntity>();

	        var list = In[Constants.DefaultStreamName].List;

	        return list.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key);
	    }
    }
}