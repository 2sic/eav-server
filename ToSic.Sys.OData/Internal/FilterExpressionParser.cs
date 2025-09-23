using System.Globalization;
using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Internal;

// Very small Pratt-style expression parser for $filter and compute/orderby expressions
internal sealed class FilterExpressionParser
{
    private readonly List<Token> _toks;
    private int _i;

    public FilterExpressionParser(string text)
    {
        _toks = new Lexer(text ?? string.Empty).Lex().ToList();
    }

    public Expr ParseExpression(int minPrec = 0)
    {
        var lhs = ParsePrimary();
        while (true)
        {
            var (isOp, op, prec, rightAssoc) = PeekOp();
            if (!isOp || prec < minPrec) break;
            var opTok = Next();
            var nextMin = rightAssoc ? prec : prec + 1;
            Expr rhs;
            if (opTok.Kind == TokKind.In)
            {
                // Accept either listExpr: (a, b, ...) or a single expr
                if (Peek().Kind == TokKind.LParen)
                {
                    Next();
                    var items = new List<Expr>();
                    if (Peek().Kind != TokKind.RParen)
                    {
                        while (true)
                        {
                            items.Add(ParseExpression());
                            if (Peek().Kind == TokKind.Comma) { Next(); continue; }
                            break;
                        }
                    }
                    Expect(TokKind.RParen);
                    rhs = new ListExpr(items);
                }
                else
                {
                    rhs = ParseExpression(nextMin);
                }
                op = BinaryOp.In;
            }
            else
            {
                rhs = ParseExpression(nextMin);
            }
            lhs = new BinaryExpr(lhs, op, rhs);
        }
        return lhs;
    }

    private Expr ParsePrimary()
    {
        var t = Peek();
        switch (t.Kind)
        {
            case TokKind.Identifier:
            {
                // function call or dotted/segmented identifier (allow a/b and a.b)
                var name = Next().Text;
                // support a/b style by collapsing with '/' into IdentifierExpr("a/b")
                while (Peek().Kind == TokKind.Slash || Peek().Kind == TokKind.Dot)
                {
                    var sep = Next();
                    name += (sep.Kind == TokKind.Slash ? "/" : ".") + Expect(TokKind.Identifier).Text;
                }
                if (Peek().Kind == TokKind.LParen)
                {
                    Next(); // '('
                    var args = new List<Expr>();
                    if (Peek().Kind != TokKind.RParen)
                    {
                        // Minimal support for lambda any/all: [var ':'] predicate
                        var isLambdaOp = name.EndsWith("/any", StringComparison.OrdinalIgnoreCase) ||
                                         name.EndsWith("/all", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(name, "any", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(name, "all", StringComparison.OrdinalIgnoreCase);
                        if (isLambdaOp && Peek().Kind == TokKind.Identifier && _toks[_i + 1].Kind == TokKind.Colon)
                        {
                            // consume var ':'
                            Next(); // var
                            Next(); // ':'
                            // parse predicate as single argument
                            args.Add(ParseExpression());
                        }
                        else
                        {
                            while (true)
                            {
                                var argExpr = ParseExpression();
                                args.Add(argExpr);
                                if (Peek().Kind == TokKind.Comma) { Next(); continue; }
                                break;
                            }
                        }
                    }
                    Expect(TokKind.RParen);
                    return new CallExpr(name, args);
                }
                return new IdentifierExpr(name);
            }
            case TokKind.String:
                return new LiteralExpr(Next().Text);
            case TokKind.True:
                Next(); return new LiteralExpr(true);
            case TokKind.False:
                Next(); return new LiteralExpr(false);
            case TokKind.Null:
                Next(); return new LiteralExpr(null);
            case TokKind.Number:
                var numTok = Next();
                if (numTok.Text.Contains("."))
                    return new LiteralExpr(double.Parse(numTok.Text, CultureInfo.InvariantCulture));
                else
                    return new LiteralExpr(long.Parse(numTok.Text, CultureInfo.InvariantCulture));
            case TokKind.Sub:
                Next(); return new UnaryExpr("-", ParsePrimary());
            case TokKind.Not:
                Next(); return new UnaryExpr("not", ParsePrimary());
            case TokKind.LParen:
                Next(); var expr = ParseExpression(); Expect(TokKind.RParen); return expr;
            default:
                // unknown -> produce empty identifier to keep permissive
                Next(); return new IdentifierExpr("?");
        }
    }

    private (bool ok, BinaryOp op, int prec, bool rightAssoc) PeekOp()
    {
        switch (Peek().Kind)
        {
            case TokKind.Mul: return (true, BinaryOp.Mul, 40, false);
            case TokKind.Div: return (true, BinaryOp.Div, 40, false);
            case TokKind.DivBy: return (true, BinaryOp.DivBy, 40, false);
            case TokKind.Mod: return (true, BinaryOp.Mod, 40, false);
            case TokKind.Add: return (true, BinaryOp.Add, 30, false);
            case TokKind.Sub: return (true, BinaryOp.Sub, 30, false);
            case TokKind.Eq: return (true, BinaryOp.Eq, 20, false);
            case TokKind.Ne: return (true, BinaryOp.Ne, 20, false);
            case TokKind.Gt: return (true, BinaryOp.Gt, 20, false);
            case TokKind.Ge: return (true, BinaryOp.Ge, 20, false);
            case TokKind.Lt: return (true, BinaryOp.Lt, 20, false);
            case TokKind.Le: return (true, BinaryOp.Le, 20, false);
            case TokKind.In: return (true, BinaryOp.Eq, 20, false); // represent 'in' as Eq to a list in AST usage
            case TokKind.Has: return (true, BinaryOp.Has, 20, false);
            case TokKind.And: return (true, BinaryOp.And, 10, false);
            case TokKind.Or: return (true, BinaryOp.Or, 5, false);
            default: return (false, default, 0, false);
        }
    }

    private Token Peek() => _toks[_i];
    private Token Next() => _toks[_i++];
    private Token Expect(TokKind kind)
    {
        var t = Next();
        if (t.Kind != kind) throw new FormatException($"Expected {kind}, got {t.Kind}");
        return t;
    }
}