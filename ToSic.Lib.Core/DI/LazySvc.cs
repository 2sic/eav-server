using System;
using ToSic.Lib.Helper;
using ToSic.Lib.Logging;

namespace ToSic.Lib.DI
{
    /// <summary>
    /// WIP - should help us create lazy objects which will auto-init if ever used
    /// This should reduce the amount of plumbing in many code files
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazySvc<T>: ILazySvc<T> where T : class
    {
        public LazySvc(Lazy<T> valueLazy) => _valueLazy = valueLazy;
        private readonly Lazy<T> _valueLazy;

        /// <summary>
        /// Set the init-command as needed
        /// </summary>
        /// <param name="newInitCall"></param>
        public LazySvc<T> SetInit(Action<T> newInitCall)
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

        public bool HasInitCall => _initCall != null;

        public T Value => _valueGet.Get(() =>
        {
            var value = _valueLazy.Value;
            _initCall?.Invoke(value);
            InitLogOrNull?.Invoke(value);
            return value;
        });

        //if (_value != null) return _value;
        //_value = _valueLazy.Value;
        //_initCall?.Invoke(_value);
        //InitLogOrNull?.Invoke(_value);
        //return _value;
        public bool IsValueCreated => _valueGet.IsValueCreated;

        //private T _value;
        private readonly GetOnce<T> _valueGet = new GetOnce<T>();

        private Action<T> _initCall;

        protected Action<T> InitLogOrNull;

        void ILazyInitLog.SetLog(ILog parentLog) => InitLogOrNull = thingWithLog => (thingWithLog as IHasLog)?.Init(parentLog);
    }
}
