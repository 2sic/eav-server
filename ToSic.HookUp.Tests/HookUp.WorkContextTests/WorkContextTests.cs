using ToSic.Sys.HookUp;

namespace ToSic.HookUp.WorkContextTests;

public class WorkContextTests
{
    [Fact]
    public void WorkContext_Empty()
    {
        var context = new WorkContext();
        NotNull(context);
        Empty(context.Items);
    }

    [Theory]
    [InlineData("key", "value")]
    [InlineData("anotherKey", "anotherValue")]
    public void WorkContext_WithAddOne(string key, string value)
    {
        var context = new WorkContext();
        var updatedContext = context.With(key, value);
        NotNull(updatedContext);
        Single(updatedContext.Items);
        Equal(value, updatedContext.Get<string>(key));
    }
    
    [Fact]
    public void WorkContext_WithCreatesCopy()
    {
        var context = new WorkContext();
        var updatedContext = context.With("key", "value");
        NotEqual(context, updatedContext);
        NotEqual(context.Items, updatedContext.Items);
    }
    
    [Fact]
    public void WorkContext_With2xCreatesCopy()
    {
        var context = new WorkContext();
        var updatedContext = context.With("key", "value");
        var reUpdated = updatedContext.With("key", "value");
        NotEqual(context, updatedContext);
        NotEqual(context.Items, updatedContext.Items);
        NotEqual(updatedContext, reUpdated);
        // The items are "Equal" but not the same object
        NotSame(updatedContext.Items, reUpdated.Items);
    }

    [Fact]
    public void WorkContext_WithDictionaryWorks()
    {
        var context = new WorkContext();
        var data = new Dictionary<string, object?>
        {
            ["key1"] = "value1"
        };
        var updatedContext = context.With(data);
        NotNull(updatedContext);
        Single(updatedContext.Items);
        Equal("value1", updatedContext.Get<string>("key1"));
    }
}
