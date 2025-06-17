using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource.Internal.Inspect;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class QueryRunDto
{
    public required IDictionary<string, IEnumerable<EavLightEntity>> Query { get; init; }
    public required ICollection<InspectStream> Streams { get; init; }
    public required Dictionary<Guid, InspectDataSourceDto> Sources { get; init; }
    public required QueryTimerDto QueryTimer { get; init; }
}

public class QueryTimerDto
{
    public required long Milliseconds { get; init; }
    public required long Ticks { get; init; }
}