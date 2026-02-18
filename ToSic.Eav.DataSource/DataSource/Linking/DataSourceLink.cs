namespace ToSic.Eav.DataSource;

/// <summary>
/// A reference to another data-source, which can be used to link multiple data-sources together.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record DataSourceLink : IDataSourceLink
{
    /// <inheritdoc/>
    public required IDataSource? DataSource { get; init; }

    /// <inheritdoc/>
    [field: AllowNull, MaybeNull]
    public string OutName
    {
        get => field ??= DataSourceConstants.StreamDefaultName;
        init;
    }

    /// <inheritdoc/>
    [field: AllowNull, MaybeNull]
    public string InName
    {
        get => field ??= DataSourceConstants.StreamDefaultName;
        init;
    }

    public IDataStream? Stream { get; init; }

    public IEnumerable<IDataSourceLink> More { get; init; } = [];

    /// <inheritdoc/>
    public IDataSourceLink WithRename(string? outName = default, string? inName = default) =>
        // Check if no names provided
        $"{outName}{inName}".IsEmptyOrWs()
            ? this
            : this with
            {
                OutName = outName ?? OutName,
                InName = inName ?? InName,
            };

    /// <inheritdoc/>
    public IDataSourceLink WithAnotherStream(string? name = default, string? outName = default, string? inName = default) =>
        // Check if no names provided
        $"{name}{outName}{inName}".IsEmptyOrWs()
            ? this
            : WithMore([
                new DataSourceLink
                {
                    DataSource = DataSource,
                    InName = name ?? inName ?? DataSourceConstants.StreamDefaultName,
                    OutName = name ?? outName ?? DataSourceConstants.StreamDefaultName,
                }
            ]);

    /// <inheritdoc/>
    public IDataSourceLink WithMore(IDataSourceLinkable[] more)
    {
        // If no more sources provided, just return this, as it's unmodified
        if (more.SafeNone())
            return this;

        var newMore = more
            .Select(m => m.GetLink())
            .Concat(More)
            .ToListOpt();

        // Note: it's important that if we add more sources, 
        // they are added _within_ the current source.
        // This ensures that the main / outer source is the primary
        // which will also provide AppId, Lookups etc.
        return this with { More = newMore };
    }

    IDataSourceLink IDataSourceLinkable.GetLink() => this;
}