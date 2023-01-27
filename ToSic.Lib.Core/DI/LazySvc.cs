using System;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// Helps us create lazy **Service** objects. It has some special features:
    /// 
    /// * It will automatically lazy-attach a logger when used correctly
    /// * It can also be configured with a lazy init function to keep code clean.
    /// 
    /// This reduces the amount of plumbing in many code files.
    /// </summary>
    /// <typeparam name="TService">Service type, ideally based on <see cref="ToSic.Lib.Services.ServiceBase"/></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class LazySvc<TService>: ILazyLike<TService>, ILazyInitLog where TService : class
    {
        /// <summary>
        /// Constructor, should never be called as it's only meant to be used with Dependency Injection.
        /// </summary>
        /// <param name="valueLazy"></param>
        public LazySvc(Lazy<TService> valueLazy) => _valueLazy = valueLazy;
        private readonly Lazy<TService> _valueLazy;

        /// <summary>
        /// Set the init-command as needed
        /// </summary>
        /// <param name="newInitCall"></param>
        public LazySvc<TService> SetInit(Action<TService> newInitCall)
        {
#if DEBUG
            // Warn if we're accidentally replacing init-call, but only do this on debug
            // In most cases it has no consequences, but we should write code that avoids this
            if (_initCall != null)
                throw new Exception($"You tried to call {nameof(SetInit)} twice. This should never happen");
#endif
            _initCall = newInitCall;
            return this;
        }

        public TService Value => _valueGet.Get(() =>
        {
            var value = _valueLazy.Value;
            _initCall?.Invoke(value);
            InitLogOrNull?.Invoke(value);
            return value;
        });
        private readonly GetOnce<TService> _valueGet = new GetOnce<TService>();

        public bool IsValueCreated => _valueGet.IsValueCreated;


        private Action<TService> _initCall;

        protected Action<TService> InitLogOrNull;

        void ILazyInitLog.SetLog(ILog parentLog) => InitLogOrNull = thingWithLog => (thingWithLog as IHasLog)?.LinkLog(parentLog);
    }
}
