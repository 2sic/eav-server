using ToSic.Eav.ImportExport.Sys;

namespace ToSic.Eav.Apps.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppConstants
{
    public const string LogName = "App";

    /// <summary>
    /// This is the folder name which contains all apps.
    /// As of now, it's hard-coded to be 2sxc
    /// If every EAV is used in other systems, we will have to put this into the global configuration and init in on startup
    /// </summary>
    public static string AppsRootFolder = "2sxc";

    public const string AppIconFile = "app-icon.png";
    public const string AppPrimaryIconFile = "app-primary.png";

    public static readonly string ContentGroupRefTypeName = "ContentGroupReference";


    #region App Configuration Fields

    //public const string FieldName = AppLoadConstants.FieldName;
    //public const string FieldHidden = AppLoadConstants.FieldHidden;

    #endregion

    // this used to be a Settings.DataIsMissingInDb
    public const int AppIdNotFound = -100;

    // Placeholders / Tokens - probably should move somewhere else someday
    // TODO: CHANGE TO [App:Folder] - must check UI code if this is used anywhere
    public const string AppFolderPlaceholder = AppLoadConstants.AppFolderPlaceholder;
    public static string AppPathPlaceholder = "[App:Path]";
    public static string AppPathSharedPlaceholder = "[App:PathShared]";


    // todo: move to some kind of injection thingy?

    /// <summary>
    /// The type name used to store templates in the eav-system
    /// </summary>
    public const string TemplateContentType = Settings.TemplateContentType;

    /// <summary>
    /// Old name, leftover, ATM can't change quickly...
    /// </summary>
    /// <remarks>
    /// This is not the right place for it, since content-blocks are not
    /// an EAV concept.
    /// But since some services need it, it's best to keep it in a central place.
    /// </remarks>
    public const string ContentGroupTypeName = "2SexyContent-ContentGroup";

}