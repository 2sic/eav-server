﻿using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// The configuration / definition of a query part. The <see cref="QueryDefinition"/> uses a bunch of these together to build a query. 
/// </summary>
[PrivateApi("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public class QueryPartDefinition(
    IEntity? entity,
    string typeIdentifier,
    Type type,
    DataSourceInfo dataSourceInfo,
    ILog parentLog)
    : EntityBasedWithLog(entity!, parentLog, "DS.QrPart")
{
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
    public string DataSourceTypeIdentifier { get; } = typeIdentifier;

    public Type DataSourceType { get; } = type;

    public DataSourceInfo DataSourceInfo { get; } = dataSourceInfo;
}