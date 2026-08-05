using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories.RawFromAnonymousHelper;

public class RawFromAnonymousHelperGeneral
{
    #region Empty Anonymous

    private static IRawEntity RawFromEmpty => new Raw.Sys.RawFromAnonymousHelper(null!).ConvertTac(new { });
        
    [Fact]
    public void RawFromEmpty_Id_ShouldBeZero() => Equal(0, RawFromEmpty.Id);

    [Fact]
    public void RawFromEmpty_Guid_ShouldBeEmpty() => Equal(Guid.Empty, RawFromEmpty.Guid);

    [Fact]
    public void RawFromEmpty_Created_ShouldBeDefaultDateTime() => Equal(default, RawFromEmpty.Created);

    [Fact]
    public void RawFromEmpty_Modified_ShouldBeDefaultDateTime() => Equal(default, RawFromEmpty.Modified);
    
    [Fact]
    public void RawFromEmpty_Values_ShouldBeEmpty() => Empty(RawFromEmpty.Values);

    #endregion

    // Shared Test Constants
    private const int ExpectedId = 123;
    private static readonly Guid ExpectedGuid = new("00000000-0000-0000-0000-000000000011");
    
    #region Ids Only

    // Static converted data
    private static IRawEntity RawWithIds => new Raw.Sys.RawFromAnonymousHelper(null!).ConvertTac(new
    {
        Id = ExpectedId,
        Guid = ExpectedGuid,
    });
    
    // Individual tests
    [Fact]
    public void RawFromAnonymousWithIds_Id_ShouldBeExpected() => Equal(ExpectedId, RawWithIds.Id);
    [Fact]
    public void RawFromAnonymousWithIds_Guid_ShouldBeExpected() => Equal(ExpectedGuid, RawWithIds.Guid);

    [Fact]
    public void RawFromAnonymousWithIds_Created_ShouldBeDefaultDateTime() => Equal(default, RawWithIds.Created);

    [Fact]
    public void RawFromAnonymousWithIds_Modified_ShouldBeDefaultDateTime() => Equal(default, RawWithIds.Modified);

    [Fact]
    public void RawFromAnonymousWithIds_Values_ShouldBeEmpty() => Empty(RawWithIds.Values);

    #endregion

    // Static values
    private static readonly DateTime ExpectedCreated = new(2020, 1, 1);
    private static readonly DateTime ExpectedModified = new(2021, 1, 1);
    private static readonly KeyValuePair<string, object?> ExpectedExtraProperty = new("SomethingElse", "hello");

    #region Raw with only Props

    // Static data
    private static IRawEntity RawWithOnlyProps => new Raw.Sys.RawFromAnonymousHelper(null!).ConvertTac(new
    {
        Id = ExpectedId,
        Guid = ExpectedGuid,
        Created = ExpectedCreated,
        Modified = ExpectedModified,
    });
    
    // Individual tests
    [Fact]
    public void RawWithOnlyProps_Id_ShouldBeExpected() => Equal(ExpectedId, RawWithOnlyProps.Id);
    [Fact]
    public void RawWithOnlyProps_Guid_ShouldBeExpected() => Equal(ExpectedGuid, RawWithOnlyProps.Guid);
    [Fact]
    public void RawWithOnlyProps_Created_ShouldBeExpected() => Equal(ExpectedCreated, RawWithOnlyProps.Created);
    [Fact]
    public void RawWithOnlyProps_Modified_ShouldBeExpected() => Equal(ExpectedModified, RawWithOnlyProps.Modified);
    [Fact]
    public void RawWithOnlyProps_Values_ShouldBeEmpty() => Empty(RawWithOnlyProps.Values);

    #endregion

    #region Raw with all core props and one extra

    // Static data
    private static IRawEntity RawWithAllPropsAndOneExtra => new Raw.Sys.RawFromAnonymousHelper(null!).ConvertTac(new
    {
        Id = ExpectedId,
        Guid = ExpectedGuid,
        Created = ExpectedCreated,
        Modified = ExpectedModified,
        SomethingElse = ExpectedExtraProperty.Value
    });
    
    // Individual tests
    [Fact]
    public void RawWithAllPropsAndOneExtra_Id_ShouldBeExpected() => Equal(ExpectedId, RawWithAllPropsAndOneExtra.Id);
    [Fact]
    public void RawWithAllPropsAndOneExtra_Guid_ShouldBeExpected() => Equal(ExpectedGuid, RawWithAllPropsAndOneExtra.Guid);
    [Fact]
    public void RawWithAllPropsAndOneExtra_Created_ShouldBeExpected() => Equal(ExpectedCreated, RawWithAllPropsAndOneExtra.Created);
    [Fact]
    public void RawWithAllPropsAndOneExtra_Modified_ShouldBeExpected() => Equal(ExpectedModified, RawWithAllPropsAndOneExtra.Modified);
    [Fact]
    public void RawWithAllPropsAndOneExtra_Values_ShouldContainExpectedExtraProperty()
    {
        var values = RawWithAllPropsAndOneExtra.Values;
        NotNull(values);
        Contains(ExpectedExtraProperty, values);
        Single(values);
    }

    #endregion

    #region Raw with only extra

    // Static data
    private static IRawEntity RawWithOnlyExtra => new Raw.Sys.RawFromAnonymousHelper(null!).ConvertTac(new
    {
        SomethingElse = ExpectedExtraProperty.Value
    });

    [Fact]
    public void RawWithOnlyExtra_Id_ShouldBeZero() => Equal(0, RawWithOnlyExtra.Id);

    [Fact]
    public void RawWithOnlyExtra_Guid_ShouldBeEmpty() => Equal(Guid.Empty, RawWithOnlyExtra.Guid);

    [Fact]
    public void RawWithOnlyExtra_Created_ShouldBeDefaultDateTime() => Equal(default, RawWithOnlyExtra.Created);

    [Fact]
    public void RawWithOnlyExtra_Modified_ShouldBeDefaultDateTime() => Equal(default, RawWithOnlyExtra.Modified);

    [Fact]
    public void RawWithOnlyExtra_Values_ShouldContainExpectedExtraProperty()
    {
        var values = RawWithOnlyExtra.Values;
        NotNull(values);
        Contains(ExpectedExtraProperty, values);
        Single(values);
    }


    #endregion
}
