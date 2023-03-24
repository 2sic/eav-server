using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources.Linking
{
    /// <summary>
    /// WIP interface to create one or many sources which can be attached when creating a new sources
    /// </summary>
    public interface IDataSourceLinkInfo : IDataSourceLink
    {
        IDataSource DataSource { get; }
        string OutName { get; }
        string InName { get; }
        IDataStream Stream { get; }

        IEnumerable<IDataSourceLinkInfo> More { get; }

        IDataSourceLinkInfo Rename(string name = default, string outName = default, string inName = default);

        IDataSourceLinkInfo Add(params IDataSourceLink[] more);

        [PrivateApi]
        IEnumerable<IDataSourceLinkInfo> Flatten(int recursion = 0);
    }
}
