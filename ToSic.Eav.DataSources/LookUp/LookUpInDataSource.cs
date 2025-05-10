using ToSic.Eav.Context;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.LookUp;

/// <inheritdoc />
/// <summary>
/// Look up stuff in a DataSource.
/// It will take the first <see cref="IEntity"/> in a source and look up properties/attributes in that. <br/>
/// Normally this is used in Queries, where you want to get a parameter from the <em>In</em> stream.
/// </summary>
/// <param name="dataSource">the data-target, of which it will use the In-Stream</param>
/// <param name="cultureResolver">culture resolver for multi-language scenarios</param>
[PrivateApi("Was PublicApi till v17, previously called LookUpInDataTarget / renamed to LookUpInDataSource")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LookUpInDataSource(IDataSource dataSource, IZoneCultureResolver cultureResolver) : LookUpBase(InStreamName, $"LookUp in DataSource on stream {InStreamName}")
{
    /// <summary>
    /// The stream name to read when finding the entity which is the source of this.
    /// </summary>
    public const string InStreamName = "In";

    private string[] Dimensions => field ??= cultureResolver.SafeLanguagePriorityCodes();


    /// <inheritdoc />
    /// <summary>
    /// Will check if any streams in In matches the requested next key-part and will retrieve the first entity in that stream
    /// to deliver the required sub-key (or even sub-sub-key)
    /// </summary>
    public override string Get(string key, string format)
    {
        // Check if it has sub-keys to see if it's trying to match an inbound stream
        var subTokens = CheckAndGetSubToken(key);
        if (!subTokens.HasSubtoken) return string.Empty;

        // check if this stream exists
        if (!dataSource.In.TryGetValue(subTokens.Source, out var entityStream)) return string.Empty;

        // check if any entities exist in this specific in-stream
        if (!entityStream.List.Any()) return string.Empty;

        // Create an LookUpInEntity based on the first item, return its Get
        var first = entityStream.List.First();
        return new LookUpInEntity("no-name", first, Dimensions).Get(subTokens.Rest, format);

    }
}