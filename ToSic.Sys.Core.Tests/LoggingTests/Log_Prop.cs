using ToSic.Lib.Services;

namespace ToSic.Lib.Core.Tests.LoggingTests;


// ReSharper disable once InconsistentNaming
public class Log_Prop: LogTestBase
{
    public class ObjectWithProperties() : HelperBase(null, "tst.test")
    {
        public string Hello => Log.Getter(() => "hello");

        public string Name
        {
            get => Log.Getter(() => _name);
            // the compiler treats this as Func<string> - with return value
            set => Log.Do(cName: $"set{nameof(Name)}", action: () => _name = value);
        }
        public string NameSetWithBody
        {
            // the compiler treats this as an Action
            set => Log.Do(cName: $"set{nameof(NameSetWithBody)}", action: () => _name = value);
        }
        private string _name = "iJungleboy";
    }

    [Fact]
    public void Prop_Get()
    {
        var o = new ObjectWithProperties();
        var x = o.Hello;
        Equal("hello", x);
        var entries = ((Log)o.Log).Entries;
        Equal(2, entries.Count); //, "should have two entries (start/stop)");
        Equal($"get:{nameof(o.Hello)}", entries[0].Message);
        Equal("hello", entries[0].Result);
    }

    [Fact]
    public void Prop_SetGetSet()
    {
        var result = "John";
        var o = new ObjectWithProperties
        {
            Name = result
        };
        var x = o.Name;
        o.NameSetWithBody = "Jane";
        Equal(result, x);
        var entries = ((Log)o.Log).Entries;
        Equal(6, entries.Count); //, "should have two entries (start/stop)");
        // Check Setter - with result!
        Equal($"set:{nameof(o.Name)}=", entries[0].Message);
        Equal(result, entries[0].Result);
        // Check getter
        Equal($"get:{nameof(o.Name)}", entries[2].Message);
        Equal(result, entries[2].Result);
        // Check Setter - no result as it's a setter with a { } wrapping and so the value-set doesn't bleed back
        Equal($"set:{nameof(o.NameSetWithBody)}", entries[4].Message);
        Equal(null, entries[4].Result);
    }
        
}