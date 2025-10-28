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
            return new Token(TokKind.Eof, string.Empty);

        var c = _text[_position];
        switch (c)
        {
            case '(':
                _position++;
                return new Token(TokKind.LParen, "(");

            case ')':
                _position++;
                return new Token(TokKind.RParen, ")");

            case ',':
                _position++;
                return new Token(TokKind.Comma, ",");

            case '/':
                _position++;
                return new Token(TokKind.Slash, "/");

            case '.':
                _position++;
                return new Token(TokKind.Dot, ".");

            case ':':
                _position++;
                return new Token(TokKind.Colon, ":");

            case '+':
                _position++;
                return new Token(TokKind.Add, "+");

            case '-':
                _position++;
                return new Token(TokKind.Sub, "-");

            case '*':
                _position++;
                return new Token(TokKind.Mul, "*");

            case '%':
                _position++;
                return new Token(TokKind.Mod, "%");

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
                    return new Token(TokKind.Eq, "=");
                }

                if (c == '!')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new Token(TokKind.Ne, "!=");
                    }
                }

                if (c == '>')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new Token(TokKind.Ge, ">=");
                    }
                    return new Token(TokKind.Gt, ">");
                }

                if (c == '<')
                {
                    _position++;
                    if (_position < _text.Length && _text[_position] == '=')
                    {
                        _position++;
                        return new Token(TokKind.Le, "<=");
                    }
                    return new Token(TokKind.Lt, "<");
                }

                // unknown char - skip once; if at end, return EOF
                _position++;

                if (_position >= _text.Length)
                    return new Token(TokKind.Eof, string.Empty);

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
        return new Token(TokKind.String, buf.ToString());
    }

    private Token ReadNumber()
    {
        var start = _position;
        while (_position < _text.Length && (char.IsDigit(_text[_position]) || _text[_position] == '.'))
            _position++;

        var t = _text.Substring(start, _position - start);
        return new Token(TokKind.Number, t);
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
            return new Token(TokKind.Identifier, w);
        }

        return w.ToUpperInvariant() switch
        {
            "AND" => new Token(TokKind.And, w),
            "OR" => new Token(TokKind.Or, w),
            "NOT" => new Token(TokKind.Not, w),
            "TRUE" => new Token(TokKind.True, w),
            "FALSE" => new Token(TokKind.False, w),
            "NULL" => new Token(TokKind.Null, w),
            "IN" => new Token(TokKind.In, w),
            "HAS" => new Token(TokKind.Has, w),
            "EQ" => new Token(TokKind.Eq, w),
            "NE" => new Token(TokKind.Ne, w),
            "GT" => new Token(TokKind.Gt, w),
            "GE" => new Token(TokKind.Ge, w),
            "LT" => new Token(TokKind.Lt, w),
            "LE" => new Token(TokKind.Le, w),
            "ADD" => new Token(TokKind.Add, w),
            "SUB" => new Token(TokKind.Sub, w),
            "MUL" => new Token(TokKind.Mul, w),
            "DIV" => new Token(TokKind.Div, w),
            "DIVBY" => new Token(TokKind.DivBy, w),
            "MOD" => new Token(TokKind.Mod, w),
            _ => new Token(TokKind.Identifier, w),
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

        if (string.Equals(w, "AND", StringComparison.OrdinalIgnoreCase))
            return new Token(TokKind.And, w);

        if (string.Equals(w, "OR", StringComparison.OrdinalIgnoreCase))
            return new Token(TokKind.Or, w);

        if (string.Equals(w, "NOT", StringComparison.OrdinalIgnoreCase))
            return new Token(TokKind.Not, w);

        return new Token(TokKind.Identifier, w);
    }
}