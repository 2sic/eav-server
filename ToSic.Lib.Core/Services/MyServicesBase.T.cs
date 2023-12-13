using ToSic.Lib.Documentation;

namespace ToSic.Lib.Services;

/// <summary>
/// Special type of MyServices (dependency helpers).
/// This one is used to extend dependencies of a base classes <see cref="MyServicesBase{T}"/>.
/// They must then still have the original Dependencies to get them.
/// </summary>
/// <typeparam name="T"></typeparam>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MyServicesBase<T>(T parentServices) : MyServicesBase
{
    public T ParentServices { get; } = parentServices;

    // Note: don't add these to log queue, as they will be handled by the base class which needs these dependencies
}