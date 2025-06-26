namespace ToSic.Eav.ListPairsTests;

public partial class ListPairTests
{

    [Fact]
    public void RemoveAt0()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Remove(0);
        AssertLength(pair, 3);
        AssertPositions(pair, 0, 2, null);
        //Assert.Equal(pair.PrimaryIds.First(), 2);
        //Assert.Equal(pair.CoupledIds.First(), null);
    }

    [Fact]
    public void RemoveAt1()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Remove(1);
        AssertLength(pair, 3);
        AssertPositions(pair, 0, 1, 101);
        //Assert.Equal(pair.PrimaryIds.First(), 1);
        //Assert.Equal(pair.CoupledIds.First(), 101);
        AssertPositions(pair, 1, null, 103);
        //Assert.Equal(pair.PrimaryIds[1], null);
        //Assert.Equal(pair.CoupledIds[1], 103);
    }

    [Fact]
    public void RemoveAtEnd()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, 104, null, null],
            PName,
            CName);
        pair.Remove(3);
        AssertLength(pair, 3);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, pair.Lists.First().Value.Count() -1, null, 103);
        //Assert.Equal(pair.PrimaryIds.First(), 1);
        //Assert.Equal(pair.CoupledIds.First(), 101);
        //Assert.Equal(pair.PrimaryIds.Last(), null);
        //Assert.Equal(pair.CoupledIds.Last(), 103);
    }


    [Fact]
    public void RemoveAfterEnd() => RemoveAtEndOrBeyond(4);

    [Fact]
    public void RemoveAfterEndFarBehind() => RemoveAtEndOrBeyond(9);

    private void RemoveAtEndOrBeyond(int addPosition)
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Remove(addPosition);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 3, 3, null);
        //Assert.Equal(pair.PrimaryIds.First(), 1);
        //Assert.Equal(pair.CoupledIds.First(), 101);
        //Assert.Equal(pair.PrimaryIds[1], 2);
        //Assert.Equal(pair.CoupledIds[1], null);
        //Assert.Equal(pair.PrimaryIds[3], 3);
        //Assert.Equal(pair.CoupledIds[3], null);
    }

}