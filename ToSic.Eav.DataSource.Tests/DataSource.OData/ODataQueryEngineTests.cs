using ToSic.Eav.Services;

namespace ToSic.Eav.DataSource.OData;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class ODataQueryEngineTests(
    IDataSourcesService dataSourcesService,
    DataTablePerson dataTablePerson)
{
    private readonly ODataQueryEngine _engine = new(dataSourcesService);

    private DataTable PersonsTable(int count = 200, PersonSpecs? specs = null)
        => dataTablePerson.Generate(count, useCacheForSpeed: false, specs: specs);

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
        var query = new Dictionary<string, string>
        {
            ["$filter"] = filter
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);
        return result;
    }

    [Fact]
    public void FilterByCityEqualBern()
    {
        var result = FilterPrepareAndRun(250, $"{PersonSpecs.FieldCity} eq '{CityToFind}'");

        Equal(50, result.Items.Count);
        All(result.Items,
            e => Equal(CityToFind, e.Get<string>(PersonSpecs.FieldCity))
        );
    }


    [Fact]
    public void FilterByCityStartsWithBern()
    {
        var result = FilterPrepareAndRun(250, $"startswith({PersonSpecs.FieldCity}, '{CityToFind}')");

        Equal(50, result.Items.Count);
        All(result.Items,
            e => Equal(CityToFind, e.Get<string>(PersonSpecs.FieldCity))
        );
    }

    [Theory]
    [InlineData("Z", CitiesWithZ)]
    [InlineData("B", CitiesWithB)]
    public void FilterByCityStartsWithLetter(string start, string expectedCities)
    {
        var result = FilterPrepareAndRun(250, $"startswith({PersonSpecs.FieldCity}, '{start}')");

        Equal(100, result.Items.Count);
        var cities = expectedCities.Split(',');
        All(result.Items,
            e => Contains(e.Get<string>(PersonSpecs.FieldCity)!, cities)
        );
    }

    [Theory]
    [InlineData("Z", 150, CitiesWithZ)]
    [InlineData("B", 150, CitiesWithB)]
    public void FilterByCityNotStartsWithLetter(string start, int expected, string notExpectedCities)
    {
        var result = FilterPrepareAndRun(250, $"not startswith({PersonSpecs.FieldCity}, '{start}')");

        Equal(expected, result.Items.Count);
        var cities = notExpectedCities.Split(',');
        All(result.Items,
            e => DoesNotContain(e.Get<string>(PersonSpecs.FieldCity)!, cities)
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

        var query = new Dictionary<string, string>
        {
            ["$filter"] = $"contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}') and {PersonSpecs.FieldHeight} gt 180"
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);

        Equal(expected.Count, result.Items.Count);
        All(result.Items, entity =>
        {
            Contains(PersonSpecs.City1, entity.Get<string>(PersonSpecs.FieldCity) ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            True(entity.Get<int?>(PersonSpecs.FieldHeight) > 180);
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

        var query = new Dictionary<string, string>
        {
            ["$filter"] = $"not contains({PersonSpecs.FieldCity},'{PersonSpecs.City1}')"
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);

        Equal(expected.Count, result.Items.Count);
        All(result.Items, entity =>
            DoesNotContain(PersonSpecs.City1, entity.Get<string>(PersonSpecs.FieldCity) ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OrderByHeightDescendingWithTop()
    {
        var table = PersonsTable();
        var query = new Dictionary<string, string>
        {
            ["$orderby"] = "Height desc",
            ["$top"] = "3"
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);
        Equal(3, result.Items.Count);

        var expected = table.List
            .OrderByDescending(e => e.Get<int?>(PersonSpecs.FieldHeight))
            .Take(3)
            .Select(e => e.EntityId)
            .ToArray();

        Equal(expected, result.Items.Select(e => e.EntityId).ToArray());
    }

    [Fact]
    public void SkipAndTopReturnExpectedPage()
    {
        var table = PersonsTable();
        var query = new Dictionary<string, string>
        {
            ["$orderby"] = "EntityId asc",
            ["$skip"] = "10",
            ["$top"] = "5"
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);

        var expected = table.List
            .OrderBy(e => e.EntityId)
            .Skip(10)
            .Take(5)
            .Select(e => e.EntityId)
            .ToArray();

        Equal(expected, result.Items.Select(e => e.EntityId).ToArray());
    }

    [Fact]
    public void SelectProjectsRequestedFields()
    {
        var table = PersonsTable();
        var query = new Dictionary<string, string>
        {
            ["$select"] = "EntityId,City"
        }.ToQueryTac();

        var result = _engine.ExecuteTac(table, query);

        NotEmpty(result.Projection);
        Equal(result.Items.Count, result.Projection.Count);

        foreach (var (entity, projection) in result.Items.Zip(result.Projection, (entity, projection) => (entity, projection)))
        {
            Equal(2, projection.Count);
            Equal(entity.EntityId, IsType<int>(projection["EntityId"]));
            Equal(entity.Get<string>(PersonSpecs.FieldCity), projection[PersonSpecs.FieldCity] as string);
        }
    }
}


