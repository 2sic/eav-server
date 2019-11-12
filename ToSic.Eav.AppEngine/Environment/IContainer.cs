using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Environment
{
    /// <summary>
    /// A unit / block within the CMS. Contains all necessary identification to pass around. 
    /// </summary>
    [PublicApi]
    public interface IContainer
    {
        /// <summary>
        /// Block ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Page ID
        /// </summary>
        int PageId { get; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        int TenantId { get; }

        /// <summary>
        /// Determines if this is a primary block (directly in the CMS) or a block within a primary block (inner content)
        /// </summary>
        bool IsPrimary { get; }
    }
}
