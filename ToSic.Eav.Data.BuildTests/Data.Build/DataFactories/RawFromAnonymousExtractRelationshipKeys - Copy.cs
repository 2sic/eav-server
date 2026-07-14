using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

public class RawFromAnonymousExtractRelationshipKeys
{
    private static void NoNewKeys(Dictionary<string, object?> dic, int expectedValueCount = 0)
    {
        var (values, relKeys) = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeysTac(27, dic);
        Single(relKeys);
        Contains(27, relKeys);
        Equal(expectedValueCount, values.Count);
    }
    
    [Fact]
    public void ExtractRelationshipKeysEmpty() =>
        NoNewKeys(new());

    [Fact]
    public void ExtractRelationshipKeysOnlyOtherKeys() =>
        NoNewKeys(new() { { "NotKey", "" }, { "Other", "" }}, 2);
    
    [Fact]
    public void ExtractRelationshipKeysNull() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), null }}, 1);
    
    [Fact]
    public void ExtractRelationshipKeysNoKeyData() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), "" }}, 1);

    [Fact]
    public void ExtractRelationshipKeysStringIgnored() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), "Something" }}, 1);

    [Fact]
    public void ExtractRelationshipKeysUnexpectedObject() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), new() } }, 1);

    [Fact]
    public void ExtractRelationshipKeysAnonWrongType() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), new { Something = 12 } } }, 1);
    
    
    [Fact]
    public void ExtractRelationshipKeysString()
    {
        var data = new Dictionary<string, object?>()
        {
            { nameof(IHasRelationshipKeys.RelationshipKeys), new[] { "20", "30" } }
        };
        var extracted = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeysTac(27, data);
        var keys = extracted.relationshipKeys;
        Equal(3, keys.Count);
        Contains(27, keys);
        Contains("20", keys);
        Contains("30", keys);
    }
    
    [Fact]
    public void ExtractRelationshipKeysInt()
    {
        var data = new Dictionary<string, object?>()
        {
            { nameof(IHasRelationshipKeys.RelationshipKeys), new[] { 20, 30 } }
        };
        var extracted = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeysTac(27, data);
        var keys = extracted.relationshipKeys;
        Equal(3, keys.Count);
        Contains(27, keys);
        Contains(20, keys);
        Contains(30, keys);
    }
}
