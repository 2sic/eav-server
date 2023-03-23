using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
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
        NameId = "ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { QueryConstants.InStreamDefaultRequired },
	    ConfigurationType = "|Config ToSic.Eav.DataSources.Paging",
        HelpLink = "https://r.2sxc.org/DsPaging")]

    public sealed class Paging: DataSource
	{
        private readonly IDataFactory _pagingFactory;

        #region Configuration-properties (no config)

	    private const int DefPageSize = 10;
	    private const int DefPageNum = 1;

        /// <summary>
        /// The Page size in the paging. Defaults to 10.
        /// </summary>
        [Configuration(Fallback = DefPageSize)]
        public int PageSize
        {
            get
            {
                var ps = Configuration.GetThis(DefPageSize);
                return ps > 0 ? ps : DefPageSize;
            }
            set => Configuration.SetThisObsolete(value);
        }

        /// <summary>
        /// The Page number to show - defaults to 1
        /// </summary>
        [Configuration(Fallback = DefPageNum)]
        public int PageNumber
        {
            get
            {
                var pn = Configuration.GetThis(DefPageNum);
                return pn > 0 ? pn : DefPageNum;
            }
            set => Configuration.SetThisObsolete(value);
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
		public Paging(MyServices services, IDataFactory dataFactory): base(services, $"{DataSourceConstants.LogPrefix}.Paging")
        {
            ConnectServices(
                _pagingFactory = dataFactory.New(options: new DataFactoryOptions(typeName: "Paging"))
            );
            ProvideOut(GetList);
            ProvideOut(GetPaging, "Paging");
		}


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();
            var itemsToSkip = (PageNumber - 1) * PageSize;

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            var result = source
                .Skip(itemsToSkip)
                .Take(PageSize)
                .ToImmutableList();
            l.A($"get page:{PageNumber} with size{PageSize} found:{result.Count}");
            return (result, "ok");
        });

        private IImmutableList<IEntity> GetPaging() => Log.Func(() =>
        {
            Configuration.Parse();

            // Calculate any additional stuff
            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            var itemCount = source.Count;
            var pageCount = Math.Ceiling((decimal)itemCount / PageSize);

            // Assemble the entity
            var paging = new Dictionary<string, object>
            {
                { Attributes.TitleNiceName, "Paging Information" },
                { nameof(PageSize), PageSize },
                { nameof(PageNumber), PageNumber },
                { "ItemCount", itemCount },
                { "PageCount", pageCount }
            };

            var entity = _pagingFactory.Create(paging, id: PageNumber);

            // Assemble list of this for the stream
            var list = new List<IEntity> { entity };
            return (list.ToImmutableList(), "ok");
        });

    }
}