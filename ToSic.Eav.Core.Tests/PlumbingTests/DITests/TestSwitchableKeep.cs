namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests
{
    public class TestSwitchableKeep: ITestSwitchableService
    {
        internal const string Name = "Keep";

        public string NameId => Name;

        public bool IsViable() => true;

        public int Priority => 10;
    }
}
