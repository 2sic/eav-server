// 2020-07-31 2dm - never used
//using System;

//namespace ToSic.Eav.Plumbing
//{
//    /// <summary>
//    /// Should help with things that must be initialized onec - but never used yet!! 
//    /// warning 2dm: not used yet, not tested!
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public class SingleInitNullable<T>
//    {
//        public bool Initialized { get; private set; }

//        public T Result
//        {
//            get
//            {
//                if (!Initialized && _result != null)
//                    _result = _initializer.Invoke();
//                Initialized = true;
//                return _result;
//            }
//        }

//        private T _result;

//        private readonly Func<T> _initializer;

//        public SingleInitNullable(Func<T> initializer)
//        {
//            _initializer = initializer;
//        }
//    }
//}
