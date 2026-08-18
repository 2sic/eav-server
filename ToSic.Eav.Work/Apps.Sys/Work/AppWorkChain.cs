namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Generator for work classes which usually require the IAppWorkContext.
/// This is the chained version, for larger work; quick work can use <see cref="AppWorkQuick{TWork}"/>.
/// </summary>
/// <typeparam name="TWork"></typeparam>
/// <param name="sp"></param>
public class AppWorkChain<TWork>(IServiceProvider sp)
    : Generator<TWork, IAppWorkContext>(sp)
    where TWork : IServiceWithSetup<IAppWorkContext>;