using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests.Types
{
    [TestClass]
    public class AutoLoadFromRuntimeFiles: TestBaseDiCore
    {
        public AutoLoadFromRuntimeFiles()
        {
            _globalTypes = Build<IGlobalTypes>();
        }
        private readonly IGlobalTypes _globalTypes;

        public Tuple<int, int> TypesInFilesRuntime = Tuple.Create(50, 65); // use range

        [TestMethod]
        public void ScanForTypesFileBased()
        {
            var types = _globalTypes.AllContentTypes();
            var count = types.Count;
            Assert.IsTrue(TypesInFilesRuntime.Item1 < count
                          && TypesInFilesRuntime.Item2 > count,
                "expect a fixed about of types at dev time");
        }
    }
}
