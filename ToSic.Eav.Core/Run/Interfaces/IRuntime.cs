﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    public interface IRuntime: IHasLog<IRuntime>
    {

        IEnumerable<IContentType> LoadGlobalContentTypes();

        IEnumerable<Data.IEntity> LoadGlobalItems(string groupIdentifier);

    }
}
