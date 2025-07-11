﻿using System.Text.Json.Serialization;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.Sys.Streams;

namespace ToSic.Eav.DataSource.Sys.Inspect;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class InspectStream
{
    public Guid Target;
    public Guid Source;
    public string? SourceOut { get; }
    public string? TargetIn { get; }
    public int Count => Stream?.List.Count() ?? 0;
        
        
    public bool Error;

    /// <summary>
    /// This object contains error data - usually as an IEntity. 
    /// </summary>
    /// <remarks>
    /// Before sending in a web-api it must be converted, but the converter is not available in the DataSources project, so it must be handled at API level
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? /*IDictionary<string, object>?*/ ErrorData;
        
    [JsonIgnore]
    protected readonly IDataStream? Stream;

    public InspectStream(IDataStream stream, IDataSource target, string inName)
    {
        try
        {
            Stream = stream;
            Target = target.Guid; // ?? Guid.Empty; // note 2025-06-16 2dm, removed null check, monitor
            Source = stream.Source.Guid;
            TargetIn = inName;
            if (stream is ConnectionStream conStream1)
                SourceOut = conStream1.Connection?.SourceStream;
            else
                foreach (var outStm in stream.Source.Out)
                    if (outStm.Value == stream)
                        SourceOut = outStm.Key;

            var firstItem = Stream.List?.FirstOrDefault();
            Error = firstItem?.Type?.Name == DataConstants.ErrorTypeName;
            if (Error)
                ErrorData = firstItem;
        }
        catch
        {
            Error = true;
        }
    }


    public bool Equals(InspectStream comparison) =>
        comparison.Target == Target
        && comparison.Source == Source
        && comparison.TargetIn == TargetIn
        && comparison.SourceOut == SourceOut;
}