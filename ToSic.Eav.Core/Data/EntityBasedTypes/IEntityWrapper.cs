using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A interface to ensure all things that carry an IEntity can be compared based on the Entity they carry.
    /// </summary>
    [PublicApi]
    public interface IEntityWrapper
    {
        /// <summary>
        /// The underlying entity. 
        /// </summary>
        /// <returns>The entity, or null if not provided</returns>
        IEntity Entity { get; }

        /// <summary>
        /// The underlying entity which is used for equality check.
        /// It's important, because the Entity can sometimes already be wrapped
        /// in which case the various wrappers would think they point
        /// to something different
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [PrivateApi("used vor very internal stuff")]
        IEntity EntityForEqualityCheck { get; }
    }
}
