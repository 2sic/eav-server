using System;
using System.Collections.Generic;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource.Debug;


namespace ToSic.Eav.WebApi.Dto;

public class QueryRunDto
{
    public IDictionary<string, IEnumerable<EavLightEntity>> Query;
    public List<StreamInfo> Streams;
    public Dictionary<Guid, DataSourceInfo> Sources;
    public QueryTimerDto QueryTimer;
}

public class QueryTimerDto
{
    public long Milliseconds;
    public long Ticks;
}