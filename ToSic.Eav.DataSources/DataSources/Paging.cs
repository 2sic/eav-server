using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Do Paging to only return a limited amount of results + show how many such pages exist and which Page we are on.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Paging",
        UiHint = "Split data into pages and forward just one batch",
        Icon = Icons.Stories,
        Type = DataSourceType.Logic, 
        GlobalName = "ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.Paging",
        HelpLink = "https://r.2sxc.org/DsPaging")]

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


	    private IImmutableList<IEntity> GetList()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();
            var itemsToSkip = (PageNumber - 1)*PageSize;

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");


            var result = originals
                .Skip(itemsToSkip)
                .Take(PageSize)
                .ToImmutableArray();
            Log.A($"get page:{PageNumber} with size{PageSize} found:{result.Length}");
            return wrapLog.ReturnAsOk(result);
	    }
        
        private IImmutableList<IEntity> GetPaging()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Calculate any additional stuff
            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");

            var itemCount = originals.Count;
            var pageCount = Math.Ceiling((decimal) itemCount / PageSize);

            // Assemble the entity
            var paging = new Dictionary<string, object>
            {
                {Attributes.TitleNiceName, "Paging Information"},
                {"PageSize", PageSize},
                {"PageNumber", PageNumber},
                {"ItemCount", itemCount},
                {"PageCount", pageCount}
            };

            var entity = new Data.Entity(Constants.TransientAppId, 0, DataBuilder.Type("Paging"), paging, Attributes.TitleNiceName);

            // Assemble list of this for the stream
            var list = new List<IEntity> {entity};
            return wrapLog.ReturnAsOk(list.ToImmutableArray());
        }

	}
}