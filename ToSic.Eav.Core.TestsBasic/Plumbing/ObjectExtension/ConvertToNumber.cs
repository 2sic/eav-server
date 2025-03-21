﻿

namespace ToSic.Eav.Plumbing.ObjectExtension;


public class ConvertToNumber: ConvertTestBase
{


    [Fact]
    public void StringToInt()
    {
        ConvT(null, 0, 0);
        ConvT("", 0, 0);
        ConvT("5", 5, 5);
        ConvT("5.2", 0, 5);
        ConvT("5.4", 0, 5);
        ConvT("5.5", 0, 6);
        ConvT("5.9", 0, 6);
        ConvT("   5.9", 0, 6);
        ConvT("5.9  ", 0, 6);
        ConvT("   5.9  ", 0, 6);
    }

    [Fact]
    public void StringToIntNull()
    {
        ConvT<int?>(null, null, null);
        ConvT<int?>("", null, null);
        ConvT<int?>("5", 5, 5);
        ConvT<int?>("5.2", null, 5);
        ConvT<int?>("5.4", null, 5);
        ConvT<int?>("5.5", null, 6);
        ConvT<int?>("5.9", null, 6);
    }

    [Fact]
    public void StringToFloat()
    {
        ConvT(null, 0f, 0f);
        ConvT("", 0f, 0f);
        ConvT("5", 5f, 5f);
        ConvT("5.2", 5.2f, 5.2f);
        ConvT("5.9", 5.9f, 5.9f);
        ConvT("-1", -1f, -1f);
        ConvT("-99.7", -99.7f, -99.7f);
    }

    [Fact]
    public void StringToDecimal()
    {
        ConvT(null, 0m, 0m);
        ConvT("", 0m, 0m);
        ConvT("5", 5m, 5m);
        ConvT("5.2", 5.2m, 5.2m);
        ConvT("5.9", 5.9m, 5.9m);
        ConvT("-1", -1m, -1m);
        ConvT("-99.7", -99.7m, -99.7m);
    }

    [Fact]
    public void StringToFloatNull()
    {
        ConvT<float?>(null, null, null);
        ConvT<float?>("", null, null);
        ConvT<float?>("5", 5f, 5f);
        ConvT<float?>("5.2", 5.2f, 5.2f);
        ConvT<float?>("5.9", 5.9f, 5.9f);
        ConvT<float?>("-1", -1f, -1f);
        ConvT<float?>("-99.7", -99.7f, -99.7f);
    }

    [Fact]
    public void StringToDecimalNull()
    {
        ConvT<decimal?>(null, null, null);
        ConvT<decimal?>("", null, null);
        ConvT<decimal?>("5", 5m, 5m);
        ConvT<decimal?>("5.2", 5.2m, 5.2m);
        ConvT<decimal?>("5.9", 5.9m, 5.9m);
        ConvT<decimal?>("-1", -1m, -1m);
        ConvT<decimal?>("-99.7", -99.7m, -99.7m);
    }
}