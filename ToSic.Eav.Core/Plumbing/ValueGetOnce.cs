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
    public class ValueGetOnce<T>
    {
        public T Get(Func<T> generator)
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
            return _value = generator();
        }

        /// <summary>
        /// Determines if value has been created. Name the same as when using Lazy()
        /// </summary>
        public bool IsValueCreated;
        private T _value;
    }
}
