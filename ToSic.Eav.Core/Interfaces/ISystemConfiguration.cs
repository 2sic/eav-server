namespace ToSic.Eav.Interfaces
{
    public interface ISystemConfiguration
    {
        /// <summary>
        /// Db Connection String used in the Eav-Connector
        /// </summary>
        string DbConnectionString { get; }

        /// <summary>
        /// A help link to show the user when a feature isn't available and 
        /// he/she needs to know more
        /// </summary>
        string FeaturesHelpLink { get; }
    }
}
