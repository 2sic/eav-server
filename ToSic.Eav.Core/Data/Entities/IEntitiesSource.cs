using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This marks something which can give a list of entities. Usually used for lazy-loading data, where the source is attached, but the data isn't loaded yet. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IEntitiesSource: ICacheExpiring
    {
        IEnumerable<IEntity> List { get; }  
    }
}
