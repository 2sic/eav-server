namespace ToSic.Sys.DI;

/// <summary>
/// Internal value transporter for the Ctx for setting the value in the override.
/// </summary>
/// <typeparam name="TService"></typeparam>
internal sealed class CtxValue<TService>() where TService : class
{
    internal CtxValue(TService value): this()
    {
        IsSet = true;
        Value = value;
    }

    public bool IsSet { get; init; }

    public TService Value { get; } = null!;
}
