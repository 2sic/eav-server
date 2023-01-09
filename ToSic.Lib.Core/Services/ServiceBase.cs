using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services
{
    /// <summary>
    /// Special base class for most services which have a bunch of child-services that should be log-connected.
    ///
    /// Provides a special Dependency-list and will auto-set the log for all if they support logs.
    /// </summary>
    [PrivateApi]
    public abstract class ServiceBase: IHasLog
    {
        [PrivateApi]
        protected ServiceBase(string logName)
        {
            Log = new Log(logName);
        }

        /// <inheritdoc />
        [JsonIgnore]
        [IgnoreDataMember]
        public ILog Log { get; }

        /// <summary>
        /// Add Log to all dependencies listed in <see cref="services"/>
        /// </summary>
        /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
        [PrivateApi]
        protected void ConnectServices(params object[] services) => (this as IHasLog).ConnectServices(services);
    }
}
