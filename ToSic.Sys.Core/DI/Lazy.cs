namespace ToSic.Lib.DI;

/// <summary>
/// Enables lazy requesting of objects - won't be available until needed.
/// This is a classic plain-vanilla implementation of Lazy for ServiceProviders.
///
/// Note that most code in the ToSic Namespace will prefer <see cref="LazySvc{TService}"/>
/// </summary>
/// <typeparam name="TService"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyImplementation<TService>(IServiceProvider sp) : Lazy<TService>(sp.Build<TService>);