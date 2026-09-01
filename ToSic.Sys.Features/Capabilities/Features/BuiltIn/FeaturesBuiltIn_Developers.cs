namespace ToSic.Sys.Capabilities.Features;

public partial class BuiltInFeatures
{
    public static readonly Feature FileNamesCrossPlatform = new()
    {
        NameId = "FileNamesCrossPlatform",
        Guid = new("9fc49d7b-9071-4004-a75a-da5157e5c333"),
        Name = "Warn about issues with file names",
        IsPublic = false,
        Ui = false,
        Description = "Warn about issues with file names across different platforms",
        Security = new(0, "Does not affect security."),
        LicenseRules = BuiltInLicenseRules.DevCoreDisabled,
        RunOnStateChange = (state, _) => { DevFeatures.FileNamesCrossPlatform = state.IsEnabled; },
    };

    public static class DevFeatures
    {
        public static bool FileNamesCrossPlatform { get; set; }
    }
}