using ToSic.Eav.Data.Processing;

namespace ToSic.Eav.DataSources.Sys;

[VisualQuery(
    NiceName = "Data Processors",
    NameId = "717d9406-08e9-48de-b985-a0f40905dc65",
    NameIds = ["System.DataProcessors"], // Internal name for the system, used in some entity-pickers. Can change at any time.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "Data Processors in this system")]
// ReSharper disable once UnusedMember.Global
public class DataProcessors(CustomDataSource.Dependencies services, LazySvc<IEnumerable<IDataProcessor>> generators)
    : RegisteredClasses<IDataProcessor>(services, generators);