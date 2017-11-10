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
	[DataSourceProperties(Type = DataSourceType.Logic, DynamicOut = false,
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Paging")]

    public sealed class Paging: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.Page";

        private const string PageSizeKey = "PageSize";
	    private const int DefPageSize = 10;
        private const string PageNumberKey = "PageNumber";
	    private const int DefPageNum = 1;

        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageSize
        {
            get
            {
                var ps = int.Parse(Configuration[PageSizeKey]);
                return ps > 0 ? ps : DefPageSize;
            }
            set => Configuration[PageSizeKey] = value.ToString();
        }

        /// <summary>
        /// The attribute whoose value will be filtered
        /// </summary>
        public int PageNumber
        {
            get
            {
                var pn = int.Parse(Configuration[PageNumberKey]);
                return pn > 0 ? pn : DefPageNum;
            }
            set => Configuration[PageNumberKey] = value.ToString();
        }

		#endregion

        #region Debug-Properties

	    public string ReturnedStreamName { get; private set; }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
		public Paging()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
            Out.Add("Paging", new DataStream(this, "Paging", null, GetPaging));
            Configuration.Add(PageSizeKey, "[Settings:" + PageSizeKey + "||" + DefPageSize + "]");
            Configuration.Add(PageNumberKey, "[Settings:" + PageNumberKey + "||" + DefPageNum + "]");

            CacheRelevantConfigurations = new[] {PageSizeKey, PageNumberKey};
		}


	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();
		    var itemsToSkip = (PageNumber - 1)*PageSize;

	        var result = In["Default"].LightList.Skip(itemsToSkip).Take(PageSize).ToList();
	        Log.Add($"get page:{PageNumber} with size{PageSize} found:{result.Count}");
            return result;
	    }

        private IEnumerable<IEntity> GetPaging()
        {
            EnsureConfigurationIsLoaded();

            // Calculate any additional stuff
            var itemCount = In["Default"].LightList.Count();
            var pageCount = Math.Ceiling((decimal) itemCount / PageSize);

            // Assemble the entity
            var paging = new Dictionary<string, object>
            {
                {"Title", "Paging Information"},
                {"PageSize", PageSize},
                {"PageNumber", PageNumber},
                {"ItemCount", itemCount},
                {"PageCount", pageCount}
            };

            var entity = new Data.Entity(Constants.TransientAppId, 0, "Paging", paging, "Title");

            // Assemble list of this for the stream
            var list = new List<IEntity> {entity};
            return list;
        }

	}
}