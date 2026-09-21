namespace ToSic.Sys.Logging;

public class LogFactorySelectorTests
{
    [Fact]
    public void UsesLegacyUntilSelected_AndAllowsSameFactoryAgain()
    {
        var legacy = new TestFactory();
        var selected = new TestFactory();
        var selector = new LogFactorySelector(legacy);

        Same(legacy, selector.Current);
        selector.Select(selected);
        selector.Select(selected);

        Same(selected, selector.Current);
    }

    [Fact]
    public void RejectsDifferentFactory()
    {
        var selector = new LogFactorySelector(new TestFactory());
        selector.Select(new TestFactory());

        Throws<InvalidOperationException>(() => selector.Select(new TestFactory()));
    }

    [Fact]
    public void UsesParentsFactoryBeforeGlobalSelection()
    {
        var legacy = new TestFactory();
        var parentFactory = new TestFactory();
        var selector = new LogFactorySelector(legacy);
        selector.Select(new TestFactory());

        Same(parentFactory, selector.For(new TestLog(parentFactory)));
        Same(legacy, selector.For(new Log("legacy")));
    }

    private sealed class TestFactory : ILogFactory
    {
        public ILog Create(string name, ILog? parent, CodeRef code, string? initialMessage = default) => new TestLog(this);
    }

    private sealed class TestLog(ILogFactory factory) : ILog, ILogFactoryOwner
    {
        public ILogFactory Factory { get; } = factory;
        public string NameId => "test";
    }
}
