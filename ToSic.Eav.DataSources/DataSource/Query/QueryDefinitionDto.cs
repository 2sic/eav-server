using System.Collections.Generic;

namespace ToSic.Eav.DataSource.Query
{

    public class QueryDefinitionDto
    {
        public Dictionary<string, object> Pipeline { get; set; }
        public List<Dictionary<string, object>> DataSources { get; set; } = new();
    }
}
