using ToSic.Eav.DataSource.Sys;
using static ToSic.Eav.DataSource.DataSourceConstants;


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
    NameId = "4c30d275-834d-42d5-8196-12c8dbbbb6f8",
    NameIds = ["ToSic.Eav.DataSources.Paging, ToSic.Eav.DataSources"],
    In = [InStreamDefaultRequired],
    ConfigurationType = "|Config ToSic.Eav.DataSources.Paging",
    HelpLink = "https://go.2sxc.org/DsPaging")]
public sealed class Paging: CustomDataSourceAdvanced
{
    #region Configuration-properties

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
    public Paging(Dependencies services): base(services, $"{DataSourceConstantsInternal.LogPrefix}.Paging")
    {
        ProvideOut(GetList);
        ProvideOut(GetPaging, "Paging");
    }


    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var itemsToSkip = (PageNumber - 1) * PageSize;

        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

        var result = source
            .Skip(itemsToSkip)
            .Take(PageSize)
            .ToImmutableOpt();
        return l.Return(result, $"page:{PageNumber}; size{PageSize}; found:{result.Count}");
    }

    private IImmutableList<IEntity> GetPaging()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // Calculate any additional stuff
        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

        var rawEntity = new PagingModel(
            PageSize: PageSize,
            PageNumber: PageNumber,
            ItemCount: source.Count,
            PageCount: (int)Math.Ceiling((decimal)source.Count / PageSize)
        );

        // Assemble list of this for the stream
        List<IEntity> list = [DataFactory.Create(rawEntity)];
        return l.ReturnAsOk(list.ToImmutableOpt());
    }
}