namespace ToSic.Eav.Configuration
{
    public interface IGlobalConfiguration
    {
        /// <summary>
        /// The absolute folder where the data is stored, usually ends in "App_Data\system" (or ".data")
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
        string SharedAppsFolder { get; set; }

        /// <summary>
        /// The assets virtual url to main module global folder where assets are stored.
        /// Eg: "~/DesktopModules/ToSic_SexyContent/assets/" in DNN,
        /// "~/Modules/ToSic.Sxc.Oqtane/assets/" in Oqtane.
        /// </summary>
        /// <returns>The folder, can be null if it was never set</returns>
        string AssetsVirtualUrl { get; set; }

        /// <summary>
        /// The absolute folder where the configurations are stored.
        /// Used for licenses and features.
        /// </summary>
        string ConfigFolder { get; set; }


        /// <summary>
        /// The absolute folder where the instructions are stored.
        /// Used for app exports.
        /// </summary>
        string InstructionsFolder { get; set; }

        /// <summary>
        /// The absolute folder where the template of App_Data with app.json are stored.
        /// Used when new app is created.
        /// </summary>
        string AppDataTemplateFolder { get; set; }
    }
}