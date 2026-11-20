namespace ToSic.Sys.DI;

public sealed record KeyedService<TKey, TService>(TKey Key, TService Service)
    where TService : notnull;
