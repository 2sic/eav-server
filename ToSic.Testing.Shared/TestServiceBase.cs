namespace ToSic.Testing.Shared;

public abstract class TestServiceBase(TestBaseEavDataSource serviceProvider) : IServiceBuilder
{
    protected readonly TestBaseEavDataSource Parent = serviceProvider;

    public T GetService<T>() => Parent.GetService<T>();

}