﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Run.Basic
{
    public class BasicRuntime: HasLog, IRuntime
    {
        public BasicRuntime() : base("Eav.BscRnt") { }

        public IRuntime Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }


        public IEnumerable<IContentType> LoadGlobalContentTypes() => new List<IContentType>();

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier) => new List<IEntity>();


        public ILog Log { get; } = new Log("empty");

    }
}
