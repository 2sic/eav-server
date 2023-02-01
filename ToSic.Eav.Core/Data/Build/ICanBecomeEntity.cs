using System;
using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    public interface ICanBecomeEntity
    {
        int Id { get; }

        Guid Guid { get; }

        DateTime Created { get; }
        DateTime Modified { get; }

        Dictionary<string, object> DataForBuilder { get; }
    }
}
