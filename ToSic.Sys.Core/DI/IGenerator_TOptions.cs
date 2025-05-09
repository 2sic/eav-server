namespace ToSic.Lib.DI;

public interface IGenerator<out TService, in TOptions>
    where TService : class, INeedsOptions<TOptions>
    where TOptions : class
{
    public TService New(TOptions options);
}