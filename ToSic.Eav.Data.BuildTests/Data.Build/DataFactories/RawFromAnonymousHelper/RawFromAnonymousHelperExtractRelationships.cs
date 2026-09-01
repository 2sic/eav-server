using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Data.Build.DataFactories.RawFromAnonymousHelper;

public class RawFromAnonymousHelperExtractRelationships
{
    public static IDictionary<string, RawRelationship> JustExtractRelationships(Dictionary<string, object?> dic)
        => new Raw.Sys.RawFromAnonymousHelper(null!).ExtractRelationshipsTac(dic);

    public static IDictionary<string, object?> JustStrongTypeRelationships(Dictionary<string, object?> dic)
        => new Raw.Sys.RawFromAnonymousHelper(null!).StrongTypeRelationshipsTac(dic);
    
    #region No Relationships to Extract

    private static void NoRelationships(Dictionary<string, object?> dic)
    {
        var relationships = JustExtractRelationships(dic);
        Empty(relationships);
    }

    [Fact]
    public void NoRelationships_BecauseEmpty() => NoRelationships(new());

    [Fact]
    public void NoRelationships_BecauseStringsOnly() => NoRelationships(new() { { "a", "b" }, { "b", "c" } });

    [Fact]
    public void NoRelationships_BecauseAnonEmpty() => NoRelationships(new() { { "a", new { } } });

    [Fact]
    public void NoRelationships_BecauseAnonWithoutRelationships() => NoRelationships(new() { { "a", new { Prop = "value" } } });

    #endregion

    #region No Relationships To Replace
    
    private static void NoRelationshipsReplaced(Dictionary<string, object?> dic)
    {
        var relationships = JustStrongTypeRelationships(dic);
        Equal(dic, relationships);
        Same(dic, relationships);   // should be identical dictionary, as no replacements were made
    }

    [Fact]
    public void NoRelationshipsReplaced_BecauseEmpty() => NoRelationshipsReplaced(new());
    
    [Fact]
    public void NoRelationshipsReplaced_BecauseStringsOnly() => NoRelationshipsReplaced(new() { { "a", "b" }, { "b", "c" } });

    [Fact]
    public void NoRelationshipsReplaced_BecauseAnonEmpty() => NoRelationshipsReplaced(new() { { "a", new { } } });

    [Fact]
    public void NoRelationshipsReplaced_BecauseAnonWithoutRelationships() => NoRelationshipsReplaced(new() { { "a", new { Prop = "value" } } });
    
    #endregion

    #region One Relationship-Axis with One Relationship Key

    private static void Relationship_OneOne<TKey>(string name, TKey relKey)
    {
        var relationships = JustExtractRelationships(new()
        {
            { name, new { Relationships = relKey} }
        });
        Single(relationships);
        Equal(relKey, relationships.First().Value.Keys.First());
    }
    
    [Theory]
    [InlineData("ChildrenSample", 492)]
    [InlineData("ChildrenSample", "File/492")]
    [InlineData("SpecificKey", 492)]
    [InlineData("SpecificKey", "File/492")]
    public void Relationship_OneOne_Various(string name, object value) => Relationship_OneOne(name, value);

    [Theory]
    [InlineData("ChildrenSample")]
    [InlineData("SpecificKey")]
    public void Relationship_OneOne_Guid(string name) => Relationship_OneOne(name, Guid.NewGuid());

    #endregion


    #region One Relationship-Axis with One Relationship Key Replaced

    private static void RelationshipReplaced_OneOne<TKey>(string name, TKey relKey)
    {
        var dic = new Dictionary<string, object?>
        {
            { name, new { Relationships = relKey } }
        };
        var relationships = JustStrongTypeRelationships(dic);
        Single(relationships);
        NotSame(dic, relationships);    // not same instance at all, as it was remade
        Equal(relKey, ((RawRelationship)relationships.First().Value!).Keys.First());
    }

    [Theory]
    [InlineData("ChildrenSample", 492)]
    [InlineData("ChildrenSample", "File/492")]
    [InlineData("SpecificKey", 492)]
    [InlineData("SpecificKey", "File/492")]
    public void RelationshipReplaced_OneOne_Various(string name, object value) => RelationshipReplaced_OneOne(name, value);

    [Theory]
    [InlineData("ChildrenSample")]
    [InlineData("SpecificKey")]
    public void RelationshipReplaced_OneOne_Guid(string name) => RelationshipReplaced_OneOne(name, Guid.NewGuid());

    #endregion

    #region Single Relationship-Axis Multiple Keys

    [Theory]
    [InlineData( "File/492", "Page/123")]
    [InlineData("File/492", 123, "00000000-0000-0000-0000-000000000000")]
    public void Relationship_Many_Mixed(params object[] keysRaw)
    {
        // Convert keys in a way which will make Guids become typed Guid objects
        var keys = keysRaw
            .Select(k => k is string s && Guid.TryParse(s, out var guid) ? guid : k)
            .ToList();
        var relationships = JustExtractRelationships(new()
        {
            { "ChildrenSample", new { Relationships = keys } }
        });
        Single(relationships);
        var relKeys = relationships.First().Value.Keys.ToList();
        Equal(keys.Count, relKeys.Count);
        Equal(keys, relKeys);
        //foreach (var key in keys)
        //    Contains(key, relKeys);
    }

    #endregion

    #region Multiple Relationships, one or many keys

    [Fact]
    public void MultipleRelationships()
    {
        var relationships = JustExtractRelationships(new()
        {
            { "ChildrenSample", new { Relationships = "File/492" } },
            { "ContentSample", new { Relationships = "Page/123" } },
        });
        Equal(2, relationships.Count);
        Contains("File/492", relationships["ChildrenSample"].Keys);
        Contains("Page/123", relationships["ContentSample"].Keys);
    }

    [Fact]
    public void MixedRelationshipsAndValues()
    {
        var relationships = JustExtractRelationships(new()
        {
            { "ChildrenSample", new { Relationships = "File/492" } },
            { "Title", "My Title" },
            { "ContentSample", new { Relationships = "Page/123" } },
            { "Number", 123 }
        });
        Equal(2, relationships.Count);
        Contains("File/492", relationships["ChildrenSample"].Keys);
        Contains("Page/123", relationships["ContentSample"].Keys);
    }

    #endregion

}
