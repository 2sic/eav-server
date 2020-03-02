﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSourceTests
{
    [TestClass]
    public class InitializeTests
    {
        public const string connectionForTests =
            "Data Source=srv-devdb-01;Initial Catalog=eav-testing;Integrated Security=True";

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context) =>
            Eav.ImportExport.Tests.InitializeTests.AssemblyInit(context, connectionForTests);

        // 2020-03-02 2dm - this was in the other test project ToSic.Eav.UnitTests, but probably doesn't affect our tests
        //[AssemblyInitialize]
        //public static void AssemblyInit(TestContext context)
        //{
        //    Testing.Shared.InitializeTests.ConfigureEfcDi();
        //}
    }
}
