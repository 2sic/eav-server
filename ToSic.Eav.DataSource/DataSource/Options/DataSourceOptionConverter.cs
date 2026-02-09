using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Special converter to take any kind of object and try to turn it into an <see cref="IDataSourceOptions"/>
/// </summary>
public class DataSourceOptionConverter
{
    public DataSourceOptions Create(IDataSourceOptions? original, object? other)
    {
        // other is null
        if (other is null)
            return original as DataSourceOptions ?? new DataSourceOptions
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
            };

        var secondDs = Convert(other, throwIfNull: false, throwIfNoMatch: false);
        if (original is not DataSourceOptions typed)
            return secondDs ?? new DataSourceOptions
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
            };

        // Most complex case, both are now "real" - must merge
        return typed with
        {
            AppIdentityOrReader = secondDs?.AppIdentityOrReader ?? typed.AppIdentityOrReader,
            MyConfigValues = secondDs?.MyConfigValues ?? typed.MyConfigValues,
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
            case ILookUpEngine lu: return new()
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
                LookUp = lu,
            };
        }

        // Check if it's a possible value
        var values = TryGetParams(original, throwIfNull: false, throwIfNoMatch: false);
        if (values != null)
            return new()
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
                MyConfigValues = values,
            };

        if (!throwIfNoMatch) return null;
        throw new ArgumentException(
            $"Could not convert {nameof(original)} of type '{original.GetType()}' to {nameof(IDataSource)}. " +
            $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(DataSourceOptionConverter)}");

    }

    public IImmutableDictionary<string, string>? TryGetParams(object? dsParams, bool throwIfNull = false, bool throwIfNoMatch = true) =>
        dsParams switch
        {
            null when throwIfNull => throw new ArgumentNullException(nameof(dsParams)),
            null => null,
            IImmutableDictionary<string, string> immutable => immutable,
            IDictionary<string, string> dicString => dicString.ToImmutableInvIgnoreCase(),
            IDictionary<string, object?> dicObj => dicObj.ToDicStringStringImInv(),
            Array { Length: 0 } => null,
            Array arr2 when arr2.GetType() == typeof(string[]) => (arr2 as string[]).ValuePairsToDicImInv(preferNullToEmpty: true),
            IDataSourceParameters typed => typed.ToDicInvariantInsensitive().ToDicStringStringImInv(),
            not null when dsParams.IsAnonymous() => dsParams.ToDicInvariantInsensitive().ToDicStringStringImInv(),
            _ => !throwIfNoMatch
                ? null
                : throw new ArgumentException(
                    $"Could not convert {nameof(dsParams)} of type '{dsParams.GetType()}' to {nameof(IDataSource)}. " +
                    $"{nameof(Convert)} only accepts object types as specified by the conversion methods of {nameof(DataSourceOptionConverter)}")
        };
}
