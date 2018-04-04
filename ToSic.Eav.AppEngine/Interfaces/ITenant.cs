namespace ToSic.Eav.Apps.Interfaces
{
    public interface ITenant
    {
        /// <summary>
        /// The tenant ID as a number (if the tenant supports it)
        /// Usually the DNN PortalId
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The default language code - like "en-US"
        /// </summary>
        string DefaultLanguage { get; }


        /// <summary>
        /// The tenant name for human readibility (UIs)
        /// Usually the DNN PortalName
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The root path of the tenant
        /// </summary>
        string SxcPath { get; }


        bool RefactorUserIsAdmin { get; }

        string ContentPath { get; }
    }
}
