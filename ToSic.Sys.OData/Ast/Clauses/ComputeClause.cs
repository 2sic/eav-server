namespace ToSic.Sys.OData.Ast;

public sealed class ComputeClause
{
    public sealed class Item
    {
        public Expr Expression { get; set; }
        public string Alias { get; set; }
    }

    public List<Item> Items { get; } = new List<Item>();
}