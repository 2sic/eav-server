namespace ToSic.Sys.OData.Ast;

public sealed class BinaryExpr : Expr
{
    public Expr Left { get; }
    public BinaryOp Op { get; }
    public Expr Right { get; }

    public BinaryExpr(Expr left, BinaryOp op, Expr right)
    {
        Left = left; Op = op; Right = right;
    }

    public override string ToString()
    {
        string op = Op switch
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