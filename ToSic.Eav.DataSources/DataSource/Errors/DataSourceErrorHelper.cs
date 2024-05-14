using ToSic.Eav.Data.Build;
using ToSic.Lib.Coding;

namespace ToSic.Eav.DataSource;

/// <summary>
/// An Errors-helper which is automatically available on all <see cref="DataSourceBase"/> objects.
///
/// It helps create a stream of standardized error entities.
/// </summary>
/// <remarks>
/// Constructor - to find out if it's used anywhere
/// </remarks>
[PublicApi]
public class DataSourceErrorHelper(DataBuilder builder)
{
    [PrivateApi]
    internal DataSourceErrorHelper Link(IDataSource source)
    {
        _source = source;
        return this;
    }

    private IDataSource _source;


    /// <summary>
    /// Create a stream containing an error entity.
    /// </summary>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="title">Error title</param>
    /// <param name="message">Error message</param>
    /// <param name="exception">Exception (if there was an exception)</param>
    /// <param name="source">The DataSource which created this error. If provided, will allow the message to contain more details.</param>
    /// <param name="streamName">The stream name. If provided, will allow the message to contain more details.</param>
    /// <returns></returns>
    public IImmutableList<IEntity> Create(
        NoParamOrder noParamOrder = default,
        string title = default, 
        string message = default,
        Exception exception = default,
        IDataSource source = default, 
        string streamName = DataSourceConstants.StreamDefaultName
    )
    {
        source?.Log?.Ex(exception);

        // Construct the IEntity and return as Immutable
        var entity = CreateErrorEntity(source, streamName, title, message);
        return new[] { entity }.ToImmutableList();
    }

    /// <summary>
    /// Create a stream of items showing a detailed error why an In stream was not found.
    /// </summary>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="source"></param>
    /// <param name="name">Name of the stream.</param>
    /// <returns></returns>
    /// <remarks>
    /// Added v16.00
    /// </remarks>
    public IImmutableList<IEntity> TryGetInFailed(NoParamOrder noParamOrder = default, IDataSource source = default, string name = DataSourceConstants.StreamDefaultName) 
        => TryGetFailed(source, true, name);

    /// <summary>
    /// Create a stream of items showing a detailed error why an Out stream was not found.
    /// </summary>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="source"></param>
    /// <param name="name">Name of the stream.</param>
    /// <returns></returns>
    /// <remarks>
    /// Added v16.01
    /// </remarks>
    public IImmutableList<IEntity> TryGetOutFailed(NoParamOrder noParamOrder = default, IDataSource source = default, string name = DataSourceConstants.StreamDefaultName) 
        => TryGetFailed(source, false, name);

    [PrivateApi]
    private IImmutableList<IEntity> TryGetFailed(IDataSource source, bool inStreams, string streamName)
    {
        source = source ?? _source;
        var inOrOut = inStreams ? source.In : source.Out;
        var partName = inStreams ? "In" : "Out";
        if (!inOrOut.ContainsKey(streamName))
            return Create(source: source, title: $"Stream '{streamName}' not found",
                message: $"This DataSource needs the stream '{streamName}' on the {partName} to work, but it couldn't find it.");
        var stream = inOrOut[streamName];
        if (stream == null)
            return Create(source: source, title: $"Stream '{streamName}' is Null", message: $"The Stream '{streamName}' was found on {partName}, but it's null");
        var list = stream.List?.ToImmutableList();
        if (list == null)
            return Create(source: source, title: $"Stream '{streamName}' is Null",
                message: $"The Stream '{streamName}' exists on {partName}, but the List is null");
        return null;
    }


    [PrivateApi("usually not needed externally")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
    {
        var values = new Dictionary<string, object>
        {
            { DataConstants.ErrorFieldTitle, GenerateTitle(title) },
            { "SourceName", source?.Name },
            { "SourceLabel", source?.Label },
            { "SourceGuid", source?.Guid },
            { "SourceType", source?.GetType().Name },
            { "SourceStream", stream },
            { DataConstants.ErrorFieldMessage, message },
            { DataConstants.ErrorFieldDebugNotes, DataConstants.ErrorDebugMessage }
        };

        // #DebugDataSource
        // When debugging I usually want to see where this happens. Feel free to comment in/out as needed
        // System.Diagnostics.Debugger.Break();

        // Don't use the default data builder here, as it needs DI and this object
        // will often be created late when DI is already destroyed
        var errorEntity = builder.Entity.Create(appId: DataConstants.ErrorAppId, entityId: DataConstants.ErrorEntityId,
            contentType: builder.ContentType.Transient(DataConstants.ErrorTypeName),
            attributes: builder.Attribute.Create(values),
            titleField: DataConstants.ErrorFieldTitle);
        return errorEntity;
    }

    /// <summary>
    /// This must be internal so it can be used/verified in testing
    /// </summary>
    [PrivateApi("only internal for testing")]
    internal static string GenerateTitle(string title) => "Error: " + title;

}