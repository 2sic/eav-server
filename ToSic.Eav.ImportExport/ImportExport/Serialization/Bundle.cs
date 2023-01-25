using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Serialization
{
    public class Bundle
    {
        public List<IContentType> ContentTypes { get; set; }
        public List<IEntity> Entities { get; set; }
    }
}
