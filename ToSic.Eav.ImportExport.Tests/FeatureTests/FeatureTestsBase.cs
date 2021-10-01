using ToSic.Eav.Configuration;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests.FeatureTests
{
    public class FeatureTestsBase: EavTestBase
    {
        public FeatureTestsBase()
        {
            // Make sure that features are ready to use
            var sysLoader = Resolve<SystemLoader>();
            sysLoader.Reload();
        }
    }
}
