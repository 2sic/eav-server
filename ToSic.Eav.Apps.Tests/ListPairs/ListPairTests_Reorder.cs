using ToSic.Eav.Apps.Internal.Work;

namespace ToSic.Eav.Apps.Tests.ListPairs;

public partial class ListPairTests
{

    [Fact]
    public void ReorderUnchanged()
    {
        var pair = GenerateListAndReorder([0, 1, 2, 3]);
        AssertLength(pair, 4);
        AssertPositions(pair,0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair,2, null, 103);
        AssertPositions(pair,3, 44, 444);
    }

    [Fact]
    public void Reorder1023()
    {
        var pair = GenerateListAndReorder([1, 0, 2, 3]);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 2, null);
        AssertPositions(pair, 1, 1, 101);
        AssertPositions(pair, 2, null, 103);
        AssertPositions(pair, 3, 44, 444);
    }

    [Fact]
    public void Reorder3201()
    {
        var pair = GenerateListAndReorder([3, 2, 0, 1]);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 44, 444);
        AssertPositions(pair, 1, null, 103);
        AssertPositions(pair, 2, 1, 101);
        AssertPositions(pair, 3, 2, null);
    }

    /// <summary>
    /// This test has too many re-order items but valid indexes
    /// </summary>
    [Fact]
    //[ExpectedException(typeof(Exception))]
    public void ReorderErrorLongerSequence() 
        => Throws<Exception>(() => GenerateListAndReorder([3, 2, 0, 1, 0, 1]));

    [Fact]
    //[ExpectedException(typeof(Exception))]
    public void ReorderErrorShorterSequence() 
        => Throws<Exception>(() => GenerateListAndReorder([3, 2, 0]));

    [Fact]
    //[ExpectedException(typeof(Exception))]
    public void ReorderErrorReUse() 
        => Throws<Exception>(() => GenerateListAndReorder([3, 2, 3, 0]));

    [Fact]
    //[ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ReorderErrorOutOfRange() 
        => Throws<ArgumentOutOfRangeException>(() => GenerateListAndReorder([3, 7, 0, 1]));

    private static CoupledIdLists GenerateListAndReorder(int[] newSequence)
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, 444],
            PName, CName);
        pair.Reorder(newSequence);
        return pair;
    }
}