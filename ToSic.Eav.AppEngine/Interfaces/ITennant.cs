namespace ToSic.Eav.Apps.Interfaces
{
    public interface ITennant
    {
        /// <summary>
        /// The tennant ID as a number (if the tennant supports it)
        /// Usually the DNN PortalId
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The default language code - like "en-US"
        /// </summary>
        string DefaultLanguage { get; }


        /// <summary>
        /// The tennant name for human readibility (UIs)
        /// Usually the DNN PortalName
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The root path of the tennant
        /// </summary>
        string RootPath { get; }


        bool RefactorUserIsAdmin { get; }

    }
}
