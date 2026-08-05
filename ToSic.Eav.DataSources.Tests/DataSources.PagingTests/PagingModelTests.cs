using ToSic.Eav.Data.Build;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSources.PagingTests;

[Startup(typeof(StartupTestsEavDataBuild))]
public class PagingModelTests(IDataFactory dataFactory)
{
    private const int PageSize = 17;
    private const int PageNumber = 3;
    private const int TotalItems = 242;
    private const int PageCount = 15; // TotalItems / PageSize;

    private static readonly PagingModelRaw Raw = new(PageSize, PageNumber, TotalItems, PageCount);

    private IPagingModel CreateModel()
        => dataFactory.Create(Raw).ToModelTac<IPagingModel>()!;

    [Fact]
    public void RoundTrip_PreservesPageNumber()
        => Equal(PageNumber, CreateModel().PageNumber);
    
    [Fact]
    public void RoundTrip_PreservesPageSize()
        => Equal(PageSize, CreateModel().PageSize);

    [Fact]
    public void RoundTrip_PreservesTotalItems()
        => Equal(TotalItems, CreateModel().ItemCount);

    [Fact]
    public void RoundTrip_PreservesPageCount()
        => Equal(PageCount, CreateModel().PageCount);

    [Fact]
    public void Entity_Id_IsPageNumber()
        => Equal(PageNumber, dataFactory.Create(Raw).EntityId);

    [Fact]
    public void Entity_Guid_IsEmpty()
        => Equal(Guid.Empty, dataFactory.Create(Raw).EntityGuid);
    
    [Fact]
    public void CreateModel_WithNullValues_StillWorks()
    {
        // Arrange
        var rawModel = new PagingModelRaw(PageSize, PageNumber, TotalItems, PageCount);
        var entity = dataFactory.Create(rawModel);
        // Simulate nulling values by creating a raw entity with missing properties
        var missingValuesEntity = dataFactory.Create(new Dictionary<string, object?>());

        // Act
        var model = missingValuesEntity.ToModelTac<IPagingModel>();

        // Assert
        // For IPagingModel, the defaults are likely 0 for numeric values if not found.
        Equal(0, model.PageNumber);
        Equal(0, model.PageSize);
        Equal(0, model.ItemCount);
        Equal(0, model.PageCount);
    }
}
