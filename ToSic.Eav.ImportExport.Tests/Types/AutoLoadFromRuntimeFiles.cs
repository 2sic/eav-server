﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests.Types
{
    [TestClass]
    // Must inherit from FullAndDb because this also preloads the global types
    public class AutoLoadFromRuntimeFiles: TestBaseDiEavFullAndDb
    {
        // status 2021-11-04 is 77 files
        private int TypesInFileRuntimeMin = 75;
        private int TypesInFileRuntimeMax = 90;

        [TestMethod]
        public void ScanForTypesFileBased()
        {
            var _globalTypes = Build<IGlobalTypes>();
            var types = _globalTypes.AllContentTypes();
            var count = types.Count;
            Assert.IsTrue(TypesInFileRuntimeMin < count
                          && TypesInFileRuntimeMax > count,
                $"expect a fixed about of types at dev time - got {count}");
        }
    }
}
