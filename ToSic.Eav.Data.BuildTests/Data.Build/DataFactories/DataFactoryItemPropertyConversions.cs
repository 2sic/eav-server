using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test the basic case where the data factory will try to auto-convert from raw directly.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemPropertyConversionsFromRaw(IDataFactory dataFactory)
    : DataFactoryItemPropertyConversions
{
    protected override IEntity CreateProcess(IConvertibleToRawEntity source)
        => dataFactory.Create(source);
}

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryItemPropertyConversionsFromConverter(IDataFactory dataFactory)
    : DataFactoryItemPropertyConversions
{
    protected override IEntity CreateProcess(IConvertibleToRawEntity source)
    {
        // Create a fake raw, which doesn't have relevant properties, but would provide the source
        // in the GetConverter...
        var fakeEntity = new MockRawEntityProvidingConversion((IRawEntity)source);
        return dataFactory.Create(fakeEntity);
    }
}

// ReSharper restore UnusedMember.Global


public abstract class DataFactoryItemPropertyConversions
{
    protected abstract IEntity CreateProcess(IConvertibleToRawEntity source);
    
    [Fact]
    public void CheckId()
    {
        var x = new MockRawEntityRecord { Id = 17 };
        var y = CreateProcess(x);
        Equal(17, y.EntityId);
    }

    [Fact]
    public void CheckGuid()
    {
        var guid = Guid.NewGuid();
        var x = new MockRawEntityRecord { Guid = guid };
        var y = CreateProcess(x);
        Equal(guid, y.EntityGuid);
    }

    [Fact]
    public void CheckCreated()
    {
        var date = new DateTime(2001, 4, 2);
        var x = new MockRawEntityRecord { Created = date };
        var y = CreateProcess(x);
        Equal(date, y.Created);
        NotEqual(date, y.Modified);
    }
    
    [Fact]
    public void CheckModified()
    {
        var date = new DateTime(2001, 4, 2);
        var x = new MockRawEntityRecord { Modified = date };
        var y = CreateProcess(x);
        Equal(date, y.Modified);
        NotEqual(date, y.Created);
    }
    
    [Fact]
    public void CheckValuesNone()
    {
        var x = new MockRawEntityRecord();
        var y = CreateProcess(x);
        Empty(y.Attributes);
    }
    
    [Fact]
    public void CheckValuesOne()
    {
        var x = new MockRawEntityRecord { Values = new Dictionary<string, object?>
        {
            { "Key", "Value" },
        }};
        var y = CreateProcess(x);
        Single(y.Attributes);
    }
    
    [Fact]
    public void CheckValuesThree()
    {
        var x = new MockRawEntityRecord { Values = new Dictionary<string, object?>
        {
            { "Key", "Value" },
            { "Key2", "Value" },
            { "Key3", "Value" },
        }};
        var y = CreateProcess(x);
        Equal(3, y.Attributes.Count);
    }
}
