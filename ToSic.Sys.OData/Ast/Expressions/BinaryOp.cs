namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public enum BinaryOp
{
    Eq, Ne, Gt, Ge, Lt, Le, In, Has,
    And, Or,
    Add, Sub, Mul, Div, DivBy, Mod
}