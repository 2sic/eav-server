using System;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// The configuration / definition of a query part. The <see cref="QueryDefinition"/> uses a bunch of these together to build a query. 
/// </summary>
[PrivateApi("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryPartDefinition: EntityBasedWithLog
{
    [PrivateApi]
    public QueryPartDefinition(IEntity entity, string typeIdentifier, Type type, DataSourceInfo dataSourceInfo, ILog parentLog) : base(entity, parentLog, "DS.QrPart")
    {
        DataSourceTypeIdentifier = typeIdentifier;
        DataSourceType = type;
        DataSourceInfo = dataSourceInfo;
    }

    /// <summary>
    /// Information for this part, how it's to be displayed in the visual query.
    /// This is a JSON string containing positioning etc.
    /// </summary>
    public string VisualDesignerData => Get(QueryConstants.VisualDesignerData, "");

    /// <summary>
    /// The .net type which the data source has for this part. <br/>
    /// Will automatically resolve old names to new names as specified in the DataSources <see cref="VisualQueryAttribute"/>
    /// </summary>
    public string DataSourceTypeIdentifier { get; }

    public Type DataSourceType { get; }
    public DataSourceInfo DataSourceInfo { get; }
}