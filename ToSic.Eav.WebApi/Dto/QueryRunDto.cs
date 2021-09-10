using System;
using System.Collections.Generic;
using ToSic.Eav.ImportExport.Json.V0;

namespace ToSic.Eav.WebApi.Dto
{
    public class QueryRunDto
    {
        public IDictionary<string, IEnumerable<IJsonEntity>> Query;
        public List<DataSources.Debug.StreamInfo> Streams;
        public Dictionary<Guid, DataSources.Debug.DataSourceInfo> Sources;
        public QueryTimerDto QueryTimer;
    }

    public class QueryTimerDto
    {
        public long Milliseconds;
        public long Ticks;
    }
}
