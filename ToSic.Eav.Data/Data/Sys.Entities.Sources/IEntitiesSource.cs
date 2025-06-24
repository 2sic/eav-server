using ToSic.Sys.Caching;

namespace ToSic.Eav.Data.Entities.Sys.Sources;

/// <summary>
/// This marks something which can give a list of entities. Usually used for lazy-loading data, where the source is attached, but the data isn't loaded yet. 
/// </summary>
[PrivateApi("this is just fyi")]
public interface IEntitiesSource: ICacheExpiring
{
    IEnumerable<IEntity> List { get; }  
}