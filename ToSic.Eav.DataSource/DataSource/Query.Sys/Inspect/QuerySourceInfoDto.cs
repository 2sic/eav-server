namespace ToSic.Eav.DataSource.Query.Sys.Inspect;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QuerySourceInfoDto
{
    /// <summary>
    /// DS Guid for identification
    /// </summary>
    public Guid? Guid { get; }

    /// <summary>
    /// Internal type
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// The resolved values used from the settings/config of the data source
    /// </summary>
    public IReadOnlyDictionary<string, string>? Configuration { get; }

    /// <summary>
    /// Error state
    /// </summary>
    public bool Error { get; set; }

    public Dictionary<string, object?>? Definition;

    public IList<QuerySourceOutDto>? Out;

    public QueryConnectionsDto? Connections { get; init; }

    public QuerySourceInfoDto(IDataSource ds)
    {
        try
        {
            Guid = ds.Guid;
            Type = ds.GetType().Name;
            Configuration = ds.Configuration.Values;

            var connections = (ds as DataSourceBase)?.Connections;
            if (connections != null)
                Connections = new()
                {
                    In = connections.In.Select(c => new QueryConnectionDto(c)).ToList(),
                    Out = connections.Out.Select(c => new QueryConnectionDto(c)).ToList(),
                };

            try
            {
                Out = ds.Out
                    .Select(o => new QuerySourceOutDto
                    {
                        Name = o.Key,
                        Scope = o.Value.Scope
                    })
                    .ToListOpt();
            }
            catch { /* ignore */ }
        }
        catch (Exception)
        {
            Error = true;
        }
    }

    public QuerySourceInfoDto WithQueryDef(QueryDefinition queryDefinition)
    {
        // find this item in the query def
        var partDef = queryDefinition.Parts
            .FirstOrDefault(p => p.Guid == Guid);
        if(partDef is null)
            return this;

        Definition = partDef.AsDictionary();
        return this;
    }
}