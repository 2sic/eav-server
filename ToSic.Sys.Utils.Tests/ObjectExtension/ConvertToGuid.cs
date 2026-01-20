namespace ToSic.Eav.Plumbing.ObjectExtension;


public class ConvertToGuid: ConvertTestBase
{

    [Fact] public void NullToGuid() => ConvT(null, null as string, null);
    [Fact] public void EmptyStringToGuid() => ConvT("", Guid.Empty, Guid.Empty);
    [Fact] public void Number0ToGuid() => ConvT(0, Guid.Empty, Guid.Empty);
    [Fact] public void Number1ToGuid() => ConvT(1, Guid.Empty, Guid.Empty);

    [Fact] public void NullToGuidSimpleFallback() => ConvFbQuick(null, FbGuid, FbGuid, true, true);
    [Fact] public void NullToGuidDefaultFallback() => ConvFbQuick<Guid>(null, default, default, true, true);
    [Fact] public void StringEmptyDefaultFallback() => ConvFbQuick<Guid>("", default, default, true, true);
    [Fact] public void StringEmptySimpleFallback() => ConvFbQuick("", FbGuid, FbGuid, true, true);
    [Fact] public void StringValidSimpleFallback() => ConvFbQuick(StrGuid, FbGuid, ExpGuid, true, true);

    private const string StrGuid = "23cec5c7-3d54-43ef-a80a-e5e5c1f8a397";
    private static readonly Guid ExpGuid = new(StrGuid);
    private static readonly Guid FbGuid = new("6d1f8424-af44-4a9b-a98d-ab9c14723072");

    [Fact] public void StringBracketsToGuid() => ConvT("{" + StrGuid + "}", ExpGuid, ExpGuid, ExpGuid);
    [Fact] public void StringSpacesToGuid() => ConvT(" " + StrGuid + " ", ExpGuid, ExpGuid, ExpGuid); 
    [Fact] public void StringCompactToGuid() => ConvT(StrGuid.Replace("-", ""), ExpGuid, ExpGuid, ExpGuid);
    [Fact] public void StringNoBracketsToGuid() => ConvT(StrGuid, ExpGuid, ExpGuid, ExpGuid);
}