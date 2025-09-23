namespace ToSic.Sys.OData.Internal;

internal enum TokKind
{
    Eof,
    Identifier,
    String,
    Number,
    LParen, RParen,
    Comma,
    Slash,
    Dot,
    Colon,
    Eq, Ne, Gt, Ge, Lt, Le,
    And, Or, Not,
    Add, Sub, Mul, Div, DivBy, Mod,
    True, False,
    Null,
    In,
    Has,
}