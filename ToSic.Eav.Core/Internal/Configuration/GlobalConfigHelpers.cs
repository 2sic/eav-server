namespace ToSic.Eav.Internal.Configuration;

public class GlobalConfigHelpers
{
    public static string ErrorMessageNullNotAllowed(string fieldName) =>
        $"ISystemFoldersConfiguration.{fieldName} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

    public static string CorrectFolderOrErrorIfInvalid(string value, string fieldName) =>
        value?.Backslash().TrimLastSlash() ?? throw new(ErrorMessageNullNotAllowed(fieldName));

    public static string GetDataRoot(string dataFolder) =>
        dataFolder?.EndsWith(Constants.FolderSystem) ?? false
            ? dataFolder.Substring(0, dataFolder.Length - Constants.FolderSystem.Length).TrimLastSlash()
            : dataFolder ?? string.Empty;
}