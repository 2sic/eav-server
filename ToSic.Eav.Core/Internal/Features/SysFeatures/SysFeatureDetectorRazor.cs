using System;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public class SysFeatureDetectorRazor() : SysFeatureDetector(DefStatic, true)
{

    private static readonly SysFeature DefStatic = new(
        "Razor",
        new("1301aa40-45e0-4349-8a23-2f05ed4120da"),
        "Razor"
    );
}