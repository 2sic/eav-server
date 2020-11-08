﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.DataSourceTests.ExternalData;
using ToSic.Eav.DataSourceTests.ValueFilter;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterBoolean: EavTestBase
    {
        private readonly ValueFilterMaker _valueFilterMaker;

        public ValueFilterBoolean()
        {
            _valueFilterMaker = Resolve<ValueFilterMaker>();
        }
        //private const int TestVolume = 10000;
        //private ValueFilter _testDataGeneratedOutsideTimer;
        //public ValueFilterBoolean()
        //{
        //    //_testDataGeneratedOutsideTimer = ValueFilter_Test.CreateValueFilterForTesting(TestVolume);
        //}
        

        #region bool tests

        public void FilterBool(string compareValue, int desiredFinds, int populationRoot)
        {
            var vf = _valueFilterMaker.CreateValueFilterForTesting(populationRoot * DataTableTst.IsMaleForEveryX); // only every 3rd is male in the demo data
            vf.Attribute = "IsMale";
            vf.Value = compareValue;
            var found = vf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

        [TestMethod]
        public void ValueFilter_FilterBool()
        {
            FilterBool(true.ToString(), 82, 82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing1()
        {
            FilterBool("true", 82, 82);
        }
        [TestMethod]
        public void ValueFilter_FilterBoolCasing2()
        {
            FilterBool("TRUE", 82,82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing3()
        {
            FilterBool("FALSE", 164, 82);
        }
        #endregion
        
    }
}
