using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Internal;

// Minimal search syntax: terms, phrases in quotes, AND/OR/NOT precedence
internal sealed class SearchExpressionParser(string text)
{
    private readonly List<Token> _toks = new Lexer(text ?? string.Empty, searchMode: true).Lex().ToList();
    private int _i;

    public Expr ParseSearch() => ParseOr();
    private Expr ParseOr()
    {
        var lhs = ParseAnd();
        while (!IsEof() && Peek().Kind == TokKind.Or)
        {
            Next();
            lhs = new BinaryExpr(lhs, BinaryOp.Or, ParseAnd());
        }
        return lhs;
    }
    private Expr ParseAnd()
    {
        var lhs = ParseUnary();
        while (!IsEof() && Peek().Kind == TokKind.And)
        {
            Next();
            lhs = new BinaryExpr(lhs, BinaryOp.And, ParseUnary());
        }
        return lhs;
    }
    private Expr ParseUnary()
    {
        if (IsEof() || Peek().Kind != TokKind.Not)
            return ParsePrimary();

        // If NOT is the only token or followed by end, treat as term per ABNF tests
        var save = _i;
        Next();
        if (IsEof()) { _i = save;
            return ParsePrimary(); }

        if (IsEof() || Peek().Kind != TokKind.LParen)
            return new UnaryExpr(UnaryExpr.Not, ParseUnary());

        // NOT ( a b ) -> parse grouped term list as right operand
        Next();
        var terms = new List<Expr>();
        while (!IsEof() && Peek().Kind != TokKind.RParen)
        {
            terms.Add(ParsePrimary());
        }
        Expect(TokKind.RParen);
        // Represent as NOT applied to a synthetic list via ANDs
        var grouped = terms.FirstOrDefault() ?? new IdentifierExpr("?");
        for (var i = 1; i < terms.Count; i++) grouped = new BinaryExpr(grouped, BinaryOp.And, terms[i]);
        return new UnaryExpr(UnaryExpr.Not, grouped);
    }
    private Expr ParsePrimary()
    {
        var t = Peek();
        switch (t.Kind)
        {
            case TokKind.String: return new LiteralExpr(Next().Text);
            case TokKind.Identifier: return new IdentifierExpr(Next().Text);
            case TokKind.And: // treat bare AND/OR as words (terms)
            case TokKind.Or:
            case TokKind.Not:
                return new IdentifierExpr(Next().Text);
            case TokKind.LParen: Next(); var e = ParseSearch(); Expect(TokKind.RParen); return e;
            default: Next(); return new IdentifierExpr("?");
        }
    }
    private Token Peek() => _toks[_i];
    private Token Next() => _toks[_i++];
    private Token Expect(TokKind k)
    {
        var t = Next();
        if (t.Kind != k)
            throw new FormatException($"Expected {k}, got {t.Kind}");
        return t;
    }
    private bool IsEof() => _i >= _toks.Count || _toks[_i].Kind == TokKind.Eof;
}