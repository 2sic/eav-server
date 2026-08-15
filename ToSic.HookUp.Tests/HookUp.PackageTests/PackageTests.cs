using ToSic.Sys.HookUp;

namespace ToSic.HookUp.PackageTests;

public class PackageTests
{
    [Fact]
    public void Package_RecordInitializer()
    {
        // Arrange
        var data = 123;
        // Act
        var package = new Package<int> { Data = data };
        // Assert
        Equal(data, package.Data);
    }
    
    [Fact]
    public void Package_ConstructorWithStringData()
    {
        // Arrange
        var data = "Another Test Data";
        // Act
        var package = new Package<string>(data);
        // Assert
        Equal(data, package.Data);
    }


    [Fact]
    public void Package_ConstructorWithIntData()
    {
        // Arrange
        var data = 456;
        // Act
        var package = new Package<int>(data);
        // Assert
        Equal(data, package.Data);
    }
    
    
    [Fact]
    public void ToPackage_ShouldWrapDataInPackage()
    {
        // Arrange
        var data = "Test Data";
        // Act
        var package = data.ToPackage();
        // Assert
        Equal(data, package.Data);
    }

    [Fact]
    public void RePackage_ShouldPreserveEverything()
    {
        // Arrange
        var data = "Test Data";
        var repackagedData = "Repackaging Reason";
        // Act
        var package = data.ToPackage();
        package = package with { Decision = DataPreprocessorDecision.Error };
        package = package.RePackage(repackagedData);
        // Assert
        Equal(repackagedData, package.Data);
        Equal(DataPreprocessorDecision.Error, package.Decision);
    }

}
