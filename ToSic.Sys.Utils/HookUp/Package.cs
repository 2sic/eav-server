namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record Package<TData> : IPackage
{
    /// <summary>
    /// Separate constructor, just to better find where it's used.
    /// </summary>
    public Package() {}
    
    [SetsRequiredMembers]
    public Package(TData data): this()
        => Data = data;

    public required TData Data { get; init; }
    
    /// <summary>
    /// Decision, not worked out yet, should be able to tell upstream to stop
    /// </summary>
    public DataPreprocessorDecision Decision
    {
        get => Exceptions.Any() ? DataPreprocessorDecision.Error : field;
        init;
    } = DataPreprocessorDecision.Continue;

    /// <summary>
    /// Collection of problems / exceptions which occured.
    /// </summary>
    public List<Exception> Exceptions
    {
        get;
        init;
    } = [];
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class PackageExtensions
{
    public static Package<TData> ToPackage<TData>(this TData data) => new(data);
    public static Package<T> RePackage<TData, T>(this Package<TData> package, T data) => new(data)
    {
        Decision = package.Decision,
        Exceptions = package.Exceptions
    };
}