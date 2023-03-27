using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// WIP interface to create one or many sources which can be attached when creating a new sources
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IDataSourceLink : IDataSourceLinkable
    {
        IDataSource DataSource { get; }
        string OutName { get; }
        string InName { get; }
        IDataStream Stream { get; }

        IEnumerable<IDataSourceLink> More { get; }

        IDataSourceLink Rename(string name = default, string outName = default, string inName = default);

        IDataSourceLink Add(params IDataSourceLinkable[] more);

        [PrivateApi]
        IEnumerable<IDataSourceLink> Flatten(int recursion = 0);
    }
}
