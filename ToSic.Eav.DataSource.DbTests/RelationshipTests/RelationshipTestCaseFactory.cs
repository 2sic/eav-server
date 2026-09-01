using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

internal class RelationshipTestCaseFactory(DataSourcesTstBuilder dsSvc, DataAssembler dataAssembler, ContentTypeAssemblyKit ctAssemblyKit)
{
    public RelationshipTestCase New(string name,
        string type,
        string? relationship = null,
        string? filter = null,
        string? relAttribute = null,
        string? compareMode = null,
        string? separator = null,
        string? direction = null) =>
        new(dsSvc, dataAssembler, ctAssemblyKit, name, type, relationship, filter, relAttribute, compareMode,
            separator, direction);
}