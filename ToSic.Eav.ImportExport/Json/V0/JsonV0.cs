using System.Collections.Generic;

namespace ToSic.Eav.ImportExport.Json.V0
{
    /// <summary>
    /// The First JSON format of EAV. It's a simple dictionary with name-value pairs.
    /// Note that it's for export/serialization only, there is no way to import an entity of this type as of now.
    /// </summary>
    /// <remarks>
    /// Introduced ca. 2sxc 4.0, but for the documentation we created an own JsonV0 type in 2sxc 12.05
    /// </remarks>
    public class JsonV0: Dictionary<string, object>, IDictionary<string, object>
    {
    }
}
