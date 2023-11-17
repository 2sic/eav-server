using System;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public static class SysFeatureSuggestions
    {
        public static SysFeature CSharp06 { get; } = new SysFeature(
            "CSharp06",
            new Guid("9057e8a4-342f-4574-9cdd-216bfbcc36cc"),
            "CSharp v6"
        );

        public static SysFeature CSharp07 { get; } = new SysFeature(
            "CSharp07",
            new Guid("686f54b2-5464-4eed-8faf-c30a36899b42"),
            "CSharp v7 (7.3)"
        );

        public static SysFeature CSharp08 { get; } = new SysFeature(
            "CSharp08",
            new Guid("a7a88eae-4ec0-4f87-8ab2-40e281031a34"),
            "CSharp v8"
        );
        
        public static SysFeature CSharp09 { get; } = new SysFeature(
            "CSharp09",
            new Guid("bf218ed5-40bf-4726-b49a-a483b2d233ba"),
            "CSharp v9"
        );

        public static SysFeature CSharp10 { get; } = new SysFeature(
            "CSharp10",
            new Guid("2bd937a2-8e8e-4867-ac66-2b1749df6743"),
            "CSharp v10"
        );

        public static SysFeature CSharp11 { get; } = new SysFeature(
            "CSharp11",
            new Guid("d973e815-2489-480c-8a82-19f72cf3aeea"),
            "CSharp v11"
        );

        public static SysFeature CSharp12 { get; } = new SysFeature(
            "CSharp12",
            new Guid("721648d1-0c2e-4795-899c-357a00fddc8a"),
            "CSharp v12 (doesn't exist yet)"
        );

        public static SysFeature Blazor { get; } = new SysFeature(
            "Blazor",
            new Guid("9880cb15-ea2a-4b85-8eb8-7e9ccd390651"),
            "Blazor"
        );

    }
}
