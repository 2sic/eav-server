using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSource;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceLink : IDataSourceLink
{
    public DataSourceLink(IDataSourceLink original,
        string noParamOrder = Parameters.Protector,
        IDataSource dataSource = default,
        IDataStream stream = default,
        string name = default,
        string outName = default,
        string inName = default,
        IEnumerable<IDataSourceLink> more = default)
    {
        More = more ?? original?.More;
        DataSource = dataSource ?? original?.DataSource;
        Stream = stream ?? original?.Stream;
        OutName = name ?? outName ?? original?.OutName ?? DataSourceConstants.StreamDefaultName;
        InName = name ?? inName ?? original?.InName ?? DataSourceConstants.StreamDefaultName;
    }
    public IDataSource DataSource { get; }
    public string OutName { get; }
    public string InName { get; }
    public IDataStream Stream { get; }
    public IEnumerable<IDataSourceLink> More { get; }
    public IDataSourceLink Rename(string name = default, string outName = default, string inName = default) =>
        // Check if no names provided
        !$"{name}{outName}{inName}".HasValue() ? this : new DataSourceLink(this, name: name, outName: outName, inName: inName);

    public IDataSourceLink AddStream(string name = default, string outName = default, string inName = default) =>
        Add(new DataSourceLink(null, dataSource: DataSource, name: name, inName: inName, outName: outName));

    public IDataSourceLink Add(params IDataSourceLinkable[] more)
    {
        if (more.SafeNone()) return this;
        var newMore = more.Select(m => m.Link);

        // Note: it's important that if we add more sources, 
        // they are added _below_ the current source.
        // This ensures that the main / outer source is the primary
        // which will also provide AppId, Lookups etc.
        if (More.SafeNone()) return new DataSourceLink(this, more: newMore);

        // Current has more and new has more, must merge
        return more.SafeNone() ? this : new DataSourceLink(this, more: newMore.Concat(More));
    }

    public IEnumerable<IDataSourceLink> Flatten(int recursion = 0)
    {
        var list = Enumerable.Empty<IDataSourceLink>();
        if (recursion > 10) return Enumerable.Empty<IDataSourceLink>();
        list = list.Concat(new[] { this });
        if (More.SafeAny()) list = list.Concat(More.SelectMany(m => m.Flatten(recursion + 1)));
        return list;
    }

    public IDataSourceLink Link => this;
}