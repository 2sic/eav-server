using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Log_Prop: LogTestBase
    {
        public class ObjectWithProperties: HelperBase
        {
            public ObjectWithProperties() : base(null, "tst.test") { }

            public string Hello => Log.Getter(() => "hello");

            public string Name
            {
                get => Log.Getter(() => _name);
                // the compiler treats this as Func<string> - with return value
                set => Log.Setter(() => _name = value);
            }
            public string NameSetWithBody
            {
                // the compiler treats this as an Action
                set => Log.Setter(() => { _name = value; });
            }
            private string _name = "iJungleboy";
        }

        [TestMethod]
        public void Prop_Get()
        {
            var o = new ObjectWithProperties();
            var x = o.Hello;
            AreEqual("hello", x);
            var entries = ((Log)o.Log).Entries;
            AreEqual(2, entries.Count, "should have two entries (start/stop)");
            AreEqual($"get:{nameof(o.Hello)}", entries[0].Message);
            AreEqual("hello", entries[0].Result);
        }

        [TestMethod]
        public void Prop_SetGetSet()
        {
            var result = "John";
            var o = new ObjectWithProperties
            {
                Name = result
            };
            var x = o.Name;
            o.NameSetWithBody = "Jane";
            AreEqual(result, x);
            var entries = ((Log)o.Log).Entries;
            AreEqual(6, entries.Count, "should have two entries (start/stop)");
            // Check Setter - with result!
            AreEqual($"set:{nameof(o.Name)}=", entries[0].Message);
            AreEqual(result, entries[0].Result);
            // Check getter
            AreEqual($"get:{nameof(o.Name)}", entries[2].Message);
            AreEqual(result, entries[2].Result);
            // Check Setter - no result as it's a setter with a { } wrapping and so the value-set doesn't bleed back
            AreEqual($"set:{nameof(o.NameSetWithBody)}", entries[4].Message);
            AreEqual(null, entries[4].Result);
        }
        
    }
}
