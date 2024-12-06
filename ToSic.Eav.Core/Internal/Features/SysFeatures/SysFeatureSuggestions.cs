using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public static class SysFeatureSuggestions
{
    // Only DNN will ever need to install Cs8, so we'll keep this link here
    private static readonly string LinkDnnCs8 = "https://r.2sxc.org/dnn-roslyn";
    private static readonly string CsDescription = "C# Language Support when Compiling Razor and other code for C# v";

    public static SysFeature CSharp06 { get; } = new()
    {
        NameId = "CSharp06",
        Guid = new("9057e8a4-342f-4574-9cdd-216bfbcc36cc"),
        Name = "CSharp v6",
        Description = CsDescription + "6",
        Link = LinkDnnCs8,
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature CSharp07 { get; } = new()
    {
        NameId = "CSharp07",
        Guid = new("686f54b2-5464-4eed-8faf-c30a36899b42"),
        Name = "CSharp v7 (7.3)",
        Description = CsDescription + "7",
        Link = LinkDnnCs8,
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature CSharp08 { get; } = new()
    {
        NameId = "CSharp08",
        Guid = new("a7a88eae-4ec0-4f87-8ab2-40e281031a34"),
        Name = "CSharp v8",
        Description = CsDescription + "8",
        Link = LinkDnnCs8,
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };
        
    public static SysFeature CSharp09 { get; } = new()
    {
        NameId = "CSharp09",
        Guid = new("bf218ed5-40bf-4726-b49a-a483b2d233ba"),
        Name = "CSharp v9",
        Description = CsDescription + "9",
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature CSharp10 { get; } = new()
    {
        NameId = "CSharp10",
        Guid = new("2bd937a2-8e8e-4867-ac66-2b1749df6743"),
        Name = "CSharp v10",
        Description = CsDescription + "10",
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature CSharp11 { get; } = new()
    {
        NameId = "CSharp11",
        Guid = new("d973e815-2489-480c-8a82-19f72cf3aeea"),
        Name = "CSharp v11",
        Description = CsDescription + "11",
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature CSharp12 { get; } = new()
    {
        NameId = "CSharp12",
        Guid = new("721648d1-0c2e-4795-899c-357a00fddc8a"),
        Name = "CSharp v12",
        Description = CsDescription + "12",
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

    public static SysFeature Blazor { get; } = new()
    {
        NameId = "Blazor",
        Guid = new("9880cb15-ea2a-4b85-8eb8-7e9ccd390651"),
        Name = "Blazor",
        Description = "Blazor is a framework for building interactive client-side web UI with .NET",
        LicenseRules = BuiltInFeatures.SystemEnabled,
    };

}