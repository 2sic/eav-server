using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

internal class RelationshipTestCaseFactory(DataSourcesTstBuilder dsSvc, DataAssembler dataAssembler, ContentTypeAssembler typeAssembler)
{
    public RelationshipTestCase New(string name,
        string type,
        string? relationship = null,
        string? filter = null,
        string? relAttribute = null,
        string? compareMode = null,
        string? separator = null,
        string? direction = null) =>
        new(dsSvc, dataAssembler, typeAssembler, name, type, relationship, filter, relAttribute, compareMode,
            separator, direction);
}