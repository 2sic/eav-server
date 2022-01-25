namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This contains system level settings for an app.
    /// Mainly (ATM) for loading inherited apps with inheritance specific configuration
    /// </summary>
    /// <remarks>
    /// New in v13.01
    /// </remarks>
    public class AppSysSettings
    {
        /// <summary>
        /// Just a kind of version info so we can see what version of EAV created this in the DB
        /// </summary>
        public string Version { get; set; } = "13.00.00";

        /// <summary>
        /// Determines if this app inherits from another app.
        /// Typically as of v13 it's true if the Ancestor is set. 
        /// </summary>
        public bool Inherit { get; set; }

        /// <summary>
        /// ID of the ancestor app - must be a number, as the guid could sometimes occur in multiple sites (like when an app is created in many sites, and one is converted to global). 
        /// </summary>
        public int AncestorAppId { get; set; }
    }
}
