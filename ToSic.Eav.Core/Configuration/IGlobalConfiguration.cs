namespace ToSic.Eav.Configuration
{
    public interface IGlobalConfiguration
    {
        /// <summary>
        /// The absolute folder where the data is stored, usually ends in "...\.data"
        /// </summary>
        /// <returns>The folder, can be null if it was never set</returns>
        string DataFolder { get; set; }


        /// <summary>
        /// The main folder (absolute) where anything incl. data is stored
        /// </summary>
        /// <returns>The folder, can be null if it was never set</returns>
        string GlobalFolder { get; set; }


        /// <summary>
        /// The root folder for temporary data
        /// </summary>
        string TemporaryFolder { get; set; }
        
        /// <summary>
        /// This is the root path of where global apps are stored
        /// </summary>
        string GlobalSiteFolder { get; set; }
    }
}