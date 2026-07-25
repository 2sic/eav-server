using ToSic.Eav.Data.Build.DataFactories.MockData;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test the basic case where the data factory will try to auto-convert from raw directly.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemTitleFromRaw(IDataFactory dataFactory)
    : DataFactoryItemTitle
{
    protected override IEntity CreateTestEntity(IRawEntitySource source)
        => dataFactory.CreateTac(source);
}

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemTitleFromConverter(IDataFactory dataFactory)
    : DataFactoryItemTitle
{
    protected override IEntity CreateTestEntity(IRawEntitySource source)
    {
        // Create a fake raw, which doesn't have relevant properties, but would provide the source
        // in the GetConverter...
        var fakeEntity = new MockRawEntityProvidingConversion((IRawEntity)source);
        return dataFactory.CreateTac(fakeEntity);
    }
}

// ReSharper restore UnusedMember.Global


public abstract class DataFactoryItemTitle
{
    /// <summary>
    /// Abstract factory of the test-entity, which is implemented by the final test class.
    /// This is because we'll have some tests which will return the entity generated from raw,
    /// while others return the entity generated from a converter.
    /// </summary>
    protected abstract IEntity CreateTestEntity(IRawEntitySource source);
    
    [Fact]
    public void TitleOfSpecsNone_Empty_IsNull()
    {
        var y = CreateTestEntity(new MockRawEntity());
        Null(y.GetBestTitle());
    }

    private IEntity CreateSpecsNone_WithSingleValue(string key, string value) =>
        CreateTestEntity(new MockRawEntity
        {
            Values = new Dictionary<string, object?>
            {
                { key, value }
            }
        });
    private IEntity CreateSpecs_NameTitle_WithSingleValue(string key, string value) =>
        CreateTestEntity(new MockRawWithNameTitle
        {
            Values = new Dictionary<string, object?>
            {
                { key, value }
            }
        });

    [Fact]
    public void TitleOfSpecsNone_WithTitleValue_IsExpected() =>
        Equal("Expected Title", CreateSpecsNone_WithSingleValue("Title", "Expected Title").GetBestTitle());

    [Fact]
    public void TitleOfSpecsNone_WithNameValue_IsNull() =>
        Null(CreateSpecsNone_WithSingleValue("Name", "Expected Name").GetBestTitle());

    [Fact]
    public void TitleOfSpecsNameTitle_WithTitleValue_IsNull() =>
        Null(CreateSpecs_NameTitle_WithSingleValue("Title", "Expected Title").GetBestTitle());

    [Fact]
    public void TitleOfSpecsNameTitle_WithNameValue_IsExpected() =>
        Equal("Expected Name", CreateSpecs_NameTitle_WithSingleValue("Name", "Expected Name").GetBestTitle());
}
