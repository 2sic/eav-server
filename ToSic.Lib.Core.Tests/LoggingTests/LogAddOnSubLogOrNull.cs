using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    /// <summary>
    /// Note: you won't see any code, because it inherits all the tests from the base class.
    /// It just has a different way of getting the Log.
    /// </summary>
    [TestClass]
    public class LogAddOnSubLogOrNull : LogAdd
    {

        protected override Log LogFactory(string name = "") => (Log)new Log("").SubLogOrNull("");

        protected override int LogDepth => 1;
    }
}