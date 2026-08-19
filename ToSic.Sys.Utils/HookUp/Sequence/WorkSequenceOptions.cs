namespace ToSic.Sys.HookUp;

public record WorkSequenceOptions
{
    public bool Sort { get; init; } = false;
    
    public bool SortUnknownFirst { get; init; } = false;
    
    public bool SortByName { get; init; } = false;

    internal List<TWork> Apply<TWork>(IEnumerable<TWork?> original)
    {
        var workSafe = original
            .OfType<TWork>();

        var fallback = SortUnknownFirst ? int.MinValue : int.MaxValue;

        var workList = Sort
            ? workSafe.OrderBy(w =>
                ((w as IWorkSequenceOrder)?.WorkSequenceOrder ?? fallback)
                .ToString("000000000") +
                (SortByName ? w!.GetType().Name : ""))
            : workSafe;
        return workList.ToList();
    }

}