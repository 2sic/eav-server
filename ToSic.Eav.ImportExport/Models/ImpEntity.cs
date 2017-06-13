using System;
using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpEntity: Entity
    {
        /// <summary>
        /// Create a brand new Entity. 
        /// Mainly used for entities which are created for later saving
        /// </summary>
        public ImpEntity(Guid entityGuid, string contentTypeName, IDictionary<string, object> values) : base(entityGuid, contentTypeName, values)
        {
        }
    }
}