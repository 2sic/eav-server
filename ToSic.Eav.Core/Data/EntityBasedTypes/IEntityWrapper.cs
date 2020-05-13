using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A interface to ensure all things that carry an IEntity can be compared based on the Entity they carry.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode("still WIP")]
    public interface IEntityWrapper
    {
        /// <summary>
        /// The underlying entity. 
        /// </summary>
        /// <returns>The entity, or null if not provided</returns>
        IEntity Entity { get; }
    }
}
