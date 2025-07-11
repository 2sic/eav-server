﻿using ToSic.Eav.Sys;

namespace ToSic.Sys.Configuration;

public class GlobalConfigHelpers
{
    private static string ErrorMessageNullNotAllowed(string fieldName) =>
        $"ISystemFoldersConfiguration.{fieldName} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

    public static string CorrectFolderOrErrorIfInvalid(string value, string fieldName) =>
        value?.Backslash().TrimLastSlash() ?? throw new(ErrorMessageNullNotAllowed(fieldName));

    public static string GetDataRoot(string? dataFolder) =>
        dataFolder?.EndsWith(FolderConstants.DataSubFolderSystem) ?? false
            ? dataFolder.Substring(0, dataFolder.Length - FolderConstants.DataSubFolderSystem.Length).TrimLastSlash()!
            : dataFolder ?? string.Empty;
}