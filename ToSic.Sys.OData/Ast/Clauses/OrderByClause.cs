namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class OrderByClause
{
    public sealed class Item
    {
        public Expr? Expression { get; init; }
        public bool Descending { get; init; }
    }

    public List<Item> Items { get; } = [];
}