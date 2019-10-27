using ToSic.Eav.PublicApi;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// Describes a tenant - this is what the Environment calls a tenant (like a portal in DNN)
    /// </summary>
    [PublicApi.PublicApi]
    public interface ITenant
    {
        /// <summary>
        /// The tenant ID as a number (if the tenant supports it)
        /// </summary>
        /// <returns>The DNN PortalId</returns>
        int Id { get; }

        /// <summary>
        /// The default language code - like "en-US"
        /// </summary>
        string DefaultLanguage { get; }


        /// <summary>
        /// The tenant name for human readability (UIs)
        /// Usually the DNN PortalName
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The root path of the tenant
        /// </summary>
        [PrivateApi]
        string SxcPath { get; }

        [PrivateApi]
        bool RefactorUserIsAdmin { get; }

        [PrivateApi]
        string ContentPath { get; }
    }
}
