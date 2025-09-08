using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;
using ToSic.Eav.TestData;
using Xunit.DependencyInjection;

namespace ToSic.Sys.OData.Tests;

[Startup(typeof(ODataValueFilterTestStartup))]
public class ODataQueryEngineTests
{
    private readonly ODataQueryEngine _engine;
    private readonly DataSourcesTstBuilder _dsBuilder;
    private readonly DataBuilder _dataBuilder;

    public ODataQueryEngineTests(IDataSourcesService dataSourcesService, DataSourcesTstBuilder dsBuilder, DataBuilder dataBuilder)
    {
        _engine = new ODataQueryEngine(dataSourcesService);
        _dsBuilder = dsBuilder;
        _dataBuilder = dataBuilder;
    }

    private DataTable PersonsTable(int count = 200)
        => new DataTablePerson(_dsBuilder, _dataBuilder).Generate(count, useCacheForSpeed: false);

    [Fact]
    public void FilterByCityKeepsExpectedCount()
    {
        var table = PersonsTable();
        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$filter"] = $"{PersonSpecs.FieldCity} eq '{PersonSpecs.City1}'"
        });

        var result = _engine.Execute(table, query);

        Assert.Equal(50, result.Items.Count);
        Assert.All(result.Items, e => Assert.Equal(PersonSpecs.City1, e.Get<string>(PersonSpecs.FieldCity)));
    }

    [Fact]
    public void FilterContainsAndGreaterThan()
    {
        var table = PersonsTable();
        var expected = table.List
            .Where(e =>
                (e.Get<string>(PersonSpecs.FieldCity) ?? string.Empty).Contains(PersonSpecs.City1, StringComparison.OrdinalIgnoreCase)
                && (e.Get<int?>(PersonSpecs.FieldHeight) ?? 0) > 180)
            .ToList();

        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$filter"] = $"contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}') and {PersonSpecs.FieldHeight} gt 180"
        });

        var result = _engine.Execute(table, query);

        Assert.Equal(expected.Count, result.Items.Count);
        Assert.All(result.Items, entity =>
        {
            Assert.Contains(PersonSpecs.City1, entity.Get<string>(PersonSpecs.FieldCity) ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.True(entity.Get<int?>(PersonSpecs.FieldHeight) > 180);
        });
    }

    [Fact]
    public void FilterNotContainsExcludesMatches()
    {
        var table = PersonsTable();
        var expected = table.List
            .Where(e => !(e.Get<string>(PersonSpecs.FieldCity) ?? string.Empty)
                .Contains(PersonSpecs.City1, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$filter"] = $"not contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}')"
        });

        var result = _engine.Execute(table, query);

        Assert.Equal(expected.Count, result.Items.Count);
        Assert.All(result.Items, entity =>
            Assert.DoesNotContain(PersonSpecs.City1, entity.Get<string>(PersonSpecs.FieldCity) ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OrderByHeightDescendingWithTop()
    {
        var table = PersonsTable();
        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$orderby"] = "Height desc",
            ["$top"] = "3"
        });

        var result = _engine.Execute(table, query);
        Assert.Equal(3, result.Items.Count);

        var expected = table.List
            .OrderByDescending(e => e.Get<int?>(PersonSpecs.FieldHeight))
            .Take(3)
            .Select(e => e.EntityId)
            .ToArray();

        Assert.Equal(expected, result.Items.Select(e => e.EntityId).ToArray());
    }

    [Fact]
    public void SkipAndTopReturnExpectedPage()
    {
        var table = PersonsTable();
        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$orderby"] = "EntityId asc",
            ["$skip"] = "10",
            ["$top"] = "5"
        });

        var result = _engine.Execute(table, query);

        var expected = table.List
            .OrderBy(e => e.EntityId)
            .Skip(10)
            .Take(5)
            .Select(e => e.EntityId)
            .ToArray();

        Assert.Equal(expected, result.Items.Select(e => e.EntityId).ToArray());
    }

    [Fact]
    public void SelectProjectsRequestedFields()
    {
        var table = PersonsTable();
        var query = UriQueryParser.Parse(new Dictionary<string, string>
        {
            ["$select"] = "EntityId,City"
        });

        var result = _engine.Execute(table, query);

        Assert.NotEmpty(result.Projection);
        Assert.Equal(result.Items.Count, result.Projection.Count);

        foreach (var (entity, projection) in result.Items.Zip(result.Projection, (entity, projection) => (entity, projection)))
        {
            Assert.Equal(2, projection.Count);
            Assert.Equal(entity.EntityId, Assert.IsType<int>(projection["EntityId"]));
            Assert.Equal(entity.Get<string>(PersonSpecs.FieldCity), projection[PersonSpecs.FieldCity] as string);
        }
    }
}

internal class ODataValueFilterTestStartup : StartupCoreDataSourcesAndTestData
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }
}


