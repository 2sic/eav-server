﻿using System;
using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class QueryRunDto
    {
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Query;
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
