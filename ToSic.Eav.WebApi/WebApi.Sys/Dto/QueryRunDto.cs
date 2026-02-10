using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource.Query.Sys.Inspect;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class QueryRunDto
{
    public required IDictionary<string, IEnumerable<EavLightEntity>> Query { get; init; }
    public required ICollection<QueryStreamInfoDto> Streams { get; init; }
    public required Dictionary<Guid, QuerySourceInfoDto> Sources { get; init; }
    public required QueryTimerDto QueryTimer { get; init; }
}

public class QueryTimerDto
{
    public required long Milliseconds { get; init; }
    public required long Ticks { get; init; }
}