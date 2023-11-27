using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.DI;

/// <summary>
/// Enables lazy requesting of objects - won't be available until needed.
/// This is a classic plain-vanilla implementation of Lazy for ServiceProviders.
///
/// Note that most code in the ToSic Namespace will prefer <see cref="LazySvc{TService}"/>
/// </summary>
/// <typeparam name="TService"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LazyImplementation<TService> : Lazy<TService>
{
    public LazyImplementation(IServiceProvider sp) : base(sp.Build<TService>)
    {
    }

}