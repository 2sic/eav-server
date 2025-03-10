using Xunit.Sdk;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

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



    [Fact]
    public void CompanyCategory_Contains_Implicit() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb).Run(true);

    [Fact]
    public void CompanyCategory_Contains_Explicit() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb, 
            compareMode: RelationshipFilter.CompareModeContains).Run(true);


    [Fact]
    public void CompanyCategory_ContainsAllWithoutOperatorOrSepartor_Empty()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
            .Run(false);



    /// <summary>
    ///  Build a contain-many basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildContainsAll(bool useThree = false, bool repeat2 = false)
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-all", RelationshipTestSpecs.Company, CompCat,
            $"{CatMny1}{DefSeparator}{CatMny2}"
            + (repeat2 ? $"{DefSeparator}{CatMny2}" : "") // optionally repeat the second value (shouldn't affect result)
            + (useThree ? $"{DefSeparator}{CatMny3}" : ""), // optionally add a third category (for other tests)
            separator: DefSeparator,
            compareMode: RelationshipFilter.CompareModeContains);





    [Fact]
    public void CompanyCategory_ContainsAllWithoutSeparator_Empty()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{DefSeparator}{CatMny2}")
            .Run(false);

    [Fact]
    public void CompanyCategory_ContainsCatHavingComma_Find()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatWithComma}")
            .Run(true, exactCount: CompsWithCatComma);

    [Fact]
    public void CompanyCategory_ContainsCatHavingComma_Empty()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat,
                $"{CatWithComma}",
                separator:DefSeparator)
            .Run(false);

    [Fact]
    public void CompanyCategory_ContainsAll2_Find() 
        => BuildContainsAll()
            .Run(true, exactCount: CompsWithCat12);

    [Fact]
    public void CompanyCategory_ContainsAll2WithDupls_Find() 
        => BuildContainsAll(repeat2: true)
            .Run(true, exactCount: CompsWithCat12);

    [Fact]
    public void CompanyCategory_ContainsAll3_Find() 
        => BuildContainsAll(true)
            .Run(true, exactCount: CompsWithCat123);

    [Fact]
    public void CompanyCategory_ContainsAll2WrongCount()
    {
        // "this is expected to fail assert-counts, just for testing that the count is actually controlled"
        Throws<EqualException>(() =>
        {
            BuildContainsAll(true)
                .Run(true, exactCount: CompsWithCat12);
        });
    }

    [Fact]
    public void CompanyCategory_NotContainsAll()
    {
        var original = BuildContainsAll();
        original.Run(true);

        var not = BuildContainsAll();
        not.CompareMode = "not-" + not.CompareMode;
        not.Run(true);

        True(original.CountAll == original.CountApi + not.CountApi, 
            "not and default should together have the total failed "
            + $"tot:{original.CountAll}, having2:{original.CountApi}, not-having:{not.CountApi}");
    }

    [Fact]
    public void CompanyCategory_ContainsAllBadSeparator() 
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, 
                $"{CatWeb}{AltSeparator}{CatGreen}", 
                compareMode:RelationshipFilter.CompareModeContains
            )
            .Run(false); // since the separator won't separate, it will not find anything

    [Fact]
    public void CompanyCategory_ContainsAllAltSeparator()
    {
        new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title",
                RelationshipTestSpecs.Company, CompCat,
                $"{CatWeb}{AltSeparator}{CatGreen}",
                compareMode: RelationshipFilter.CompareModeContains,
                separator: AltSeparator)
            .Run(true);
    }

    [Fact]
    public void CompanyCategory_Contains2AndCommaCase()
    {
        new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title",
                RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatWithComma}",
                compareMode: RelationshipFilter.CompareModeContains,
                separator: AltSeparator)
            .Run(true, exactCount: CompsWithCat12Comma);
    }

    [Fact]
    public void CompanyCategory_Contains3AndCommaCase()
    {
        new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-having-title",
                RelationshipTestSpecs.Company, CompCat,
                $"{CatMny1}{AltSeparator}{CatMny2}{AltSeparator}{CatMny3}{AltSeparator}{CatWithComma}",
                compareMode: RelationshipFilter.CompareModeContains,
                separator: AltSeparator)
            .Run(true, exactCount: CompsWithCat123Comma);
    }





    /// <summary>
    ///  Build a contain-ANY basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildContainsAny(bool useThree = false)
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-any",
            RelationshipTestSpecs.Company, CompCat,
            $"{CatMny1}{DefSeparator}{CatMny2}"
            + (useThree ? $"{DefSeparator}{CatMny3}" : ""), // optionally add a third category (for other tests)
            separator: DefSeparator,
            compareMode: RelationshipFilter.CompareModeContainsAny);


    [Fact]
    public void CompanyCategory_ContainsAny1Or2_Find()
        => BuildContainsAny(true)
            .Run(true, exactCount: CompAnyCat1Or2);

    [Fact]
    public void CompanyCategory_ContainsAny1OrSpecial_Find()
    {
        var relf = BuildContainsAny();
        relf.Filter = $"{CatMny1}{DefSeparator}{CatForAnyTest3}";
        relf.Run(true, exactCount: CompAnyCat1AndForAny);
    }




    /// <summary>
    ///  Build a contain-ANY basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildContainsAnything()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
            $"{CatMny1}{DefSeparator}{CatMny2}", // doesn't really matter hat's here, as we only want the none
            separator: DefSeparator,
            compareMode: RelationshipFilter.CompareModeAny);

    [Fact]
    public void CompanyCategory_ContainsAny_FindLots()
    {
        var tst = BuildContainsAnything();
        tst.Run(true);
        True(tst.CountApi > 5, "should get more than 5 hits");
    }

    /// <summary>
    ///  Build a contain-ANY basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildContainsNotAnything()
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
            $"{CatMny1}{DefSeparator}{CatMny2}", // doesn't really matter hat's here, as we only want the none
            separator: DefSeparator,
            compareMode: "not-" + RelationshipFilter.CompareModeAny);


    [Fact]
    public void CompanyCategory_ContainsNone_Find()
        => BuildContainsNotAnything()
            .Run(true, exactCount: 1);


    /// <summary>
    ///  Build a contain-ANY basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildCount(bool useNot, string amount)
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
            $"{amount}", // doesn't really matter hat's here, as we only want the none
            compareMode: (useNot ? "not-" : "") + RelationshipFilter.CompareModeCount);


    [Fact]
    public void CompanyCategory_Count_0_Find1()
        => BuildCount(false, 0.ToString())
            .Run(true, exactCount: 1);

    [Fact]
    public void CompanyCategory_Count_1_Find()
        => BuildCount(false, 1.ToString())
            .Run(true);
    [Fact]
    public void CompanyCategory_Count_2_Find()
        => BuildCount(false, 2.ToString())
            .Run(true);
    [Fact]
    public void CompanyCategory_Count_12_Empty()
        => BuildCount(false, 12.ToString())
            .Run(false);

    [Fact]
    public void CompanyCategory_Count_Not12_Empty()
        => BuildCount(true, 12.ToString())
            .Run(true, shouldReturnAll: true);

    [Fact]
    public void CompanyCategory_Count_InvalidZero_Empty()
        => BuildCount(false, "abc")
            .Run(false);


    /// <summary>
    ///  Build a contain-ANY basic test - but don't run yet
    /// </summary>
    /// <returns></returns>
    private RelationshipTestCase BuildContainsFirst2(bool includeTheAny)
        => new RelationshipTestCase(dsSvc, dataBuilder, "basic-cat-contains-any", RelationshipTestSpecs.Company, CompCat,
            $"{CatMny2}"
            + (includeTheAny ? $"{DefSeparator}{CatForAnyTest3}":""),
            separator: DefSeparator,
            compareMode: RelationshipFilter.CompareModeFirst);


    [Fact]
    public void CompanyCategory_ContainsFirst2_Find()
        => BuildContainsFirst2(false)
            .Run(true, exactCount: 1);
    [Fact]
    public void CompanyCategory_ContainsFirst2AndTheAny_Find()
        => BuildContainsFirst2(true)
            .Run(true, exactCount: 3);

}