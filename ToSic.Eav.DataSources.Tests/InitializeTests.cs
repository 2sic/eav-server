﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) => Testing.Shared.InitializeTests.ConfigureEfcDi();

    }
}
