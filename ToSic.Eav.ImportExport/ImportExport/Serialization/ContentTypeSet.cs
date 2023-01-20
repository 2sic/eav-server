using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Serialization
{
    public class ContentTypeSet
    {
        public IContentType ContentType;
        
        // TODO: DISCUSS - DOESN'T SEEM USED
        // SO this entire class doesn't seem to make sense
        public List<Entity> Entities;
    }
}
