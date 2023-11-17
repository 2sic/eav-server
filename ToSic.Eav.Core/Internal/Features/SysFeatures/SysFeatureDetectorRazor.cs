using System;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public class SysFeatureDetectorRazor: SysFeatureDetector
    {

        private static readonly SysFeature DefStatic = new SysFeature(
            "Razor",
            new Guid("1301aa40-45e0-4349-8a23-2f05ed4120da"),
            "Razor"
        );

        public SysFeatureDetectorRazor(): base(DefStatic, true) { }

    }
}
