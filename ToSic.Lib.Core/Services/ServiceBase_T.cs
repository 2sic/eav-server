namespace ToSic.Lib.Services
{
    /// <summary>
    /// Base class for any service which expects a Dependencies class
    /// </summary>
    /// <typeparam name="TDeps"></typeparam>
    public abstract class ServiceBase<TDeps>: ServiceBase where TDeps : ServiceDependencies
    {
        protected ServiceBase(TDeps services, string logName) : base(logName)
        {
            Services = services.SetLog(Log);
        }

        protected readonly TDeps Services;
    }
}
