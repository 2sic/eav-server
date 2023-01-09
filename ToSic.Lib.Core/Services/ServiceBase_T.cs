using ToSic.Lib.Logging;

namespace ToSic.Lib.Services
{
    /// <summary>
    /// Base class for any service which expects a Dependencies class
    /// </summary>
    /// <typeparam name="TDeps"></typeparam>
    public abstract class ServiceBase<TDeps>: ServiceBase where TDeps : ServiceDependencies
    {
        protected ServiceBase(TDeps dependencies, string logName) : base(logName)
        {
            Deps = dependencies.SetLog(Log);
        }

        //protected ServiceBase(string logName, CodeRef codeRef) : base(logName, codeRef)
        //{
        //}

        protected readonly TDeps Deps;
    }
}
