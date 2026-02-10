using ToSic.Eav.DataSource.Sys.Query;

namespace ToSic.Eav.DataSource.Query;

public class WireTests
{
    [Fact]
    public void WireEquality()
    {
        var wire1 = new QueryWire { From = "source1", Out = "out1", To = "target1", In = "in1" };
        var wire2 = new QueryWire { From = "source1", Out = "out1", To = "target1", In = "in1" };
        Equal(wire1, wire2);
    }
    [Fact]
    public void WireEquality2()
    {
        var wire1 = new QueryWire { From = "source1", Out = "out1", To = "target1", In = "in1" };
        var wire2 = new QueryWire { From = "source1", Out = "out1", To = "target1", In = "in1" };
        True(wire1.Equals(wire2));
    }

    [Fact]
    public void WireInEquality()
    {
        var wire1 = new QueryWire { From = "source1", Out = "out1", To = "target1", In = "in1" };
        var wire2 = new QueryWire { From = "source2", Out = "out1", To = "target1", In = "in1" };
        NotEqual(wire1, wire2);
    }
}
