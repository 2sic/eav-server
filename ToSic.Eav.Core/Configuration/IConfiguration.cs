using System.Collections.Generic;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Configuration
{
    public interface IConfiguration
    {
        ILookUpEngine GetLookupEngineWip();

        IDictionary<string, string> GetValuesWip();
    }
}
