using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Generics;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using static System.StringComparer;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Provide configuration to new data sources.
    /// </summary>
    public class DataSourceConfiguration: IDataSourceConfiguration
    {
        public DataSourceConfiguration(
            IDataSourceConfiguration original = default,
            string noParamOrder = Parameters.Protector,
            IAppIdentity appIdentity = default,
            IDictionary<string, string> values = default,
            ILookUpEngine lookUp = default,
            bool? showDrafts = default)
        {
            ShowDrafts = showDrafts ?? original?.ShowDrafts;
            AppIdentity = appIdentity ?? original?.AppIdentity;
            Values = values ?? original?.Values;
            LookUp = lookUp ?? original?.LookUp;
        }

        public IDictionary<string, string> Values { get; }

        public IAppIdentity AppIdentity { get; }

        public ILookUpEngine LookUp { get; }
        public bool? ShowDrafts { get; }

        public DataSourceConfiguration(IDictionary<string, string> values)
        {
            if (values == null) return;
            Values = values.ToInvariant();
        }

        public DataSourceConfiguration(params string[] values)
        {
            var cleaned = values?.Where(v => v.HasValue()).ToList() ?? new List<string>();
            if (cleaned.SafeNone()) return;
            Values = cleaned
                .Select(v => v.Split('='))
                .Where(pair => pair.Length == 2)
                .ToDictionary(pair => pair[0], pair => pair[1], InvariantCultureIgnoreCase);
        }


    }
}
