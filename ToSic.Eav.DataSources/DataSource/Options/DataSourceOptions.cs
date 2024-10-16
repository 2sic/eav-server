﻿using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;
using ToSic.Lib.Coding;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Provide setup configuration for a new data source.
///
/// This is internal functionality
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class DataSourceOptions: IDataSourceOptions
{
    /// <summary>
    /// Empty constructor to create new config without anything.
    /// </summary>
    public DataSourceOptions() { }

    public DataSourceOptions(
        IDataSourceOptions original = default,
        NoParamOrder noParamOrder = default,
        IAppIdentity appIdentity = default,
        IImmutableDictionary<string, string> values = default,
        ILookUpEngine lookUp = default,
        bool? showDrafts = default,
        bool? immutable = default)
    {
        ShowDrafts = showDrafts ?? original?.ShowDrafts;
        AppIdentityOrReader = appIdentity ?? original?.AppIdentityOrReader;
        _values = values ?? original?.Values;
        LookUp = lookUp ?? original?.LookUp;
        Immutable = immutable ?? original?.Immutable ?? false;
    }

    IImmutableDictionary<string, string> IDataSourceOptions.Values => _values;
    private readonly IImmutableDictionary<string, string> _values;
    public IAppIdentity AppIdentityOrReader { get; }
    public ILookUpEngine LookUp { get; }
    public bool? ShowDrafts { get; }
    public bool Immutable { get; }


    #region Value Setters

    public DataSourceOptions Values(IImmutableDictionary<string, string> values) => new(this, values: values);
        
        
    #endregion

}