namespace ToSic.Sys.OData.Ast;

public sealed class OrderByClause
{
    public sealed class Item
    {
        public Expr Expression { get; set; }
        public bool Descending { get; set; }
    }

    public List<Item> Items { get; } = new List<Item>();
}