using System.Linq;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests.RelationshipFilterTests
{
    public partial class Tst_RelationshipFilterDS: RelationshipTestBase
    {

        // for contains-many tests
        const string CatMnyBase = "ContMny";
        const string CatMny1 = CatMnyBase + "1";
        const string CatMny2 = CatMnyBase + "2";
        const string CatMny3 = CatMnyBase + "3";

        private const int CompsWithCat12 = 5;
        private const int CompsWithCat123 = 4;


        [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains_Implicit() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, CatWeb).Run(true);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains_Explitic() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, CatWeb, 
                compareMode: RelationshipFilter.CompareModes.contains.ToString()).Run(true);



        /// <summary>
        ///  Build a contain-many basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsMany(bool useThree = false)
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}" + (useThree ? $"{DefSeparator}{CatMny3}" : ""),
                compareMode: RelationshipFilter.CompareModes.containsall.ToString());

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsManyWithoutOperator()
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsMany2() 
            => BuildContainsMany()
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsMany3() 
            => BuildContainsMany(true)
            .Run(true, exactCount: CompsWithCat123);

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException), "this is expected to fail assert-counts, just for testing that the count is actually controlled")]
        public void DS_RelFil_Comp_Cat_ContainsMany2WrongCount() 
            => BuildContainsMany(true)
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_NotContainsMany()
        {
            var original = BuildContainsMany();

            var not = BuildContainsMany();
            not.CompareMode = "not-" + not.CompareMode;
            not.Rebuild().Run(true);

            Assert.IsTrue(original.CountAll == original.CountApi + not.CountApi, 
                "not and default should together have the total failed "
                + $"tot:{original.CountAll}, having2:{original.CountApi}, not-having:{not.CountApi}");
        }

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsManyBadSeparator() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, 
                $"{CatWeb}{AltSeparator}{CatGreen}", 
                compareMode:RelationshipFilter.CompareModes.containsall.ToString()
                )
            .Run(false); // since the separator won't separate, it will not find anything

         [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsManyAltSeparator()
         {
             new RelationshipTest("basic-cat-having-title", Company, CompCat,
                     $"{CatWeb}{AltSeparator}{CatGreen}",
                     compareMode: RelationshipFilter.CompareModes.containsall.ToString(),
                     separator: AltSeparator)
                 .Run(true);
         }
    }
}
