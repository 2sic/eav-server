namespace ToSic.Sys.HookUp;

/// <inheritdoc cref="IWorkSequence{TWork,TData}"/>
internal class WorkSequence<TWork, TData>(IEnumerable<TWork> works)
    : WorkSequenceBase<TWork, TData>(works, () => new() { Sort = true, SortByName = true }),
        IWorkSequence<TWork, TData>
    where TWork : class, IWork<TData, TData>;


/// <inheritdoc cref="IWorkSequenceManual{TWork,TData}"/>
internal class WorkSequenceManual<TWork, TData>(IEnumerable<TWork> works)
    : WorkSequenceBase<TWork, TData>(works, () => new()),
        IWorkSequenceManual<TWork, TData>
    where TWork : class, IWork<TData, TData>;


/// <summary>
/// The shared implementation.
/// </summary>
/// <param name="works"></param>
/// <param name="getDefaultOptions"></param>
internal class WorkSequenceBase<TWork, TData>(IEnumerable<TWork> works, Func<WorkSequenceOptions> getDefaultOptions)
    : ServiceBase("Sec.Process")
    where TWork : class, IWork<TData, TData>
{
    public async Task<Package<TData>> Handle(WorkContext context, Package<TData> package)
    {
        if (works == null!)
            return Log.Quick(() => package, message: "Error: works is null");

        var workList = works.ToList();
        var l = Log.Fn<Package<TData>>($"For {workList.Count} works");

        // Exit early if no work is found
        if (!workList.Any())
            return l.Return(package, "no work found");

        // Determine options, check if we must sort
        var options = context.TryGet<WorkSequenceOptions>()
                      ?? getDefaultOptions();

        workList = options.Apply(workList);

        // Run all the work actions
        var payload = package;
        foreach (var work in workList)
        {
            try
            {
                var updated = await work.Handle(new(), payload);
                switch (updated.Decision)
                {
                    // Stop, exit now
                    case ResultState.StopSequence:
                        return l.Return(updated, "stop decision");

                    // Error, exit now
                    case ResultState.Error:
                        return l.Return(updated, "error decision");

                    // Skip: ignore this work, but continue with the next one
                    case ResultState.Skip:
                        payload = payload.LogSkipped($"Work {work.GetType().Name} skipped");
                        continue;

                    // Default: update the payload and continue with the next work
                    case ResultState.Default:
                    default:
                        payload = updated;
                        continue;
                }
            }
            catch (Exception ex)
            {
                l.Ex(message: $"Error running processor for work {work.GetType().Name}", ex);
                var errPackage = payload with { Decision = ResultState.Error, Exceptions = [ex] };
                l.ReturnAsError(errPackage);
            }
        }

        return l.Return(payload);
    }
}