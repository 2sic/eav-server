namespace ToSic.Sys.HookUp;

/// <summary>
/// Run a sequence of work steps which always return the same type of data as is passed in.
/// </summary>
/// <remarks>
/// The default implementation will usually retrieve the steps from dependency injection,
/// and sort them according to <see cref="IWorkSequenceOrder.WorkSequenceOrder"/> and A-Z.
///
/// For running steps in a manual order, use <see cref="IWorkSequenceManual{TWork,TData}"/>.
///
/// The sequence can be stopped or skipped based on the decision returned by each work step.
/// </remarks>
/// <typeparam name="TWork">The type of work to be executed in the sequence.</typeparam>
/// <typeparam name="TData">The type of data being processed by the work sequence.</typeparam>
public interface IWorkSequence<TWork, TData>
    : IWorkSequenceManual<TWork, TData>
    where TWork : class, IWork<TData, TData>;