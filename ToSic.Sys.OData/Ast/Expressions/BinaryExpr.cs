namespace ToSic.Sys.OData.Ast;

public sealed class BinaryExpr(Expr left, BinaryOp op, Expr right) : Expr
{
    public Expr Left { get; } = left;
    public BinaryOp Op { get; } = op;
    public Expr Right { get; } = right;

    public override string ToString()
    {
        var op = Op switch
        {
            BinaryOp.Eq => "eq",
            BinaryOp.Ne => "ne",
            BinaryOp.Gt => "gt",
            BinaryOp.Ge => "ge",
            BinaryOp.Lt => "lt",
            BinaryOp.Le => "le",
            BinaryOp.In => "in",
            BinaryOp.Has => "has",
            BinaryOp.And => "and",
            BinaryOp.Or => "or",
            BinaryOp.Add => "add",
            BinaryOp.Sub => "sub",
            BinaryOp.Mul => "mul",
            BinaryOp.Div => "div",
            BinaryOp.DivBy => "divby",
            BinaryOp.Mod => "mod",
            _ => Op.ToString()
        };
        return $"({Left} {op} {Right})";
    }
}