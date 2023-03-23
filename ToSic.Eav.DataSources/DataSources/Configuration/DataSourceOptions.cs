using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
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
            bool? showDrafts = default)
        {
            ShowDrafts = showDrafts ?? original?.ShowDrafts;
            AppIdentity = appIdentity ?? original?.AppIdentity;
            _values = values ?? original?.Values;
            LookUp = lookUp ?? original?.LookUp;
        }

        IImmutableDictionary<string, string> IDataSourceOptions.Values => _values;
        private readonly IImmutableDictionary<string, string> _values;
        public IAppIdentity AppIdentity { get; }
        public ILookUpEngine LookUp { get; }
        public bool? ShowDrafts { get; }

        #region General Setter - like a constructor

        

        #endregion

        #region Value Setters

        public DataSourceOptions Values(IImmutableDictionary<string, string> values)
            => new DataSourceOptions(this, values: values);
        
        
        #endregion

        #region AppIdentitySetters

        

        #endregion

    }
}
