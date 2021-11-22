using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    public partial interface IDataSourceConfiguration
    {
#if NET472

        [PrivateApi("just included for compatibility, as previous public examples used Add")]
        [Obsolete("please use the indexer instead - Configuration[key] = value")]
        void Add(string key, string value);

#endif
    }
}
