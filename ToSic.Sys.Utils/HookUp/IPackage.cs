namespace ToSic.Sys.HookUp;

/// <summary>
/// Package without generic type - not sure if we need / keep this.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPackage
{
    ResultState Decision { get; init; }
    List<Exception> Exceptions { get; init; }
}