using ToSic.Lib.Coding;

namespace ToSic.Eav.DataSource;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class DataSourceLink(IDataSourceLink original,
#pragma warning disable CS9113 // Parameter is unread.
    NoParamOrder protect = default,
#pragma warning restore CS9113 // Parameter is unread.
    IDataSource dataSource = default,
    IDataStream stream = default,
    string name = default,
    string outName = default,
    string inName = default,
    IEnumerable<IDataSourceLink> more = default) : IDataSourceLink
{
    public IDataSource DataSource { get; } = dataSource ?? original?.DataSource;
    public string OutName { get; } = name ?? outName ?? original?.OutName ?? DataSourceConstants.StreamDefaultName;
    public string InName { get; } = name ?? inName ?? original?.InName ?? DataSourceConstants.StreamDefaultName;
    public IDataStream Stream { get; } = stream ?? original?.Stream;
    public IEnumerable<IDataSourceLink> More { get; } = more ?? original?.More;

    public IDataSourceLink Rename(string name = default, string outName = default, string inName = default) =>
        // Check if no names provided
        !$"{name}{outName}{inName}".HasValue() ? this : new(this, name: name, outName: outName, inName: inName);

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
        return more.SafeNone() ? this : new(this, more: newMore.Concat(More));
    }

    public IEnumerable<IDataSourceLink> Flatten(int recursion = 0)
    {
        var list = Enumerable.Empty<IDataSourceLink>();
        if (recursion > 10) return [];
        list = list.Concat([this]);
        if (More.SafeAny()) list = list.Concat(More.SelectMany(m => m.Flatten(recursion + 1)));
        return list;
    }

    public IDataSourceLink Link => this;
}