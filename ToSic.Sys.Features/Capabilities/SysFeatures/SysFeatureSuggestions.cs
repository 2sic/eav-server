using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Core definitions of system features. These will be initialized on the platform according to capabilities.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class SysFeatureSuggestions
{
    // Only DNN will ever need to install Cs8, so we'll keep this link here
    private static readonly string LinkDnnCs8 = "https://r.2sxc.org/dnn-roslyn";
    private static readonly string CsDescription = "C# Language Support when Compiling Razor and other code for C# v";

    private static SysFeature CSharp(Guid guid, int version) => new()
    {
        NameId = $"CSharp{version:00}",
        Guid = guid,
        Name = $"CSharp v{version}",
        Description = CsDescription + version,
        Link = LinkDnnCs8,
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };


    public static SysFeature CSharp06 { get; } = CSharp(new("9057e8a4-342f-4574-9cdd-216bfbcc36cc"), 6);

    public static SysFeature CSharp07 { get; } = CSharp(new("686f54b2-5464-4eed-8faf-c30a36899b42"), 7);

    public static SysFeature CSharp08 { get; } = CSharp(new("a7a88eae-4ec0-4f87-8ab2-40e281031a34"), 8);

    public static SysFeature CSharp09 { get; } = CSharp(new("bf218ed5-40bf-4726-b49a-a483b2d233ba"), 9);

    public static SysFeature CSharp10 { get; } = CSharp(new("2bd937a2-8e8e-4867-ac66-2b1749df6743"), 10);

    public static SysFeature CSharp11 { get; } = CSharp(new("d973e815-2489-480c-8a82-19f72cf3aeea"), 11);

    public static SysFeature CSharp12 { get; } = CSharp(new("721648d1-0c2e-4795-899c-357a00fddc8a"), 12);

    public static SysFeature CSharp13 { get; } = CSharp(new("89b49dd6-2683-44b0-b812-195a7f78b3b0"), 13);

    public static SysFeature CSharp14 { get; } = CSharp(new("cabe7c09-6350-4d78-b463-ac254832c7c2"), 14);

    public static SysFeature CSharp15 { get; } = CSharp(new("90db53f5-d269-41f1-b0d0-808ea22950bc"), 15);

    public static SysFeature CSharp16 { get; } = CSharp(new("d9994537-3da9-4e72-8456-1ddb5b825f6c"), 16);

    public static SysFeature Blazor { get; } = new()
    {
        NameId = "Blazor",
        Guid = new("9880cb15-ea2a-4b85-8eb8-7e9ccd390651"),
        Name = "Blazor",
        Description = "Blazor is a framework for building interactive client-side web UI with .NET",
        LicenseRules = BuiltInLicenseRules.SystemEnabled,
    };

}