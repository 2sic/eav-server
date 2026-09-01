using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ToSic.Sys.Utils;

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
    public ResultState Decision
    {
        get => Exceptions.Any() ? ResultState.Error : field;
        init;
    } = ResultState.Default;

    /// <summary>
    /// Collection of problems / exceptions which occured.
    /// </summary>
    public List<Exception> Exceptions
    {
        get;
        init;
    } = [];

    public IImmutableStack<string> History { get; init; } = [];
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class PackageExtensions
{
    public static Package<TData> ToPackage<TData>(
        this TData data,
        [CallerFilePath] string? callerFilePath = null
    ) => new(data) { History = ["Created package"] };

    extension<TData>(Package<TData> package)
    {
        public Package<T> RePackage<T>(T data,
            [CallerFilePath] string? callerFilePath = null
        ) => new(data)
        {
            Decision = package.Decision,
            Exceptions = package.Exceptions,
            History = package.History.Push($"Repackaged from {callerFilePath.After("\\")}")
        };

        public Package<TData> Visited([CallerFilePath] string? callerFilePath = null
        ) => package with
        {
            History = package.History.Push($"Visited from {callerFilePath.After("\\")}")
        };

        public Package<TData> LogSkipped(string? message = default,
            [CallerFilePath] string? callerFilePath = null
        ) => package with
        {
            History = package.History.Push($"Skipped '{message}' in {callerFilePath.After("\\")}")
        };
    }
}