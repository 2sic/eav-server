using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.RelationshipTests;

namespace ToSic.Eav.DataSourceTests.RelationshipFilterTests
{
    public partial class RelationshipFilterTest
    {
        // More tests
        // 

        // for contains-many tests
        private const string CatMnyBase = "ContMny";
        private const string CatMny1 = CatMnyBase + "1";
        private const string CatMny2 = CatMnyBase + "2";
        private const string CatMny3 = CatMnyBase + "3";
        private const string CatWithComma = "CategoryWith,Comma";
        private const string CatForAnyTest3 = "CatForAny3Times";

        // expected results
        private const int CompsWithCat12 = 5;
        private const int CompsWithCat123 = 4;
        private const int CompsWithCatComma = 4;
        private const int CompsWithCat12Comma = 3;
        private const int CompsWithCat123Comma = 2;

        private const int CompAnyCat1 = 6;
        private const int CompAnyCat3 = 4;
        private const int CompAnyCat1Or2 = 7;
        private const int CompAnyCat1AndForAny = 8;

        private const int CompsWithCatExactly123 = 2;



        [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains_Implicit() 
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb).Run(true);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains_Explicit() 
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb, 
                compareMode: RelationshipFilter.CompareModes.contains.ToString()).Run(true);


        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllWithoutOperatorOrSepartor_Empty()
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
                .Run(false);



        /// <summary>
        ///  Build a contain-many basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsAll(bool useThree = false, bool repeat2 = false)
            => new RelationshipTest("basic-cat-contains-all", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}"
                + (repeat2 ? $"{DefSeparator}{CatMny2}" : "") // optionally repeat the second value (shouldn't affect result)
                + (useThree ? $"{DefSeparator}{CatMny3}" : ""), // optionally add a third category (for other tests)
                separator: DefSeparator,
                compareMode: RelationshipFilter.CompareModes.contains.ToString());





        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllWithoutSeparator_Empty()
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsCatHavingComma_Find()
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatWithComma}")
                .Run(true, exactCount: CompsWithCatComma);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsCatHavingComma_Empty()
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatWithComma}",
                separator:DefSeparator)
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll2_Find() 
            => BuildContainsAll()
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll2WithDupls_Find() 
            => BuildContainsAll(repeat2: true)
            .Run(true, exactCount: CompsWithCat12);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAll3_Find() 
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
            => new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, 
                $"{CatWeb}{AltSeparator}{CatGreen}", 
                compareMode:RelationshipFilter.CompareModes.contains.ToString()
                )
            .Run(false); // since the separator won't separate, it will not find anything

         [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAllAltSeparator()
         {
             new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                     $"{CatWeb}{AltSeparator}{CatGreen}",
                     compareMode: RelationshipFilter.CompareModes.contains.ToString(),
                     separator: AltSeparator)
                 .Run(true);
         }

         [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains2AndCommaCase()
         {
             new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                     $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatWithComma}",
                     compareMode: RelationshipFilter.CompareModes.contains.ToString(),
                     separator: AltSeparator)
                 .Run(true, exactCount: CompsWithCat12Comma);
         }

         [TestMethod]
        public void DS_RelFil_Comp_Cat_Contains3AndCommaCase()
         {
             new RelationshipTest("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                     $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatMny3}{AltSeparator}{CatWithComma}",
                     compareMode: RelationshipFilter.CompareModes.contains.ToString(),
                     separator: AltSeparator)
                 .Run(true, exactCount: CompsWithCat123Comma);
         }





        /// <summary>
        ///  Build a contain-ANY basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsAny(bool useThree = false)
            => new RelationshipTest("basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}"
                + (useThree ? $"{DefSeparator}{CatMny3}" : ""), // optionally add a third category (for other tests)
                separator: DefSeparator,
                compareMode: RelationshipFilter.CompareModes.containsany.ToString());


        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAny1Or2_Find()
            => BuildContainsAny(true)
                .Run(true, exactCount: CompAnyCat1Or2);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAny1OrSpecial_Find()
        {
            var relf = BuildContainsAny();
            relf.Filter = $"{CatMny1}{DefSeparator}{CatForAnyTest3}";
            relf.Run(true, exactCount: CompAnyCat1AndForAny);
        }




        /// <summary>
        ///  Build a contain-ANY basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsAnything()
            => new RelationshipTest("basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}", // doesn't really matter hat's here, as we only want the none
                separator: DefSeparator,
                compareMode: RelationshipFilter.CompareModes.any.ToString());

        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsAny_FindLots()
        {
            var tst = BuildContainsAnything();
            tst.Run(true);
            Assert.IsTrue(tst.CountApi > 5, "should get more than 5 hits");
        }

        /// <summary>
        ///  Build a contain-ANY basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsNotAnything()
            => new RelationshipTest("basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}", // doesn't really matter hat's here, as we only want the none
                separator: DefSeparator,
                compareMode: "not-" + RelationshipFilter.CompareModes.any);


        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsNone_Find()
            => BuildContainsNotAnything()
                .Run(true, exactCount: 1);


        /// <summary>
        ///  Build a contain-ANY basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildCount(bool useNot, string amount)
            => new RelationshipTest("basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
                $"{amount}", // doesn't really matter hat's here, as we only want the none
                compareMode: (useNot ? "not-" : "") + RelationshipFilter.CompareModes.count);


        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_0_Find1()
            => BuildCount(false, 0.ToString())
                .Run(true, exactCount: 1);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_1_Find()
            => BuildCount(false, 1.ToString())
                .Run(true);
        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_2_Find()
            => BuildCount(false, 2.ToString())
                .Run(true);
        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_12_Empty()
            => BuildCount(false, 12.ToString())
                .Run(false);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_Not12_Empty()
            => BuildCount(true, 12.ToString())
                .Run(true, shouldReturnAll: true);

        [TestMethod]
        public void DS_RelFil_Comp_Cat_Count_InvalidZero_Empty()
            => BuildCount(false, "abc")
                .Run(false);


        /// <summary>
        ///  Build a contain-ANY basic test - but don't run yet
        /// </summary>
        /// <returns></returns>
        private static RelationshipTest BuildContainsFirst2(bool includeTheAny)
            => new RelationshipTest("basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny2}"
                + (includeTheAny ? $"{DefSeparator}{CatForAnyTest3}":""),
                separator: DefSeparator,
                compareMode: RelationshipFilter.CompareModes.first.ToString());


        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsFirst2_Find()
            => BuildContainsFirst2(false)
                .Run(true, exactCount: 1);
        [TestMethod]
        public void DS_RelFil_Comp_Cat_ContainsFirst2AndTheAny_Find()
            => BuildContainsFirst2(true)
                .Run(true, exactCount: 3);

    }
}
