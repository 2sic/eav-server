using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Blocks
{
    /// <summary>
    /// A unit / block within the CMS. Contains all necessary identification to pass around. 
    /// </summary>
    [PublicApi]
    public interface ICmsBlock
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
