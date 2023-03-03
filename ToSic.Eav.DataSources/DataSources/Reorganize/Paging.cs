﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Factory;
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
        GlobalName = "ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.Paging",
        HelpLink = "https://r.2sxc.org/DsPaging")]

    public sealed class Paging: DataSource
	{
        private readonly IDataBuilder _pagingBuilder;

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
            set => Configuration.SetThis(value);
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
            set => Configuration.SetThis(value);
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
		public Paging(MyServices services, IDataBuilder dataBuilder): base(services, $"{DataSourceConstants.LogPrefix}.Paging")
        {
            ConnectServices(
                _pagingBuilder = dataBuilder.Configure(typeName: "Paging")
            );
            Provide(GetList);
            Provide("Paging", GetPaging);
		}


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();
            var itemsToSkip = (PageNumber - 1) * PageSize;

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = originals
                .Skip(itemsToSkip)
                .Take(PageSize)
                .ToImmutableArray();
            l.A($"get page:{PageNumber} with size{PageSize} found:{result.Length}");
            return (result, "ok");
        });

        private IImmutableList<IEntity> GetPaging() => Log.Func(() =>
        {
            Configuration.Parse();

            // Calculate any additional stuff
            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var itemCount = originals.Count;
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

            var entity = _pagingBuilder.Create(paging, id: PageNumber);

            // Assemble list of this for the stream
            var list = new List<IEntity> { entity };
            return (list.ToImmutableArray(), "ok");
        });

    }
}