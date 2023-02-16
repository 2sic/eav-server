namespace ToSic.Lib.Services
{
    /// <summary>
    /// Base class for any service which expects a Dependencies class
    /// </summary>
    /// <typeparam name="TMyServices"></typeparam>
    public abstract class ServiceBase<TMyServices>: ServiceBase where TMyServices : MyServicesBase
    {
        protected ServiceBase(TMyServices services, string logName) : base(logName)
        {
            Services = services.SetLog(Log);
        }

        protected readonly TMyServices Services;
    }
}
