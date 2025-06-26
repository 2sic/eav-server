using ToSic.Eav.Data.Build;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

internal class RelationshipTestCaseFactory(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{
    public RelationshipTestCase New(string name,
        string type,
        string? relationship = null,
        string? filter = null,
        string? relAttribute = null,
        string? compareMode = null,
        string? separator = null,
        string? direction = null) =>
        new(dsSvc, dataBuilder, name, type, relationship, filter, relAttribute, compareMode,
            separator, direction);
}