using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Eav.Model;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// The configuration / definition of a query part. The <see cref="QueryDefinition"/> uses a bunch of these together to build a query. 
/// </summary>
[PrivateApi("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record QueryPartDefinition : ModelOfEntityWithLog
{
    public QueryPartDefinition(IEntity? entity,
        string typeIdentifier,
        Type type,
        DataSourceInfo dataSourceInfo,
        ILog parentLog) : base(entity!, parentLog, "DS.QrPart")
    {
        DataSourceTypeIdentifier = typeIdentifier;
        DataSourceType = type;
        DataSourceInfo = dataSourceInfo;
    }

    /// <summary>Content-Type name of the query Content-Type</summary>
    internal static readonly string TypeName = "DataPipelinePart";

    /// <summary>
    /// Information for this part, how it's to be displayed in the visual query.
    /// This is a JSON string containing positioning etc.
    /// </summary>
    public string VisualDesignerData => GetThis("");

    public string? PartAssemblyAndType => GetThis<string>(null);

    /// <summary>
    /// The .net type which the data source has for this part. <br/>
    /// Will automatically resolve old names to new names as specified in the DataSources <see cref="VisualQueryAttribute"/>
    /// </summary>
    public string DataSourceTypeIdentifier { get; }

    public Type DataSourceType { get; }

    public DataSourceInfo DataSourceInfo { get; }
}