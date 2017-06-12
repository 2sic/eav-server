using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default
    // Create tests with multiple fields to sort
    // Create tests with DATEs
    // Create tests with Modified!

    [TestClass]
    public class ValueSort_DateTime
    {
        private const int TestVolume = 30, InitialId = 1001;
        private const string Birthdate = "Birthdate";
        private const string ModifiedTest = "InternalModified";
        private const string ModifiedReal = "Modified";
        private readonly ValueSort _testDataGeneratedOutsideTimer;
        public ValueSort_DateTime()
        {
            _testDataGeneratedOutsideTimer = ValueSortShared.GeneratePersonSourceWithDemoData(TestVolume, InitialId);
        }


        [TestMethod]
        public void ValueSort_SortFieldDesc_Birthday()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attributes = Birthdate;
            vf.Directions = "d";
            var result = vf.LightList.ToList();
            // check that each following city is same or larger...
            ValidateDateFieldIsSorted(result, Birthdate, false);
        }

        [TestMethod]
        public void ValueSort_Sort_Modified()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attributes = ModifiedReal;
            var result = vf.LightList.ToList();
            // check that each following city is same or larger...
            ValidateDateFieldIsSorted(result, ModifiedReal, true);
        }

        [TestMethod]
        public void ValueSort_Sort_Modified_Desc()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attributes = ModifiedReal;
            vf.Directions = "d";
            var result = vf.LightList.ToList();
            // check that each following city is same or larger...
            ValidateDateFieldIsSorted(result, ModifiedReal, false);
        }
        //[TestMethod]
        //public void ValueSort_SortFieldAndAsc_City()
        //{
        //    TestSortFieldAndDirection(City, "asc", true);
        //}

        //private void TestSortFieldAndDirection(string field, string direction, bool testAscending)
        //{
        //    var vf = _testDataGeneratedOutsideTimer;
        //    vf.Attributes = field;
        //    vf.Directions = direction;
        //    var result = vf.LightList.ToList();
        //    // check that each following city is same or larger...
        //    ValidateFieldIsSorted(result, field, testAscending);
        //}

        //[TestMethod]
        //public void ValueSort_SortFieldAndA_City()
        //{
        //    TestSortFieldAndDirection(City, "a", true);
        //}
        //[TestMethod]
        //public void ValueSort_SortFieldAndDir1_City()
        //{
        //    TestSortFieldAndDirection(City, "1", true);
        //}
        //public void ValueSort_SortFieldAndGt_City()
        //{
        //    TestSortFieldAndDirection(City, "<", true);
        //}
        //[TestMethod]
        //public void ValueSort_SortFieldAndDesc_City()
        //{
        //    TestSortFieldAndDirection(City, "desc", false);
        //}
        //[TestMethod]
        //public void ValueSort_SortFieldAndD_City()
        //{
        //    TestSortFieldAndDirection(City, "d", false);
        //}
        //public void ValueSort_SortFieldAndDCase_City()
        //{
        //    TestSortFieldAndDirection(City, "D", false);
        //}
        //[TestMethod]
        //public void ValueSort_SortFieldAnd0_City()
        //{
        //    TestSortFieldAndDirection(City, "0", false);
        //}
        //[TestMethod]
        //public void ValueSort_SortFieldAndLt_City()
        //{
        //    TestSortFieldAndDirection(City, ">", false);
        //}

        private void ValidateDateFieldIsSorted(List<ToSic.Eav.Interfaces.IEntity> list, string field, bool asc)
        {
            var previous = list.First().GetBestValue(field) as DateTime?;
            foreach (var entity in list)
            {
                var next = entity.GetBestValue(field) as DateTime?;
                if(!next.HasValue || !previous.HasValue) 
                    throw new Exception("try to compare prev/next dates, but one or both are null");
                var comp = (next.Value - previous.Value).TotalMilliseconds;// string.Compare(previous, next, StringComparison.Ordinal);
                if (asc)
                    Assert.IsTrue(comp >= 0, "new " + field + " " + next + " should be = or larger than prev " + previous);
                else
                    Assert.IsTrue(comp <=0, "new " + field + " " + next + " should be = or smaller than prev " + previous);
                previous = next;
            }
        }

    }
}
