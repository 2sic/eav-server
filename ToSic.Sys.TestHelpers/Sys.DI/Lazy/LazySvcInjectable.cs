namespace ToSic.Sys.DI;

/// <summary>
/// LazySvc - used for testing
/// </summary>
/// <typeparam name="TService"></typeparam>
/// <param name="service"></param>
[PrivateApi("ATM only used in tests")]
public class LazySvcInjectable<TService>(TService service)
    : LazySvc<TService>(null! /* prevent any base class activity or trigger an exception */), ILazyLike<TService>
    where TService : class
{

    /// <summary>
    /// Fake replacement - just provide the value from constructor
    /// </summary>
    public override TService Value => service;

    bool ILazyLike<TService>.IsValueCreated => true;
}
