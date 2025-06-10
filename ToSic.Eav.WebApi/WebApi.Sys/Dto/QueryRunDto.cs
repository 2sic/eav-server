using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource.Internal.Inspect;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class QueryRunDto
{
    public IDictionary<string, IEnumerable<EavLightEntity>> Query;
    public ICollection<InspectStream> Streams;
    public Dictionary<Guid, InspectDataSource> Sources;
    public QueryTimerDto QueryTimer;
}

public class QueryTimerDto
{
    public long Milliseconds;
    public long Ticks;
}