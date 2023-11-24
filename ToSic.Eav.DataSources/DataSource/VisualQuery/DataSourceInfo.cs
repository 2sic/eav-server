using System;
using ToSic.Eav.DataSources;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSource.VisualQuery;

public class DataSourceInfo: TypeWithMetadataBase<VisualQueryAttribute>
{
        
    public VisualQueryAttribute VisualQuery { get; }

    // By default the name is the global name of the VisualQuery.
    public override string Name => VisualQuery?.NameId;

    public bool IsGlobal { get; }

    public string TypeName { get; }

    /// <summary>
    /// Error object WIP
    /// </summary>
    public DataSourceInfoError ErrorOrNull { get; }

    public DataSourceInfo(Type dsType, bool isGlobal, string overrideTypeName = default, VisualQueryAttribute overrideVisualQuery = null, DataSourceInfoError error = default) : base(dsType)
    {
        IsGlobal = isGlobal;
        TypeName = overrideTypeName ?? dsType.Name;
        VisualQuery = overrideVisualQuery ?? TypeMetadata;
        ErrorOrNull = error;
    }

    public static DataSourceInfo CreateError(string typeName, bool isGlobal, DataSourceType type, DataSourceInfoError error)
    {
        var vq = new VisualQueryAttribute
        {
            NameId = typeName,
            Type = type,
            DynamicOut = true,
            DynamicIn = true,
            Icon = "warning",
        };
        return new DataSourceInfo(typeof(Error), isGlobal, typeName, vq, error);
    }
}


public class DataSourceInfoError
{
    public DataSourceInfoError(string title, string message)
    {
        Title = title;
        Message = message;
    }
    public string Title { get; set; }
    public string Message { get; set; }
}