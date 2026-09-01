namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class ComputeClause
{
    public sealed class Item
    {
        public Expr? Expression { get; init; }
        public string? Alias { get; init; }
    }

    public List<Item> Items { get; } = [];
}