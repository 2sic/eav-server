using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Environment
{
    /// <summary>
    /// A cms-specifically typed block - which still gives access to the underlying item
    /// </summary>
    [PublicApi]
    public interface IContainer<T>: IContainer
    {
        /// <summary>
        /// The underlying, original object. Helpful for inner methods which need access to the real, CMS specific item
        /// </summary>
        T Original { get; }

    }
}
