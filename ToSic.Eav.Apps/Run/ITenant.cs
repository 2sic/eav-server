using ToSic.Eav.Apps.Run;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <summary>
    /// Describes a tenant - this is what the Environment calls a tenant (like a portal in DNN)
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface ITenant: IWebResource
    {
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
        string AppsRoot { get; }

        [PrivateApi]
        bool RefactorUserIsAdmin { get; }

        [PrivateApi]
        string ContentPath { get; }
    }
}
