using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.Interfaces
{
    public interface IDeferredEntitiesList
    {
        IDictionary<int, IEntity> List { get; }
        IEnumerable<IEntity> LightList { get; }  
    }
}
