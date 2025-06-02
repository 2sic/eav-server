using ToSic.Eav.Internal.Loaders;
using ToSic.Sys.Configuration;
using static ToSic.Eav.Sys.Configuration.GlobalConfigHelpers;

namespace ToSic.Eav.Sys.Configuration;

public static class GlobalConfigSysFolders
{
    /// <summary>
    /// The absolute folder where the data is stored, usually ends in "App_Data\system" (or ".data")
    /// </summary>
    /// <returns>The folder. Will be generated if it was never set</returns>
    public static string DataFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.GlobalFolder(), Constants.AppDataProtectedFolder, Constants.FolderSystem));

    /// <summary>
    /// The absolute folder where the data is stored, usually ends in "App_Data\system" (or ".data")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void DataFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(DataFolder)));



    /// <summary>
    /// The absolute folder where the beta data is stored, usually ends in "App_Data\system-beta" (or ".databeta")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static string DataBetaFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(GetDataRoot(config.DataFolder()), Constants.FolderSystemBeta));

    /// <summary>
    /// The absolute folder where the beta data is stored, usually ends in "App_Data\system-beta" (or ".databeta")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void DataBetaFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(DataBetaFolder)));



    /// <summary>
    /// The absolute folder where the custom data is stored, usually ends in "App_Data\system-custom" (or ".data-custom")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static string DataCustomFolder(this IGlobalConfiguration config)
       => config.GetThisOrSet(() => Path.Combine(GetDataRoot(config.DataFolder()), Constants.FolderSystemCustom));

    /// <summary>
    /// The absolute folder where the custom data is stored, usually ends in "App_Data\system-custom" (or ".data-custom")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void DataCustomFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(DataCustomFolder)));



    /// <summary>
    /// The main folder (absolute) where anything incl. data is stored
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static string GlobalFolder(this IGlobalConfiguration config)
        => config.GetThisErrorOnNull();

    /// <summary>
    /// The main folder (absolute) where anything incl. data is stored
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void GlobalFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(GlobalFolder)));



    /// <summary>
    /// The root folder for temporary data
    /// </summary>
    public static string TemporaryFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.GlobalFolder(), Constants.TemporaryFolder));

    /// <summary>
    /// The root folder for temporary data
    /// </summary>
    public static void TemporaryFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(TemporaryFolder)));



    /// <summary>
    /// This is the root path of where global apps are stored
    /// </summary>
    public static string SharedAppsFolder(this IGlobalConfiguration config)
        => config.GetThisErrorOnNull();

    /// <summary>
    /// This is the root path of where global apps are stored
    /// </summary>
    public static void SharedAppsFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(SharedAppsFolder)));

    /// <summary>
    /// The assets virtual url to main module global folder where assets are stored.
    /// Eg: "~/DesktopModules/ToSic_SexyContent/assets/" in DNN,
    /// "~/Modules/ToSic.Sxc.Oqtane/assets/" in Oqtane.
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static string AssetsVirtualUrl(this IGlobalConfiguration config)
        => config.GetThisErrorOnNull();

    /// <summary>
    /// The assets virtual url to main module global folder where assets are stored.
    /// Eg: "~/DesktopModules/ToSic_SexyContent/assets/" in DNN,
    /// "~/Modules/ToSic.Sxc.Oqtane/assets/" in Oqtane.
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void AssetsVirtualUrl(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(AssetsVirtualUrl)));


    /// <summary>
    /// The absolute folder where the configurations are stored.
    /// Used for licenses and features.
    /// </summary>
    public static string ConfigFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.DataCustomFolder(), FsDataConstants.ConfigFolder));

    /// <summary>
    /// The absolute folder where the configurations are stored.
    /// Used for licenses and features.
    /// </summary>
    public static void ConfigFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(ConfigFolder)));

    /// <summary>
    /// The absolute folder where the instructions are stored.
    /// Used for app exports.
    /// </summary>
    public static string InstructionsFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.GlobalFolder(), Constants.InstructionsFolder));

    /// <summary>
    /// The absolute folder where the instructions are stored.
    /// Used for app exports.
    /// </summary>
    public static void InstructionsFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(InstructionsFolder)));


    /// <summary>
    /// The absolute folder where the template of App_Data with app.json are stored.
    /// Used when new app is created.
    /// </summary>
    public static string AppDataTemplateFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.GlobalFolder(), Constants.AppDataProtectedFolder, Constants.NewAppFolder));

    /// <summary>
    /// The absolute folder where the template of App_Data with app.json are stored.
    /// Used when new app is created.
    /// </summary>
    public static void AppDataTemplateFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(AppDataTemplateFolder)));

    /// <summary>
    /// The absolute folder where the template of new apps are stored.
    /// Used when new app is created.
    /// </summary>

    public static string NewAppsTemplateFolder(this IGlobalConfiguration config)
        => config.GetThisOrSet(() => Path.Combine(config.GlobalFolder(), Constants.AppDataProtectedFolder, Constants.NewAppsFolder));

    /// <summary>
    /// The absolute folder where the template of new apps are stored.
    /// Used when new app is created.
    /// </summary>
    public static void NewAppsTemplateFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(CorrectFolderOrErrorIfInvalid(value, nameof(NewAppsTemplateFolder)));



    /// <summary>
    /// The absolute folder where the 2sxc app temp assemblies for AppCode, assembly dependencies... are stored.
    /// </summary>
    public static string TempAssemblyFolder(this IGlobalConfiguration config)
        => config.GetThisErrorOnNull();

    /// <summary>
    /// The absolute folder where the 2sxc app temp assemblies for AppCode, assembly dependencies... are stored.
    /// </summary>
    public static void TempAssemblyFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(value);
}
