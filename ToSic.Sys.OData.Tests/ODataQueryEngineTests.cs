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
public class ODataQueryEngineTests(
    IDataSourcesService dataSourcesService,
    DataSourcesTstBuilder dsBuilder,
    DataBuilder dataBuilder)
{
    private readonly ODataQueryEngine _engine = new(dataSourcesService);

    private DataTable PersonsTable(int count = 200, PersonSpecs? specs = null)
        => new DataTablePerson(dsBuilder, dataBuilder)
            .Generate(count, useCacheForSpeed: false, specs: specs);

    private const string CityToFind = "Bern";
    private const string CitiesWithB = "Buchs,Bern";
    private const string CitiesWithZ = "Zurich,Zug";

    private static readonly PersonSpecs PersonInCities = new()
    {
        TestCities = ["Buchs", "Zurich", "Geneva", "Zug", CityToFind],
    };

    private QueryExecutionResult FilterPrepareAndRun(int count, string filter)
    {
        var table = PersonsTable(count, specs: PersonInCities);
        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$filter"] = filter
        });

        var result = _engine.ExecuteTac(table, query);
        return result;
    }

    [Fact]
    public void FilterByCityEqualBern()
    {
        var result = FilterPrepareAndRun(250, $"{PersonSpecs.FieldCity} eq '{CityToFind}'");

        Assert.Equal(50, result.Items.Count);
        Assert.All(result.Items,
            e => Assert.Equal(CityToFind, e.Get<string>(PersonSpecs.FieldCity))
        );
    }


    [Fact]
    public void FilterByCityStartsWithBern()
    {
        var result = FilterPrepareAndRun(250, $"startswith({PersonSpecs.FieldCity}, '{CityToFind}')");

        Assert.Equal(50, result.Items.Count);
        Assert.All(result.Items,
            e => Assert.Equal(CityToFind, e.Get<string>(PersonSpecs.FieldCity))
        );
    }

    [Theory]
    [InlineData("Z", CitiesWithZ)]
    [InlineData("B", CitiesWithB)]
    public void FilterByCityStartsWithLetter(string start, string expectedCities)
    {
        var result = FilterPrepareAndRun(250, $"startswith({PersonSpecs.FieldCity}, '{start}')");

        Assert.Equal(100, result.Items.Count);
        var cities = expectedCities.Split(',');
        Assert.All(result.Items,
            e => Assert.Contains(e.Get<string>(PersonSpecs.FieldCity)!, cities)
        );
    }

    [Theory]
    [InlineData("Z", 150, CitiesWithZ)]
    [InlineData("B", 150, CitiesWithB)]
    public void FilterByCityNotStartsWithLetter(string start, int expected, string notExpectedCities)
    {
        var result = FilterPrepareAndRun(250, $"not startswith({PersonSpecs.FieldCity}, '{start}')");

        Assert.Equal(expected, result.Items.Count);
        var cities = notExpectedCities.Split(',');
        Assert.All(result.Items,
            e => Assert.DoesNotContain(e.Get<string>(PersonSpecs.FieldCity)!, cities)
        );
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

        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$filter"] = $"contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}') and {PersonSpecs.FieldHeight} gt 180"
        });

        var result = _engine.ExecuteTac(table, query);

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

        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$filter"] = $"not contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}')"
        });

        var result = _engine.ExecuteTac(table, query);

        Assert.Equal(expected.Count, result.Items.Count);
        Assert.All(result.Items, entity =>
            Assert.DoesNotContain(PersonSpecs.City1, entity.Get<string>(PersonSpecs.FieldCity) ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OrderByHeightDescendingWithTop()
    {
        var table = PersonsTable();
        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$orderby"] = "Height desc",
            ["$top"] = "3"
        });

        var result = _engine.ExecuteTac(table, query);
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
        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$orderby"] = "EntityId asc",
            ["$skip"] = "10",
            ["$top"] = "5"
        });

        var result = _engine.ExecuteTac(table, query);

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
        var query = UriQueryParserTac.Parse(new Dictionary<string, string>
        {
            ["$select"] = "EntityId,City"
        });

        var result = _engine.ExecuteTac(table, query);

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


