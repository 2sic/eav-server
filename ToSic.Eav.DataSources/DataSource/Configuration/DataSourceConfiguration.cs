using System.Runtime.CompilerServices;
using ToSic.Eav.Context;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;
using ToSic.Lib.LookUp;
using ToSic.Lib.LookUp.Engines;
using static System.StringComparer;

namespace ToSic.Eav.DataSource;

[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
internal class DataSourceConfiguration(DataSourceConfiguration.MyServices services)
    : ServiceBase<DataSourceConfiguration.MyServices>(services, $"{DataSourceConstantsInternal.LogPrefix}.Config"),
        IDataSourceConfiguration
{
    #region Dependencies - Must be in DI

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public class MyServices(LazySvc<IZoneCultureResolver> zoneCultureResolverLazy)
        : MyServicesBase(connect: [zoneCultureResolverLazy])
    {
        public LazySvc<IZoneCultureResolver> ZoneCultureResolverLazy { get; } = zoneCultureResolverLazy;
    }

    #endregion

    #region Constructor

    internal DataSourceConfiguration Attach(DataSourceBase inDataSource)
    {
        DataSourceForIn = inDataSource;
        return this;
    }


    internal DataSourceBase DataSourceForIn;

    #endregion

    private const string ConfigNotFoundMessage = "Trying to get a configuration by name of {0} but it doesn't exist. Did you forget to add to ConfigMask?";

    public string GetThis([CallerMemberName] string name = default)
    {
        if (name == null || name.IsEmptyOrWs())
            return null;
        if (_getThisCache.TryGetValue(name, out var result))
            return result;

        // on first call, get and log
        var l = Log.Fn<string>($"{DataSourceForIn.GetType().Name}:{name}");
        var raw = GetRaw(name);
        var parsed = ParseToken(raw);
        _getThisCache[name] = parsed;
        return l.Return(parsed, raw == parsed ? $"'{raw}'" : $"raw: '{raw}'; parsed: '{parsed}'");
    }
    private readonly Dictionary<string, string> _getThisCache = [];

    /// <summary>
    /// Get the raw token or throw an error, since it should only be used on things which have a [Configuration] attribute.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private string GetRaw(string name = default) => _values.TryGetValue(name, out var result) 
        ? result
        : throw new ArgumentException(string.Format(ConfigNotFoundMessage, name));

    public T GetThis<T>(T fallback, [CallerMemberName] string name = default) => Get(name, fallback: fallback);

    // ReSharper disable once AssignNullToNotNullAttribute
    [Obsolete("This is necessary for older DataSources which hat configuration setters. We will not support that any more, do not use.")]
    internal void SetThisObsolete<T>(T value, [CallerMemberName] string name = default)
    {
        // Set the value in the dictionary
        _values[name] = value?.ToString();

        // Flush the quick get-this cache, otherwise it will still return the old value
        _getThisCache.Clear();
    }

    [PrivateApi]
    public IReadOnlyDictionary<string, string> Values => _values as IReadOnlyDictionary<string, string>;
    private IDictionary<string, string> _values = new Dictionary<string, string>(InvariantCultureIgnoreCase);

    public ILookUpEngine LookUpEngine { get; protected internal set; }

    [PrivateApi]
    public bool IsParsed { get; private set; }

    /// <summary>
    /// Make sure that configuration-parameters have been parsed (tokens resolved)
    /// but do it only once (for performance reasons)
    /// </summary>
    [PrivateApi]
    public void Parse()
    {
        if (IsParsed) return;
        _values = Parse(_values);
        IsParsed = true;
    }

    /// <summary>
    /// Make sure that configuration-parameters have been parsed (tokens resolved)
    /// but do it only once (for performance reasons)
    /// </summary>
    [PrivateApi]
    public IDictionary<string, string> Parse(IDictionary<string, string> values)
    {
        // Ensure that we have a configuration-provider (not always the case, but required)
        if (LookUpEngine == null)
            throw new($"No LookUpEngine configured on this data-source. Cannot run {nameof(Parse)}");

        // construct a property access for in, use it in the config provider
        return LookUpEngine.LookUp(values, overrides: OverrideLookUps);
    }

    private string Parse(string name)
    {
        if (name == null) return null;
        if (!Values.TryGetValue(name, out var token) || !token.HasValue()) return token;
        return ParseToken(token);
    }

    private string ParseToken(string token)
    {
        var list = new Dictionary<string, string>(InvariantCultureIgnoreCase) { { "0", token } };
        var parsed = Parse(list);
        return parsed["0"];
    }

    /// <summary>
    /// An internally created lookup to give access to the In-streams if there are any
    /// </summary>
    [PrivateApi]
    private IEnumerable<ILookUp> OverrideLookUps => field ??=
    [
        new LookUpInDataSource(DataSourceForIn, Services.ZoneCultureResolverLazy.Value)
    ];


    public string Get(string name) => Parse(name);

    public TValue Get<TValue>(string name) => Parse(name).ConvertOrDefault<TValue>();

    public TValue Get<TValue>(string name, NoParamOrder noParamOrder = default, TValue fallback = default) =>
        Parse(name).ConvertOrFallback(fallback);


    internal void AddIfMissing(string name, string value)
    {
        if (_values.ContainsKey(name)) return;
        _values[name] = value;
    }

    internal void AddMany(IDictionary<string, string> values)
    {
        if (values == null) return;
        foreach (var pair in values) _values[pair.Key] = pair.Value;
    }
}