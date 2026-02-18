using static System.StringComparison;

namespace ToSic.Sys.OData.Internal;

internal sealed class Lexer(string? text, bool searchMode = false)
{
    private readonly string _text = text ?? string.Empty;
    private int _position;

    public IEnumerable<Token> Lex()
    {
        Token t;
        do
        {
            t = NextToken(); 
            yield return t;
        } 
        while (t.Kind != TokKind.Eof);
    }

    private void SkipWs()
    {
        while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            _position++;
    }

    private Token NextToken()
    {
        SkipWs();
        if (_position >= _text.Length)
            return new(TokKind.Eof, string.Empty);

        var c = _text[_position];
        switch (c)
        {
            case '(':
                _position++;
                return new(TokKind.LParen, "(");

            case ')':
                _position++;
                return new(TokKind.RParen, ")");

            case ',':
                _position++;
                return new(TokKind.Comma, ",");

            case '/':
                _position++;
                return new(TokKind.Slash, "/");

            case '.':
                _position++;
                return new(TokKind.Dot, ".");

            case ':':
                _position++;
                return new(TokKind.Colon, ":");

            case '+':
                _position++;
                return new(TokKind.Add, "+");

            case '-':
                _position++;
                return new(TokKind.Sub, "-");

            case '*':
                _position++;
                return new(TokKind.Mul, "*");

            case '%':
                _position++;
                return new(TokKind.Mod, "%");

            case '\'':
                return searchMode 
                    ? ReadSearchWord() // In search mode treat a leading quote as part of a term until whitespace/paren
                    : ReadString('\'');

            case '"':
                return searchMode
                    ? ReadSearchWord()
                    : ReadString('"');

            default:
                if (char.IsDigit(c))
                    return ReadNumber();

                if (char.IsLetter(c) || c == '_' || c == '@' || c == '$')
                    return ReadWord();

                if (searchMode && !char.IsWhiteSpace(c) && c != '(' && c != ')')
                    return ReadSearchWord();

                if (c == '=')
                {
                    _position++;
                    return new(TokKind.Eq, "=");
                }

                if (c == '!')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new(TokKind.Ne, "!=");
                    }
                }

                if (c == '>')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new(TokKind.Ge, ">=");
                    }
                    return new(TokKind.Gt, ">");
                }

                if (c == '<')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new(TokKind.Le, "<=");
                    }
                    return new(TokKind.Lt, "<");
                }

                // unknown char - skip once; if at end, return EOF
                _position++;

                if (_position >= _text.Length)
                    return new(TokKind.Eof, string.Empty);

                return NextToken();
        }
    }

    private Token ReadString(char quote)
    {
        var start = ++_position; // skip opening quote
        var buf = new System.Text.StringBuilder();
        while (_position < _text.Length)
        {
            var c = _text[_position++];
            if (c == quote)
            {
                // doubled '' inside string -> escape
                if (_position < _text.Length && _text[_position] == quote)
                {
                    buf.Append(quote);
                    _position++;
                    continue;
                }
                break;
            }
            buf.Append(c);
        }
        if (_position >= _text.Length && (_text.Length == 0 || _text[_text.Length - 1] != quote))
        {
            throw new FormatException("Unterminated string literal");
        }
        return new(TokKind.String, buf.ToString());
    }

    private Token ReadNumber()
    {
        var start = _position;
        while (_position < _text.Length && (char.IsDigit(_text[_position]) || _text[_position] == '.'))
            _position++;

        var t = _text.Substring(start, _position - start);
        return new(TokKind.Number, t);
    }

    private Token ReadWord()
    {
        var start = _position;
        while (_position < _text.Length && (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_' || _text[_position] == '@' || _text[_position] == '$'))
            _position++;

        var w = _text.Substring(start, _position - start);
        if (w.Length == 0)
        {
            // ensure progress to avoid infinite loop
            w = _text[_position].ToString();
            _position++;
            return new(TokKind.Identifier, w);
        }

        return w.ToUpperInvariant() switch
        {
            "AND" => new(TokKind.And, w),
            "OR" => new(TokKind.Or, w),
            "NOT" => new(TokKind.Not, w),
            "TRUE" => new(TokKind.True, w),
            "FALSE" => new(TokKind.False, w),
            "NULL" => new(TokKind.Null, w),
            "IN" => new(TokKind.In, w),
            "HAS" => new(TokKind.Has, w),
            "EQ" => new(TokKind.Eq, w),
            "NE" => new(TokKind.Ne, w),
            "GT" => new(TokKind.Gt, w),
            "GE" => new(TokKind.Ge, w),
            "LT" => new(TokKind.Lt, w),
            "LE" => new(TokKind.Le, w),
            "ADD" => new(TokKind.Add, w),
            "SUB" => new(TokKind.Sub, w),
            "MUL" => new(TokKind.Mul, w),
            "DIV" => new(TokKind.Div, w),
            "DIVBY" => new(TokKind.DivBy, w),
            "MOD" => new(TokKind.Mod, w),
            _ => new(TokKind.Identifier, w),
        };
    }

    private Token ReadSearchWord()
    {
        var start = _position;
        while (_position < _text.Length)
        {
            var ch = _text[_position];
            if (char.IsWhiteSpace(ch) || ch == '(' || ch == ')')
                break;

            _position++;
        }
        var w = _text.Substring(start, _position - start);

        if (string.Equals(w, "AND", OrdinalIgnoreCase))
            return new(TokKind.And, w);

        if (string.Equals(w, "OR", OrdinalIgnoreCase))
            return new(TokKind.Or, w);

        if (string.Equals(w, "NOT", OrdinalIgnoreCase))
            return new(TokKind.Not, w);

        return new(TokKind.Identifier, w);
    }
}