using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Serialization
{
    public class Bundle
    {
        public List<ContentTypeSet> ContentTypeSets { get; set; }
        public List<Entity> Entities { get; set; }
    }
}
