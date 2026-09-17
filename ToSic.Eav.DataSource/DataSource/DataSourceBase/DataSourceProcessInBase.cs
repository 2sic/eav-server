namespace ToSic.Eav.DataSource;

/// <summary>
/// Base class for data sources which typically process in - and should first test if in is available.
/// </summary>
/// <remarks>
/// Meant for internal use only, to quickly create data sources without much overhead.
///
/// NOTE: There are probably quite a few data sources which would benefit from less code if they inherit from this.
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class DataSourceProcessInBase : CustomDataSourceAdvanced
{
    /// <summary>
    /// Constructor
    /// </summary>
    protected DataSourceProcessInBase(Dependencies services, string logName, object[]? connect = default) : base(services, logName, connect)
    {
        ProvideOut(GetDefault);
    }

    /// <summary>
    /// Pre-made GetDefault which will check the In to process.
    /// If ok, hand it over to the abstract GetDefault which must be implemented by the inheriting class.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<IEntity> GetDefault()
    {
        var l = Log.Fn<IEnumerable<IEntity>>();

        // Make sure we have an In - otherwise error
        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

        var result = GetDefault(source);

        return l.Return(result);
    }

    /// <summary>
    /// Method to override, returning the metadata entities for the inheriting class.
    /// </summary>
    /// <param name="defaultIn"></param>
    /// <returns></returns>
    protected virtual IEnumerable<IEntity> GetDefault(IImmutableList<IEntity> defaultIn)
        => defaultIn;
}
