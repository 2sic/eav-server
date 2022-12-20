using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    /// <summary>
    /// Note: you won't see any code, because it inherits all the tests from the base class.
    /// It just has a different way of getting the Log.
    /// </summary>
    [TestClass]
    public class LogAddOnLogWithChild : LogAdd
    {
        protected override Log LogFactory(string name = "")
        {
            var parent = new Log(name);
            var child = new Log(name, parent);
            return parent;
        }
    }
}