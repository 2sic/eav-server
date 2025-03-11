namespace ToSic.Eav.Apps.Tests;

public partial class ListPairTests
{

    [TestMethod]
    public void AddAt0()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Add(0, [999,777]);
        AssertLength(pair, 5);
        AssertPositions(pair,0, 999, 777);
        AssertPositions(pair, 1, 1, 101);
        AssertPositions(pair,2, 2, null);
    }

    [TestMethod]
    public void AddAt1()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Add(1, [999, 777]);
        AssertLength(pair, 5);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 999, 777);
        AssertPositions(pair, 2, 2, null);
    }

    [TestMethod]
    public void AddAtEnd()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Add(3, [999, 777]);
        AssertLength(pair, 5);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 3, 999, 777);
    }


    [TestMethod]
    public void AddToEnd() => AddAtEndOrBeyond(4);

    [TestMethod]
    public void AddToEndFarBehind() => AddAtEndOrBeyond(9);

    private void AddAtEndOrBeyond(int addPosition)
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName, CName);
        pair.Add(addPosition, [999, 777]);
        AssertLength(pair, 5);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 3, 3, null);
        AssertPositions(pair, 4, 999, 777);
    }

}