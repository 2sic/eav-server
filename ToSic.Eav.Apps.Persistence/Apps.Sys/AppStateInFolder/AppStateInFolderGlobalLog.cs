namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStateInFolderGlobalLog
{
    public static ILog LoadLog { get; internal set; } = null;
}