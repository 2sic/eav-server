namespace ToSic.Eav.DataSource.Query.Sys.Inspect;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QuerySourceOutDto
{
    public required string Name { get; init; }
    public required string Scope { get; init; }
}