using ToSic.Eav.Data.Build.DataFactories.MockData;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test the basic case where the data factory will try to auto-convert from raw directly.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemPropertiesFromRaw(IDataFactory dataFactory)
    : DataFactoryItemProperties
{
    protected override IEntity CreateTestEntity(IRawEntitySource source)
        => dataFactory.CreateTac(source);
}

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemPropertiesFromConverter(IDataFactory dataFactory)
    : DataFactoryItemProperties
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


public abstract class DataFactoryItemProperties
{
    /// <summary>
    /// Abstract factory of the test-entity, which is implemented by the final test class.
    /// This is because we'll have some tests which will return the entity generated from raw,
    /// while others return the entity generated from a converter.
    /// </summary>
    protected abstract IEntity CreateTestEntity(IRawEntitySource source);
    
    [Fact]
    public void CheckId()
    {
        var x = new MockRawEntity { Id = 17 };
        var y = CreateTestEntity(x);
        Equal(17, y.EntityId);
    }

    [Fact]
    public void CheckGuid()
    {
        var guid = Guid.NewGuid();
        var x = new MockRawEntity { Guid = guid };
        var y = CreateTestEntity(x);
        Equal(guid, y.EntityGuid);
    }

    [Fact]
    public void CheckCreated()
    {
        var date = new DateTime(2001, 4, 2);
        var x = new MockRawEntity { Created = date };
        var y = CreateTestEntity(x);
        Equal(date, y.Created);
        NotEqual(date, y.Modified);
    }
    
    [Fact]
    public void CheckModified()
    {
        var date = new DateTime(2001, 4, 2);
        var x = new MockRawEntity { Modified = date };
        var y = CreateTestEntity(x);
        Equal(date, y.Modified);
        NotEqual(date, y.Created);
    }
    
    [Fact]
    public void CheckTitleFieldName()
    {
        var x = new MockRawEntity();
        var y = CreateTestEntity(x);
        Equal(null, y.GetBestTitle());
    }
    


    [Fact]
    public void CheckValuesNone()
    {
        var x = new MockRawEntity();
        var y = CreateTestEntity(x);
        Empty(y.Attributes);
    }
    
    [Fact]
    public void CheckValuesOne()
    {
        var x = new MockRawEntity { Values = new Dictionary<string, object?>
        {
            { "Key", "Value" },
        }};
        var y = CreateTestEntity(x);
        Single(y.Attributes);
    }
    
    [Fact]
    public void CheckValuesThree()
    {
        var x = new MockRawEntity { Values = new Dictionary<string, object?>
        {
            { "Key", "Value" },
            { "Key2", "Value" },
            { "Key3", "Value" },
        }};
        var y = CreateTestEntity(x);
        Equal(3, y.Attributes.Count);
    }
}
