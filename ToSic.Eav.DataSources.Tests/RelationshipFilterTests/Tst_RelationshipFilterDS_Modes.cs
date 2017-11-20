using System.Linq;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests.RelationshipFilterTests
{
    public partial class Tst_RelationshipFilterDS: RelationshipTestBase
    {
        // More tests
        // 

        // for contains-many tests
        const string CatMnyBase = "ContMny";
        const string CatMny1 = CatMnyBase + "1";
        const string CatMny2 = CatMnyBase + "2";
        const string CatMny3 = CatMnyBase + "3";

        private const int CompsWithCat12 = 5;
        private const int CompsWithCat123 = 4;
        private const int CompsWithCatComma = 4;
        private const int CompsWithCat12Comma = 3;
        private const int CompsWithCat123Comma = 2;

        private const int CompsWithCatExactly123 = 2;

        // todo
        private const string CatWithComma = "CategoryWith,Comma";


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
        private static RelationshipTest BuildContainsAll(bool useThree = false, bool repeat2 = false)
            => new RelationshipTest("basic-cat-contains-all", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}"
                + (repeat2 ? $"{DefSeparator}{CatMny2}" : "") // optionally repeate the second value (shouldn't affect result)
                + (useThree ? $"{DefSeparator}{CatMny3}" : ""), // optionally add a third category (for other tests)
                separator: DefSeparator,
                compareMode: RelationshipFilter.CompareModes.containsall.ToString());

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllWithoutOperator()
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}", 
                separator: DefSeparator)
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllWithoutSeparator()
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsCatHavingComma()
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatWithComma}")
                .Run(true, exactCount: CompsWithCatComma);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll2() 
            => BuildContainsAll()
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll2WithDupls() 
            => BuildContainsAll(repeat2: true)
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll3() 
            => BuildContainsAll(true)
            .Run(true, exactCount: CompsWithCat123);

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException), "this is expected to fail assert-counts, just for testing that the count is actually controlled")]
        public void DS_RelFil_Comp_Cat_ContainsAll2WrongCount() 
            => BuildContainsAll(true)
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_NotContainsAll()
        {
            var original = BuildContainsAll();
            original.Run(true);

            var not = BuildContainsAll();
            not.CompareMode = "not-" + not.CompareMode;
            not.Run(true);

            Assert.IsTrue(original.CountAll == original.CountApi + not.CountApi, 
                "not and default should together have the total failed "
                + $"tot:{original.CountAll}, having2:{original.CountApi}, not-having:{not.CountApi}");
        }

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllBadSeparator() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, 
                $"{CatWeb}{AltSeparator}{CatGreen}", 
                compareMode:RelationshipFilter.CompareModes.containsall.ToString()
                )
            .Run(false); // since the separator won't separate, it will not find anything

         [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllAltSeparator()
         {
             new RelationshipTest("basic-cat-having-title", Company, CompCat,
                     $"{CatWeb}{AltSeparator}{CatGreen}",
                     compareMode: RelationshipFilter.CompareModes.containsall.ToString(),
                     separator: AltSeparator)
                 .Run(true);
         }

         [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains2AndCommaCase()
         {
             new RelationshipTest("basic-cat-having-title", Company, CompCat,
                     $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatWithComma}",
                     compareMode: RelationshipFilter.CompareModes.containsall.ToString(),
                     separator: AltSeparator)
                 .Run(true, exactCount: CompsWithCat12Comma);
         }

         [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains3AndCommaCase()
         {
             new RelationshipTest("basic-cat-having-title", Company, CompCat,
                     $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatMny3}{AltSeparator}{CatWithComma}",
                     compareMode: RelationshipFilter.CompareModes.containsall.ToString(),
                     separator: AltSeparator)
                 .Run(true, exactCount: CompsWithCat123Comma);
         }

        /// <summary>
        ///  Build a contain-many basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsAny(bool useThree = false)
            => new RelationshipTest("basic-cat-having-title", Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}" + (useThree ? $"{DefSeparator}{CatMny3}" : ""),
                compareMode: RelationshipFilter.CompareModes.todocontainsany.ToString());

    }
}
