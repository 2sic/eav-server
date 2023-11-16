using System;

namespace ToSic.Eav.Run.Capabilities
{
    public class SysFeatureDetectorRazor: SysFeatureDetector
    {

        private static readonly SystemCapabilityDefinition DefStatic = new SystemCapabilityDefinition(
            "Razor",
            new Guid("1301aa40-45e0-4349-8a23-2f05ed4120da"),
            "Razor"
        );

        public SysFeatureDetectorRazor(): base(DefStatic, true) { }

    }
}
