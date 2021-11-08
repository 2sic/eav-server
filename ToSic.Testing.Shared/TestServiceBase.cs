namespace ToSic.Testing.Shared
{
    public abstract class TestServiceBase: IServiceBuilder
    {
        protected readonly IServiceBuilder Parent;

        protected TestServiceBase(IServiceBuilder serviceProvider)
        {
            Parent = serviceProvider;
        }

        public T Build<T>() => Parent.Build<T>();

    }
}
