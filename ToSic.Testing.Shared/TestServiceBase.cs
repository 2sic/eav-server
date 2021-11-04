namespace ToSic.Testing.Shared
{
    public abstract class TestServiceBase: IServiceBuilder
    {
        private readonly IServiceBuilder _serviceProvider;

        protected TestServiceBase(IServiceBuilder serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Build<T>() => _serviceProvider.Build<T>();

    }
}
