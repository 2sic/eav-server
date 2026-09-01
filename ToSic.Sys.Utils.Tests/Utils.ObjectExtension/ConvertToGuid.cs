namespace ToSic.Sys.Utils.ObjectExtension;


public class ConvertToGuid: ConvertTestBase
{
    #region Null / Empty to Guid, no fallback

    [Fact]
    public void NullToGuid() =>
        RunConvTest(value: null!, exp: null as string, expNumeric: null);
    
    [Fact]
    public void NullToGuidReal() =>
        RunConvTest(value: null!, exp: null as Guid?, expNumeric: null);

    [Fact]
    public void EmptyStringToGuid() =>
        RunConvTest(value: "", exp: Guid.Empty, expNumeric: Guid.Empty);
    
    [Fact]
    public void Number0ToGuid() =>
        RunConvTest(value: 0, exp: Guid.Empty, expNumeric: Guid.Empty);
    
    [Fact]
    public void Number1ToGuid() =>
        RunConvTest(value: 1, exp: Guid.Empty, expNumeric: Guid.Empty);

    #endregion

    #region Test Guids

    private const string StrGuid = "23cec5c7-3d54-43ef-a80a-e5e5c1f8a397";
    private const string StrGuidUpper = "23CEC5C7-3D54-43EF-A80A-E5E5C1F8A397";
    private const string StrGuidMixed = "23CeC5c7-3D54-43eF-a80A-e5E5C1f8A397";

    private static readonly Guid ExpGuid = new(g: StrGuid);
    private static readonly Guid FbGuid = new(g: "6d1f8424-af44-4a9b-a98d-ab9c14723072");

    #endregion

    #region Null / Empty to Guid, with fallbacks

    [Fact]
    public void NullToGuidSimpleFallback() =>
        ConvFbQuick(value: null!, fallback: FbGuid, exp: FbGuid, doBasic: true, doOnDefault: true);
    
    [Fact]
    public void NullToGuidDefaultFallback() =>
        ConvFbQuick<Guid>(value: null!, fallback: default, exp: default, doBasic: true, doOnDefault: true);
    
    [Fact]
    public void StringEmptyDefaultFallback() =>
        ConvFbQuick<Guid>(value: "", fallback: default, exp: default, doBasic: true, doOnDefault: true);
    
    [Fact]
    public void StringEmptySimpleFallback() =>
        ConvFbQuick(value: "", fallback: FbGuid, exp: FbGuid, doBasic: true, doOnDefault: true);
    
    [Fact] public void StringValidSimpleFallback() =>
        ConvFbQuick(value: StrGuid, fallback: FbGuid, exp: ExpGuid, doBasic: true, doOnDefault: true);

    #endregion

    #region Convert guid - using various add-ons or variations such as spaces, {} brackets etc.

    [Theory]
    [InlineData(StrGuid)]
    [InlineData(StrGuidUpper)]
    [InlineData(StrGuidMixed)]
    public void StringBracketsToGuid(string guid) =>
        RunConvTest(value: "{" + guid + "}", exp: ExpGuid, expNumeric: ExpGuid, expTruthy: ExpGuid);
    
    [Theory]
    [InlineData(StrGuid)]
    [InlineData(StrGuidUpper)]
    [InlineData(StrGuidMixed)]
    public void StringSpacesToGuid(string guid) =>
        RunConvTest(value: $" {guid} ", exp: ExpGuid, expNumeric: ExpGuid, expTruthy: ExpGuid); 
    
    [Theory]
    [InlineData(StrGuid)]
    [InlineData(StrGuidUpper)]
    [InlineData(StrGuidMixed)]
    public void StringCompactToGuid(string guid) =>
        RunConvTest(value: guid.Replace(oldValue: "-", newValue: ""), exp: ExpGuid, expNumeric: ExpGuid, expTruthy: ExpGuid);
    
    [Theory]
    [InlineData(StrGuid)]
    [InlineData(StrGuidUpper)]
    [InlineData(StrGuidMixed)]
    public void StringNoBracketsToGuid(string guid) =>
        RunConvTest(value: guid, exp: ExpGuid, expNumeric: ExpGuid, expTruthy: ExpGuid);
    #endregion

}