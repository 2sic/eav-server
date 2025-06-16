﻿using ToSic.Lib.LookUp.Engines;
using static System.StringComparer;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Special converter to take any kind of object and try to turn it into an <see cref="IDataSourceOptions"/>
/// </summary>
public class DataSourceOptionConverter
{
    public DataSourceOptions Create(IDataSourceOptions original, object? other)
    {
        // other is null
        if (other is null)
            return original as DataSourceOptions ?? new DataSourceOptions();

        var secondDs = Convert(other, throwIfNull: false, throwIfNoMatch: false);
        if (original is not DataSourceOptions typed)
            return secondDs ?? new DataSourceOptions();

        // Most complex case, both are now "real" - must merge
        return typed with
        {
            AppIdentityOrReader = secondDs?.AppIdentityOrReader ?? typed.AppIdentityOrReader,
            Values = secondDs?.Values ?? typed.Values,
            LookUp = secondDs?.LookUp ?? typed.LookUp,
            ShowDrafts = secondDs?.ShowDrafts ?? typed.ShowDrafts
        };
    }

    public DataSourceOptions? Convert(object original, bool throwIfNull, bool throwIfNoMatch)
    {
        switch (original)
        {
            case null when throwIfNull: throw new ArgumentNullException(nameof(original));
            case null: return null;
            case DataSourceOptions dss: return dss;
            //case IDataSourceOptions ds: return ds;
            case ILookUpEngine lu: return new() { LookUp = lu };
        }

        // Check if it's a possible value
        var values = Values(original, throwIfNull: false, throwIfNoMatch: false);
        if (values != null)
            return new() { Values = values };

        if (!throwIfNoMatch) return null;
        throw new ArgumentException(
            $"Could not convert {nameof(original)} of type '{original.GetType()}' to {nameof(IDataSource)}. " +
            $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(DataSourceOptionConverter)}");

    }

    public IImmutableDictionary<string, string>? Values(object original, bool throwIfNull = false, bool throwIfNoMatch = true)
    {
        switch (original)
        {
            case null when throwIfNull
                : throw new ArgumentNullException(nameof(original));
            case null:
                return null;
            case IImmutableDictionary<string, string> immutable:
                return immutable;
            case IDictionary<string, string> dicString:
                return ValuesFromDictionary(dicString);
            case IDictionary<string, object> dicObj:
                return ValuesFromDictionary(dicObj!);
            case Array { Length: 0 }:
                return null;
            case Array arr2 when arr2.GetType() == typeof(string[]):
                return ValuesFromStringArray(arr2 as string[]);
            default:
                var values = original.IsAnonymous()
                    ? ValuesFromAnonymous(original)
                    : null;
                if (values != null)
                    return values;
                if (!throwIfNoMatch)
                    return null;
                throw new ArgumentException(
                    $"Could not convert {nameof(original)} of type '{original.GetType()}' to {nameof(IDataSource)}. " +
                    $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(DataSourceOptionConverter)}");
        }
    }

    public IImmutableDictionary<string, string>? ValuesFromAnonymous(object? original)
    {
        if (original == null || !original.IsAnonymous())
            return null;
        return ValuesFromDictionary(original.ToDicInvariantInsensitive());
    }

    public IImmutableDictionary<string, string>? ValuesFromDictionary(IDictionary<string, string>? original)
        => original?.ToImmutableInvIgnoreCase();

    [return: NotNullIfNotNull(nameof(original))]
    public IImmutableDictionary<string, string>? ValuesFromDictionary(IDictionary<string, object?>? original)
        => original
            ?.Where(pair => pair.Value != null)
            .ToImmutableDictionary(
                pair => pair.Key,
                pair => pair.Value!.ToString()!,
                InvariantCultureIgnoreCase
            );

    public IImmutableDictionary<string, string>? ValuesFromStringArray(params string[]? values)
    {
        var cleaned = values
                          ?.Where(v => v.HasValue())
                          .ToList()
                      ?? [];
        if (cleaned.SafeNone())
            return null;
        var valDic = cleaned
            .Select(v => v.Split('='))
            .Where(pair => pair.Length == 2)
            .ToImmutableDictionary(pair => pair[0], pair => pair[1], InvariantCultureIgnoreCase);
        return valDic;
    }

}
