using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources",
        Type = DataSourceType.Logic, 
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.Paging",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Paging")]

    public sealed class Paging: DataSourceBase
	{
        #region Configuration-properties (no config)
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.Page";

        private const string PageSizeKey = "PageSize";
	    private const int DefPageSize = 10;
        private const string PageNumberKey = "PageNumber";
	    private const int DefPageNum = 1;

        /// <summary>
        /// The Page size in the paging. Defaults to 10.
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
        /// The Page number to show - defaults to 1
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
        [PrivateApi]
	    public string ReturnedStreamName { get; private set; }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public Paging()
		{
            Provide(GetList);
            Provide("Paging", GetPaging);
		    ConfigMask(PageSizeKey, "[Settings:" + PageSizeKey + "||" + DefPageSize + "]");
		    ConfigMask(PageNumberKey, "[Settings:" + PageNumberKey + "||" + DefPageNum + "]");
		}


	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();
		    var itemsToSkip = (PageNumber - 1)*PageSize;

	        var result = In["Default"].List.Skip(itemsToSkip).Take(PageSize).ToList();
	        Log.Add($"get page:{PageNumber} with size{PageSize} found:{result.Count}");
            return result;
	    }

        private IEnumerable<IEntity> GetPaging()
        {
            EnsureConfigurationIsLoaded();

            // Calculate any additional stuff
            var itemCount = In["Default"].List.Count();
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

            var entity = new Data.Entity(Constants.TransientAppId, 0, ContentTypeBuilder.Fake("Paging"), paging, "Title");

            // Assemble list of this for the stream
            var list = new List<IEntity> {entity};
            return list;
        }

	}
}