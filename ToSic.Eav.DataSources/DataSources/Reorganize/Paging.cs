using ToSic.Eav.Data.Build;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Do Paging to only return a limited amount of results + show how many such pages exist and which Page we are on.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Paging",
    UiHint = "Split data into pages and forward just one batch",
    Icon = DataSourceIcons.Stories,
    Type = DataSourceType.Logic, 
    NameId = "ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [InStreamDefaultRequired],
    ConfigurationType = "|Config ToSic.Eav.DataSources.Paging",
    HelpLink = "https://go.2sxc.org/DsPaging")]

public sealed class Paging: DataSourceBase
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


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public Paging(MyServices services, IDataFactory dataFactory): base(services, $"{LogPrefix}.Paging")
    {
        ConnectServices(
            _pagingFactory = dataFactory.New(options: new(typeName: "Paging"))
        );
        ProvideOut(GetList);
        ProvideOut(GetPaging, "Paging");
    }


    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();
        var itemsToSkip = (PageNumber - 1) * PageSize;

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var result = source
            .Skip(itemsToSkip)
            .Take(PageSize)
            .ToImmutableList();
        return l.Return(result, $"page:{PageNumber}; size{PageSize}; found:{result.Count}");
    }

    private IImmutableList<IEntity> GetPaging()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Calculate any additional stuff
        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

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
        return l.ReturnAsOk(list.ToImmutableList());
    }

}