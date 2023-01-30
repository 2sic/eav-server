using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    public interface IQuickDataForBuilder
    {
        Dictionary<string, object> DataForBuilder { get; }
    }
}
