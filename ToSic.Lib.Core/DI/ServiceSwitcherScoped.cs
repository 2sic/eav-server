using System.Collections.Generic;

namespace ToSic.Lib.DI;

/// <summary>
/// Same as the <see cref="ServiceSwitcher{T}"/> but must have another name so we can register it as scoped.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ServiceSwitcherScoped<T> : ServiceSwitcher<T> where T : ISwitchableService
{
    public ServiceSwitcherScoped(IEnumerable<T> allServices) : base(allServices)
    {
    }
}