namespace ToSic.Eav.DataSource.Sys;

/// <summary>
/// This is a simple record to connect a source and target DataSource with the stream names.
/// </summary>
/// <param name="Source"></param>
/// <param name="SourceStream"></param>
/// <param name="Target"></param>
/// <param name="TargetStream"></param>
/// <param name="DirectlyAttachedStream">
/// Temporary safety net - unsure if useful
/// For streams which are attached and MAYBE (for reasons unknown) don't have a proper source.
/// </param>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record DataSourceConnection(IDataSource Source, string SourceStream, IDataSource Target, string TargetStream, IDataStream? DirectlyAttachedStream = default);