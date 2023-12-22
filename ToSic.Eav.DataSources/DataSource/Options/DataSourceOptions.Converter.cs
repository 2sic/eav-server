using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Generics;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using static System.StringComparer;

namespace ToSic.Eav.DataSource;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class DataSourceOptions
{
    /// <summary>
    /// Special converter to take any kind of object and try to turn it into an <see cref="IDataSourceOptions"/>
    /// </summary>
    public class Converter
    {
        public IDataSourceOptions Create(IDataSourceOptions original, object other)
        {
            // other is null
            if (other is null) return original ?? new DataSourceOptions();

            var secondDs = Convert(other, throwIfNull: false, throwIfNoMatch: false);
            if (original is null) return secondDs ?? new DataSourceOptions();

            // Most complex case, both are now "real" - must merge
            return new DataSourceOptions(original,
                appIdentity: secondDs.AppIdentity,
                values: secondDs.Values,
                lookUp: secondDs.LookUp,
                showDrafts: secondDs.ShowDrafts);
        }

        public IDataSourceOptions Convert(object original, bool throwIfNull, bool throwIfNoMatch)
        {
            switch (original)
            {
                case null when throwIfNull: throw new ArgumentNullException(nameof(original));
                case null: return null;
                case IDataSourceOptions ds: return ds;
                case ILookUpEngine lu: return ConvertLookUpEngine(lu);
            }

            // Check if it's a possible value
            var values = Values(original, throwIfNull: false, throwIfNoMatch: false);
            if (values != null)
                return new DataSourceOptions(values: values);

            if (!throwIfNoMatch) return null;
            throw new ArgumentException(
                $"Could not convert {nameof(original)} of type '{original.GetType()}' to {nameof(IDataSource)}. " +
                $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(Converter)}");

        }

        public IDataSourceOptions ConvertLookUpEngine(ILookUpEngine lookUp) => new DataSourceOptions(lookUp: lookUp);

        public IImmutableDictionary<string, string> Values(object original, bool throwIfNull = false, bool throwIfNoMatch = true)
        {
            switch (original)
            {
                case null when throwIfNull: throw new ArgumentNullException(nameof(original));
                case null: return null;
                case IImmutableDictionary<string, string> immutable: return immutable;
                case IDictionary<string, string> dicString: return ValuesFromDictionary(dicString);
                case IDictionary<string, object> dicObj: return ValuesFromDictionary(dicObj);
                case Array arr when arr.Length == 0: return null;
                case Array arr2 when arr2.GetType() == typeof(string[]): return ValuesFromStringArray(arr2 as string[]);
                default:
                    var values = original.IsAnonymous() ? ValuesFromAnonymous(original) : null;
                    if (values != null) return values;
                    if (!throwIfNoMatch) return null;
                    throw new ArgumentException(
                        $"Could not convert {nameof(original)} of type '{original.GetType()}' to {nameof(IDataSource)}. " +
                        $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(Converter)}");
            }
        }
        public IImmutableDictionary<string, string> ValuesFromAnonymous(object original)
        {
            if (original == null || !original.IsAnonymous()) return null;
            return ValuesFromDictionary(original.ToDicInvariantInsensitive());
        }

        public IImmutableDictionary<string, string> ValuesFromDictionary(IDictionary<string, string> original)
            => original?.ToImmutableInvariant();

        public IImmutableDictionary<string, string> ValuesFromDictionary(IDictionary<string, object> original)
            => original?.ToImmutableDictionary(pair => pair.Key, pair => pair.Value?.ToString(),
                InvariantCultureIgnoreCase);

        public IImmutableDictionary<string, string> ValuesFromStringArray(params string[] values)
        {
            var cleaned = values?.Where(v => v.HasValue()).ToList() ?? new List<string>();
            if (cleaned.SafeNone()) return null;
            var valDic = cleaned
                .Select(v => v.Split('='))
                .Where(pair => pair.Length == 2)
                .ToImmutableDictionary(pair => pair[0], pair => pair[1], InvariantCultureIgnoreCase);
            return valDic;
        }

    }
}