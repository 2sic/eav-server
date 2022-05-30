﻿using System;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DI
{
    /// <summary>
    /// Enables generating additional objects of a specific type
    /// </summary>
    public class GeneratorLog<T>: IGenerator<T> where T : class, IHasLog
    {
        public GeneratorLog(IServiceProvider sp) => _sp = sp;
        private readonly IServiceProvider _sp;

        public T New
        {
            get
            {
                var created = _sp.Build<T>();
                created.Log.LinkTo(_parentLog);
                return created;
            }
        }

        public GeneratorLog<T> SetLog(ILog parentLog)
        {
            _parentLog = parentLog;
            return this;
        }
        private ILog _parentLog;

    }
}
