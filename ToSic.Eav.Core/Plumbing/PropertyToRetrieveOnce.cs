using System;

namespace ToSic.Eav.Plumbing
{
    /// <summary>
    /// Simple helper class to use on object properties which should be generated once.
    /// 
    /// Important for properties which can also return null, because then checking for null won't work to determine if we already tried to retrieve it.
    ///
    /// ATM used in the ResponsiveBase, but we should also use it in other places where we have a second toggle to determine if it had been retrieved already. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyToRetrieveOnce<T>
    {
        public T Get(Func<T> generator)
        {
            if (_retrieved) return _value;
            _retrieved = true;
            return _value = generator();
        }

        private bool _retrieved;
        private T _value;
    }
}
