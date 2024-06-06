namespace ToSic.Testing.Shared;

public abstract class TestServiceBase: IServiceBuilder
{
    protected readonly TestBaseEavDataSource Parent;

    protected TestServiceBase(TestBaseEavDataSource serviceProvider)
    {
        Parent = serviceProvider;
    }

    public T GetService<T>() => Parent.GetService<T>();

}