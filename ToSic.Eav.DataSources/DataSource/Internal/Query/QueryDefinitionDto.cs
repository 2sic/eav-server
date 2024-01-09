using System.Collections.Generic;

namespace ToSic.Eav.DataSource.Internal.Query;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryDefinitionDto
{
    public Dictionary<string, object> Pipeline { get; set; }
    public List<Dictionary<string, object>> DataSources { get; set; } = new();
}