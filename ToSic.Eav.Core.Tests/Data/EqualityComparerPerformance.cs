using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.Data;

/// <summary>
/// Test which kind of comparison is faster.
/// - Simple int == comparison
/// - using a function to compare
/// - using a function with a compare-function as parameter
/// - using the generic EqualityComparer - which is the slowest but ATM the default for attribute-value find
/// </summary>
[TestClass]
public class EqualityComparerPerformance
{
    private const int Iterations = 1000000;

    private static bool IsDefault(int value) => value == default;

    private static bool IsDefaultFn<T>(T value, Func<T, bool> compare) => compare(value);


    [TestMethod]
    public void TestPerformanceForNumberEqualsDefault()
    {
        // start timer
        var sw = Stopwatch.StartNew();
        var ok = false;
        for (var i = 0; i < Iterations; i++)
            ok = i == default;
        sw.Stop();
        Trace.WriteLine($"Time: {sw.ElapsedMilliseconds}ms / {sw.ElapsedTicks} Ticks, ok: {ok}");
    }

    [TestMethod]
    public void TestPerformanceForNumberEqualsDefaultWithFn()
    {
        // start timer
        var sw = Stopwatch.StartNew();
        var ok = false;
        for (var i = 0; i < Iterations; i++)
            ok = IsDefault(i);
        sw.Stop();
        Trace.WriteLine($"Time: {sw.ElapsedMilliseconds}ms / {sw.ElapsedTicks} Ticks, ok: {ok}");
    }

    [TestMethod]
    public void TestPerformanceForNumberEqualsDefaultWithFnInParam()
    {
        // start timer
        var sw = Stopwatch.StartNew();
        var ok = false;
        for (var i = 0; i < Iterations; i++)
            ok = IsDefaultFn(i, IsDefault);
        sw.Stop();
        Trace.WriteLine($"Time: {sw.ElapsedMilliseconds}ms / {sw.ElapsedTicks} Ticks, ok: {ok}");
    }

    [TestMethod]
    public void TestPerformanceForNumberEqualsDefaultWithGeneric()
    {
        // start timer
        var sw = Stopwatch.StartNew();
        var ok = false;
        for (var i = 0; i < Iterations; i++)
            ok = EqualityComparer<int>.Default.Equals(i, default);
        sw.Stop();
        Trace.WriteLine($"Time: {sw.ElapsedMilliseconds}ms / {sw.ElapsedTicks} Ticks, ok: {ok}");
    }

}