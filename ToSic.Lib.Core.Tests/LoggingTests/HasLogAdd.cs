using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    /// <summary>
    /// Note: you won't see any code, because it inherits all the tests from the base class.
    /// It just has a different way of getting the Log - from a HasLog.
    /// </summary>
    [TestClass]
    public class HasLogAdd: LogAdd
    {
        protected class ThingWithLog: HasLog
        {
            public ThingWithLog() : base("") { }
        }

        protected override Log LogFactory(string name = "") => (Log)new ThingWithLog().Log;
    }
}