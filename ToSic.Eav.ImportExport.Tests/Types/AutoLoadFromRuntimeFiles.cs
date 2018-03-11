using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Tests.Types
{
    [TestClass]
    public class AutoLoadFromRuntimeFiles
    {
        public Tuple<int, int> TypesInFilesRuntime = Tuple.Create(40, 50); // use range

        [TestMethod]
        public void ScanForTypesFileBased()
        {
            var types = Global.AllContentTypes();
            var count = types.Count;
            Assert.IsTrue(TypesInFilesRuntime.Item1 < count
                          && TypesInFilesRuntime.Item2 > count,
                "expect a fixed about of types at dev time");
        }
    }
}
