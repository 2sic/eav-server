﻿namespace ToSic.Eav.ListPairsTests;

public partial class ListPairTests
{

    [Fact]
    public void Move0_1()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(0, 1);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 2, null);
        AssertPositions(pair, 1, 1, 101);
        AssertPositions(pair, 2, null, 103);
    }


    [Fact]
    public void Move1_0()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(1, 0);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 2, null);
        AssertPositions(pair, 1, 1, 101);
        AssertPositions(pair, 2, null, 103);
    }

    [Fact]
    public void Move0_0()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(0, 0);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, null, 103);
    }

    [Fact]
    public void Move1_1()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(1, 1);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, null, 103);
    }

    [Fact]
    public void Move0_2()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(0, 2);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 2, null);
        AssertPositions(pair, 1, null, 103);
        AssertPositions(pair, 2, 1, 101);
        AssertPositions(pair, 3, 44, null);
    }

    [Fact]
    public void MoveEndTwo2_3()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, null, null, null],
            PName,
            CName);
        pair.Move(2, 3);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, 44, null);
        AssertPositions(pair, 3, null, 103);
    }

    [Fact]
    public void MoveEndPastEnd()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, 104, null, null],
            PName,
            CName);
        var changes = pair.Move(3, 4);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, null, 103);
        AssertPositions(pair, 3, 44, 104);

        NotNull(changes);
    }

    [Fact]
    public void MoveEndPastEndFar()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, 104, null, null],
            PName,
            CName);
        var changes = pair.Move(3, 9);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, null, 103);
        AssertPositions(pair, 3, 44, 104);

        NotNull(changes);//, "should have changes, because it should move to end");
    }

    [Fact]
    public void MoveOutsideOfRange()
    {
        var pair = CoupledIdLists([1, 2, null, 44],
            [101, null, 103, 104, null, null],
            PName,
            CName);
        var changes = pair.Move(7, 9);
        AssertLength(pair, 4);
        AssertPositions(pair, 0, 1, 101);
        AssertPositions(pair, 1, 2, null);
        AssertPositions(pair, 2, null, 103);
        AssertPositions(pair, 3, 44, 104);

        NotNull(changes);//, "should not have any changes");
        Empty(changes);
    }
}