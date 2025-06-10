namespace ToSic.Eav.DataSources.Internal;

/// <summary>
/// Base class for Children and Parents - since they share a lot of code
/// </summary>
public abstract class MetadataDataSourceBase : CustomDataSourceAdvanced
{
    /// <remarks>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </remarks>
    public abstract string ContentTypeName { get; }


    /// <summary>
    /// Constructor
    /// </summary>
    protected MetadataDataSourceBase(MyServices services, string logName, object[] connect = default) : base(services, logName, connect)
    {
        ProvideOut(GetMetadata);
    }

    private IImmutableList<IEntity> GetMetadata()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Make sure we have an In - otherwise error
        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var typeName = ContentTypeName;
        if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
        l.A($"Content Type Name: {typeName}");

        var relationships = SpecificGet(source, typeName);

        return l.ReturnAsOk(relationships.ToImmutableOpt());
    }

    protected abstract IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName);
}