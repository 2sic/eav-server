using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.Context;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using static System.StringComparer;

namespace ToSic.Eav.DataSource;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceConfiguration : ServiceBase<DataSourceConfiguration.MyServices>, IDataSourceConfiguration
{
    #region Dependencies - Must be in DI

    public class MyServices: MyServicesBase
    {
        public LazySvc<IZoneCultureResolver> ZoneCultureResolverLazy { get; }

        public MyServices(LazySvc<IZoneCultureResolver> zoneCultureResolverLazy)
        {
            ConnectServices(
                ZoneCultureResolverLazy = zoneCultureResolverLazy
            );
        }
    }

    #endregion

    #region Constructor (non DI)

    [PrivateApi]
    public DataSourceConfiguration(MyServices services) : base(services, $"{DataSourceConstants.LogPrefix}.Config")
    {
    }

    internal DataSourceConfiguration Attach(DataSourceBase ds)
    {
        DataSourceForIn = ds;
        return this;
    }


    [PrivateApi] internal DataSourceBase DataSourceForIn;

    #endregion

    private const string ConfigNotFoundMessage = "Trying to get a configuration by name of {0} but it doesn't exist. Did you forget to add to ConfigMask?";

    public string GetThis([CallerMemberName] string name = default) => ParseToken(GetRaw(name));
    public string GetRaw([CallerMemberName] string name = default) => _values.TryGetValue(name, out var result) 
        ? result
        : throw new ArgumentException(string.Format(ConfigNotFoundMessage, name));

    public T GetThis<T>(T fallback, [CallerMemberName] string name = default) => Get<T>(name, fallback: fallback);

    // ReSharper disable once AssignNullToNotNullAttribute
    [Obsolete("This is necessary for older DataSources which hat configuration setters. We will not support that any more, do not use.")]
    internal void SetThisObsolete<T>(T value, [CallerMemberName] string name = default) => _values[name] = value?.ToString();

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
            throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(Parse)}");

        // construct a property access for in, use it in the config provider
        return LookUpEngine.LookUp(values, OverrideLookUps);
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
    private IDictionary<string, ILookUp> OverrideLookUps 
        => _overrideLookUps.Get(() => new Dictionary<string, ILookUp>
        {
            { "In".ToLowerInvariant(), new LookUpInDataTarget(DataSourceForIn, base.Services.ZoneCultureResolverLazy.Value) }
        });
    private readonly GetOnce<IDictionary<string, ILookUp>> _overrideLookUps = new();


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