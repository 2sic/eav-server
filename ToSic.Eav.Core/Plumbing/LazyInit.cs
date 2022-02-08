using System;

namespace ToSic.Eav.Plumbing
{
    /// <summary>
    /// WIP - should help us create lazy objects which will auto-init if ever used
    /// This should reduce the amount of plumbing in many code files
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyInit<T> where T : class
    {
        public LazyInit(Lazy<T> valueLazy) => _valueLazy = valueLazy;
        private readonly Lazy<T> _valueLazy;

        /// <summary>
        /// Set the init-command as needed
        /// </summary>
        /// <param name="newInitCall"></param>
        public LazyInit<T> SetInit(Action<T> newInitCall)
        {
            _initCall = newInitCall;
            return this;
        }

        public T Ready
        {
            get
            {
                if (_value != null) return _value;
                _value = _valueLazy.Value;
                _initCall?.Invoke(_value);
                return _value;
            }
        }
        private T _value;

        private Action<T> _initCall;
    }
}
