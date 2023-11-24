using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Provide setup configuration for a new data source.
///
/// This is internal functionality, but we show it in the docs TODO:
/// </summary>
public partial class DataSourceOptions: IDataSourceOptions
{
    /// <summary>
    /// Empty constructor to create new config without anything.
    /// </summary>
    public DataSourceOptions() { }

    public DataSourceOptions(
        IDataSourceOptions original = default,
        string noParamOrder = Parameters.Protector,
        IAppIdentity appIdentity = default,
        IImmutableDictionary<string, string> values = default,
        ILookUpEngine lookUp = default,
        bool? showDrafts = default,
        bool? immutable = default)
    {
        ShowDrafts = showDrafts ?? original?.ShowDrafts;
        AppIdentity = appIdentity ?? original?.AppIdentity;
        _values = values ?? original?.Values;
        LookUp = lookUp ?? original?.LookUp;
        Immutable = immutable ?? original?.Immutable ?? false;
    }

    IImmutableDictionary<string, string> IDataSourceOptions.Values => _values;
    private readonly IImmutableDictionary<string, string> _values;
    public IAppIdentity AppIdentity { get; }
    public ILookUpEngine LookUp { get; }
    public bool? ShowDrafts { get; }
    public bool Immutable { get; }

    #region General Setter - like a constructor

        

    #endregion

    #region Value Setters

    public DataSourceOptions Values(IImmutableDictionary<string, string> values) => new(this, values: values);
        
        
    #endregion

    #region AppIdentitySetters

        

    #endregion

}