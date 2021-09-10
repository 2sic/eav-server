using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Json.V0
{
    /// <inheritdoc />
    [PrivateApi("don't publish implementation")] 
    public class JsonEntity: Dictionary<string, object>, IJsonEntity
    {
        public JsonEntity() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public string TestValue27 => "test value - does this make it to the output?";
    }
}
