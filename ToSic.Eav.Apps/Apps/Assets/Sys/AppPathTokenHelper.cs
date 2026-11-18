using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Assets.Sys;

public class AppPathTokenHelper
{
    public static bool HasAppPathToken(string? value)
    {
        value ??= "";
        return value.StartsWith(AppConstants.AppPathPlaceholder, StringComparison.InvariantCultureIgnoreCase)
               || value.StartsWith(AppConstants.AppPathSharedPlaceholder, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string AppPathTokenReplace(string? value, string appPath, string appPathShared)
    {
        value ??= "";
        if (value.StartsWith(AppConstants.AppPathPlaceholder, StringComparison.InvariantCultureIgnoreCase))
            return appPath + value.After(AppConstants.AppPathPlaceholder);
        if (value.StartsWith(AppConstants.AppPathSharedPlaceholder, StringComparison.InvariantCultureIgnoreCase))
            return appPathShared + value.After(AppConstants.AppPathSharedPlaceholder);
        return value;
    }

}
