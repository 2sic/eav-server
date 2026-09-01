namespace ToSic.Sys.HookUp;

/// <summary>
/// Run a sequence of work steps which always return the same type of data as is passed in.
/// </summary>
/// <remarks>
/// This interface is for running a sequence of work steps in a manual order, without any sorting applied.
/// The works are executed in the order they are provided, and each work step processes the data and returns an updated package.
///
/// For running steps in a sorted order, use <see cref="IWorkSequence{TWork,TData}"/>.
/// 
/// The sequence can be stopped or skipped based on the decision returned by each work step.
/// </remarks>
/// <typeparam name="TWork">The type of work to be executed in the sequence.</typeparam>
/// <typeparam name="TData">The type of data being processed by the work sequence.</typeparam>
public interface IWorkSequenceManual<TWork, TData>
    : IWork<TData>
    where TWork : class, IWork<TData, TData>;