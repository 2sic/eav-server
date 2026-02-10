namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// Represent a connection which connects two DataSources in a Query
/// </summary>
[PrivateApi("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public struct QueryWire : IEquatable<QueryWire>
{
    internal const string FromField = "From";
    internal const string OutField = "Out";
    internal const string ToField = "To";
    internal const string InField = "In";

    /// <summary>
    /// The DataSource ID which has data, an <see cref="IDataSource"/>
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// The Out-stream name of the Source DataSource
    /// </summary>
    public string Out { get; set; }

    /// <summary>
    /// The DataSource ID which receives data, an <see cref="IDataSource"/>
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The In-stream name on the target DataSource
    /// </summary>
    public string In { get; set; }

    [PrivateApi]
    public override string ToString() => From + ":" + Out + ">" + To + ":" + In;

    public bool Equals(QueryWire other)
    {
        return From == other.From && Out == other.Out && To == other.To && In == other.In;
    }

    public override bool Equals(object? obj)
    {
        return obj is QueryWire other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, Out, To, In);
    }
}