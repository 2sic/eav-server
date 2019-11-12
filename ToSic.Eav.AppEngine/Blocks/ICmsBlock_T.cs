using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Blocks
{
    /// <summary>
    /// A cms-specifically typed block - which still gives access to the underlying item
    /// </summary>
    [PublicApi]
    public interface ICmsBlock<T>: ICmsBlock
    {
        /// <summary>
        /// The underlying, original object. Helpful for inner methods which need access to the real, CMS specific item
        /// </summary>
        T Original { get; }

    }
}
