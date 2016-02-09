using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
	public class Paging: BaseDataSource
	{
		#region Configuration-properties (no config)
        private const string PageSizeKey = "PageSize";
        private const string PageNumberKey = "PageNumber";

        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageSize
        {
            get { return int.Parse(Configuration[PageSizeKey]); }
            set { Configuration[PageSizeKey] = value.ToString(); }
        }

        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageNumber
        {
            get { return int.Parse(Configuration[PageNumberKey]); }
            set { Configuration[PageNumberKey] = value.ToString(); }
        }

		#endregion

        #region Debug-Properties

	    public string ReturnedStreamName { get; private set; }
        #endregion


        /// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public Paging()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
            Out.Add("Paging", new DataStream(this, "Paging", null, GetPaging));
            Configuration.Add(PageSizeKey, "[Settings:" + PageSizeKey + "||10]");
            Configuration.Add(PageNumberKey, "[Settings:" + PageNumberKey + "||1]");

            CacheRelevantConfigurations = new[] {PageSizeKey, PageNumberKey};
		}


	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

		    var itemsToSkip = (PageNumber - 1)*PageSize;

	        return In["Default"].LightList.Skip(itemsToSkip).Take(PageSize).ToList();
	    }

        private IEnumerable<IEntity> GetPaging()
        {
            EnsureConfigurationIsLoaded();

            // Calculate any additional stuff
            var itemCount = In["Default"].LightList.Count();
            var pageCount = Math.Ceiling((decimal) itemCount / PageSize);

            // Assemble the entity
            var paging = new Dictionary<string, object>();
            paging.Add("Title", "Paging Information");
            paging.Add("PageSize", PageSize);
            paging.Add("PageNumber", PageNumber);
            paging.Add("ItemCount", itemCount);
            paging.Add("PageCount", pageCount);

            var entity = new Data.Entity(0, "Paging", paging, "Title");

            // Assemble list of this for the stream
            var list = new List<IEntity>();
            list.Add(entity);
            return list;
        }

	}
}