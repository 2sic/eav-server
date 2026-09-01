using ToSic.Eav.Sys;

namespace ToSic.Sys.Configuration;

public class GlobalConfigHelpers
{
    public static string CorrectFolderOrErrorIfInvalid(string value, string fieldName) =>
        value?.ToSystemPath().TrimLastSlash() ?? throw new(GlobalConfiguration.ErrorMessageNullNotAllowed(fieldName));

    public static string GetDataRoot(string? dataFolder) =>
        dataFolder?.EndsWith(FolderConstants.DataSubFolderSystem) ?? false
            ? dataFolder.Substring(0, dataFolder.Length - FolderConstants.DataSubFolderSystem.Length).TrimLastSlash()!
            : dataFolder ?? string.Empty;
}
