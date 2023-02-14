using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    public partial interface IDataSourceConfiguration
    {
#if NETFRAMEWORK

        // 2022-02-14 2dm disabled, as all DataSources will need recompiling - Remove 2023 Q2

        //[PrivateApi("just included for compatibility, as previous public examples used Add")]
        //[Obsolete("please use the indexer instead - Configuration[key] = value")]
        //void Add(string key, string value);

#endif
    }
}
