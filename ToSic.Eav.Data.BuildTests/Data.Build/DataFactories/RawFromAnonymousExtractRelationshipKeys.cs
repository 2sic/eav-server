using ToSic.Eav.Data.Raw.Sys;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Data.Build.DataFactories;

public class RawFromAnonymousExtractRelationshipKeys
{
    private void NoNewKeys(Dictionary<string, object?> dic)
    {
        var extracted = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeys(27, dic);
        var keys = extracted.relationshipKeys;
        Single(keys);
        Contains(27, keys);
    }
    
    [Fact]
    public void ExtractRelationshipKeysEmpty() =>
        NoNewKeys(new());

    [Fact]
    public void ExtractRelationshipKeysNoKeys() =>
        NoNewKeys(new() { { "NotKey", "" }});
    
    [Fact]
    public void ExtractRelationshipKeysNull() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), null }});
    
    [Fact]
    public void ExtractRelationshipKeysNoKeyData() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), "" }});
    [Fact]
    public void ExtractRelationshipKeysUnexpectedObject() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), new() } });

    [Fact]
    public void ExtractRelationshipKeysAnonWrongType() =>
        NoNewKeys(new() { { nameof(IHasRelationshipKeys.RelationshipKeys), new { Something = 12 } } });
    
    
    [Fact]
    public void ExtractRelationshipKeysString()
    {
        var data = new Dictionary<string, object?>()
        {
            { nameof(IHasRelationshipKeys.RelationshipKeys), new[] {"20","30"} }
        };
        var extracted = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeys(27, data);
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
            { nameof(IHasRelationshipKeys.RelationshipKeys), new[] {20,30} }
        };
        var extracted = new RawFromAnonymousHelper(null!)
            .ExtractRelationshipKeys(27, data);
        var keys = extracted.relationshipKeys;
        Equal(3, keys.Count);
        Contains(27, keys);
        Contains(20, keys);
        Contains(30, keys);
    }
}
