namespace ToSic.Lib.Services
{
    /// <summary>
    /// Base class for any service which expects a Dependencies class
    /// </summary>
    /// <typeparam name="TMyServices"></typeparam>
    public abstract class ServiceBase<TMyServices>: ServiceBase where TMyServices : MyServicesBase
    {
        /// <summary>
        /// Constructor for normal case, with services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="logName"></param>
        protected ServiceBase(TMyServices services, string logName) : base(logName)
        {
            Services = services.SetLog(Log);
        }

        /// <summary>
        /// Constructor for passing on service dependencies which are extended by a inheriting dependencies.
        /// </summary>
        /// <param name="extendedServices"></param>
        /// <param name="logName"></param>
        protected ServiceBase(MyServicesBase<TMyServices> extendedServices, string logName) : this(extendedServices.ParentServices, logName)
        {
            // Ensure the extended copy also has SetLog run
            extendedServices.SetLog(Log);
        }

        protected readonly TMyServices Services;
    }
}
