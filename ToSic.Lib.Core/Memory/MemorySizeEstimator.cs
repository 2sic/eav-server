using System;
using System.Collections;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Lib.Memory;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MemorySizeEstimator(ILog parentLog) : HelperBase(parentLog, "Eav.MemSiz")
{
    public SizeEstimate Estimate(object value)
        => EstimateInternal(value, 5);

    public SizeEstimate EstimateMany(object[] values)
        => values.Aggregate(new SizeEstimate(), (current, value) => current + Estimate(value));

    public SizeEstimate EstimateInternal(object value, int recursion)
    {
        if (value == null) return new(0, 0);
        if (recursion <= 0) return new(0, 0, Error: true);

        var type = value.GetType();
        if (type.IsValueType)
            return new(SizeOfValueType(value), 0);
        if (type.IsEnum)
            return new(4, 0);

        return value switch
        {
            ICanEstimateSize canEstimate => canEstimate.EstimateSize(Log),
            IEnumerable => SizeOfEnumerable(value, recursion),
            _ => new(0, 0, Unknown: true)
        };
    }

    private SizeEstimate SizeOfEnumerable(object value, int recursion)
    {
        if (value is not IEnumerable enumerable)
            return new(0, 0, Unknown: true);

        try
        {
            return (from object item in enumerable select EstimateInternal(item, recursion - 1))
                .Aggregate(new SizeEstimate(0, 0), (current, estimate) => current + estimate);
        }
        catch (Exception)
        {
            return new(0, 0, Error: true);
        }
    }

    private int SizeOfValueType(object value) => value switch
    {
        string str => str.Length,
        byte[] bytes => bytes.Length,
        int _ => 4,
        long _ => 8,
        double _ => 8,
        float _ => 4,
        decimal _ => 16,
        bool _ => 1,
        char _ => 2,
        DateTime _ => 8,
        DateTimeOffset _ => 16,
        TimeSpan _ => 8,
        Guid _ => 16,
        short _ => 2,
        byte _ => 1,
        uint _ => 4,
        ulong _ => 8,
        ushort _ => 2,
        sbyte _ => 1,
        char[] chars => chars.Length * 2,
        int[] ints => ints.Length * 4,
        long[] longs => longs.Length * 8,
        double[] doubles => doubles.Length * 8,
        _ => -1
    };
}