using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests.Types
{
    [TestClass]
    // Must inherit from FullAndDb because this also preloads the global types
    public class AutoLoadFromRuntimeFiles: TestBaseDiEavFullAndDb
    {
        // status 2021-11-04 is 77 files
        private int TypesInFileRuntimeMin = 75;
        private int TypesInFileRuntimeMax = 150;

        [TestMethod]
        public void ScanForTypesFileBased()
        {
            var globalApp = Build<IAppStates>().GetPresetApp();
            var types = globalApp.ContentTypes;
            var count = types.Count();
            Assert.IsTrue(TypesInFileRuntimeMin < count
                          && TypesInFileRuntimeMax > count,
                $"expect a fixed about of types at dev time - got {count}");
        }
    }
}
