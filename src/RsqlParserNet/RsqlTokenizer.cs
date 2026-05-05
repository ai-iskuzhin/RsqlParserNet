using System.Globalization;

namespace RsqlParserNet;

/// <summary>
/// Reads an RSQL expression and produces lexical tokens for the syntax parser.
/// </summary>
internal sealed class RsqlTokenizer
{
    private readonly List<RsqlDiagnostic> _diagnostics = [];
    private readonly RsqlParseOptions _options;
    private readonly RsqlSourceText _sourceText;
    private readonly string _text;
    private int _position;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlTokenizer"/> class.
    /// </summary>
    /// <param name="text">The expression text to tokenize.</param>
    /// <param name="options">The parser options.</param>
    public RsqlTokenizer(string text, RsqlParseOptions options)
    {
        _text = text;
        _options = options;
        _sourceText = new RsqlSourceText(text);
    }

    /// <summary>
    /// Gets diagnostics produced while tokenizing invalid input.
    /// </summary>
    public IReadOnlyList<RsqlDiagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// Reads the next token from the expression.
    /// </summary>
    /// <returns>The next token.</returns>
    public RsqlToken Next()
    {
        var hadLeadingWhiteSpace = SkipWhiteSpace();

        if (_position >= _text.Length)
        {
            return new RsqlToken(RsqlTokenKind.EndOfInput, string.Empty, string.Empty, new RsqlTextSpan(_position, 0));
        }

        var start = _position;
        var current = _text[_position];

        return current switch
        {
            '(' => Single(RsqlTokenKind.OpenParen),
            ')' => Single(RsqlTokenKind.CloseParen),
            ',' => Single(RsqlTokenKind.Comma),
            ';' => Single(RsqlTokenKind.Semicolon),
            '"' or '\'' => ReadQuotedString(current),
            '=' => ReadEqualsOperator(),
            '!' => ReadNotEqual(),
            '>' => ReadGreaterThan(),
            '<' => ReadLessThan(),
            _ => IsReserved(current)
                ? Invalid(start, $"Unexpected character '{current}'.")
                : ReadIdentifier(hadLeadingWhiteSpace)
        };
    }

    private RsqlToken Single(RsqlTokenKind kind)
    {
        var start = _position;
        _position++;
        var text = _text.Substring(start, 1);
        return new RsqlToken(kind, text, text, new RsqlTextSpan(start, 1));
    }

    private RsqlToken ReadEqualsOperator()
    {
        var start = _position;

        if (Peek(1) == '=')
        {
            _position += 2;
            return new RsqlToken(RsqlTokenKind.Equal, "==", "==", new RsqlTextSpan(start, 2));
        }

        _position++;
        while (_position < _text.Length && char.IsAsciiLetter(_text[_position]))
        {
            _position++;
        }

        if (_position < _text.Length && _text[_position] == '=')
        {
            _position++;
            var text = _text[start.._position];
            var kind = text.ToLowerInvariant() switch
            {
                "=gt=" => RsqlTokenKind.GreaterThan,
                "=ge=" => RsqlTokenKind.GreaterThanOrEqual,
                "=lt=" => RsqlTokenKind.LessThan,
                "=le=" => RsqlTokenKind.LessThanOrEqual,
                "=in=" => RsqlTokenKind.In,
                "=out=" => RsqlTokenKind.NotIn,
                _ => RsqlTokenKind.Invalid
            };

            if (kind == RsqlTokenKind.Invalid && IsConfiguredCustomOperator(text))
            {
                kind = RsqlTokenKind.CustomOperator;
            }

            if (kind != RsqlTokenKind.Invalid)
            {
                return new RsqlToken(kind, text, text, new RsqlTextSpan(start, text.Length));
            }
        }

        return Invalid(start, "Unknown comparison operator.", _position);
    }

    private RsqlToken ReadNotEqual()
    {
        var start = _position;
        if (Peek(1) == '=')
        {
            _position += 2;
            return new RsqlToken(RsqlTokenKind.NotEqual, "!=", "!=", new RsqlTextSpan(start, 2));
        }

        return Invalid(start, "Expected '=' after '!'.");
    }

    private RsqlToken ReadGreaterThan()
    {
        var start = _position;
        _position++;
        if (_position < _text.Length && _text[_position] == '=')
        {
            _position++;
            return new RsqlToken(RsqlTokenKind.GreaterThanOrEqual, ">=", ">=", new RsqlTextSpan(start, 2));
        }

        return new RsqlToken(RsqlTokenKind.GreaterThan, ">", ">", new RsqlTextSpan(start, 1));
    }

    private RsqlToken ReadLessThan()
    {
        var start = _position;
        _position++;
        if (_position < _text.Length && _text[_position] == '=')
        {
            _position++;
            return new RsqlToken(RsqlTokenKind.LessThanOrEqual, "<=", "<=", new RsqlTextSpan(start, 2));
        }

        return new RsqlToken(RsqlTokenKind.LessThan, "<", "<", new RsqlTextSpan(start, 1));
    }

    private RsqlToken ReadIdentifier(bool hadLeadingWhiteSpace)
    {
        var start = _position;
        while (_position < _text.Length && !char.IsWhiteSpace(_text[_position]) && !IsReserved(_text[_position]))
        {
            _position++;
        }

        var value = _text[start.._position];
        // Treat word operators as operators only when surrounded by whitespace.
        // This keeps selectors and values such as "and==true" and "status==and" valid.
        if (_options.AllowWordLogicalOperators
            && hadLeadingWhiteSpace
            && HasTrailingWhiteSpace()
            && IsLogicalWord(value, out var logicalKind))
        {
            return new RsqlToken(logicalKind, value, value, new RsqlTextSpan(start, value.Length));
        }

        return new RsqlToken(RsqlTokenKind.Identifier, value, value, new RsqlTextSpan(start, value.Length));
    }

    private RsqlToken ReadQuotedString(char quote)
    {
        var start = _position;
        _position++;
        var value = new System.Text.StringBuilder();

        while (_position < _text.Length)
        {
            var current = _text[_position++];
            if (current == quote)
            {
                var stringSpan = RsqlTextSpan.FromBounds(start, _position);
                return new RsqlToken(RsqlTokenKind.String, value.ToString(), _text[start.._position], stringSpan);
            }

            if (current == '\\' && _position < _text.Length)
            {
                value.Append(_text[_position++]);
                continue;
            }

            value.Append(current);
        }

        var span = RsqlTextSpan.FromBounds(start, _position);
        _diagnostics.Add(_sourceText.CreateDiagnostic(
            RsqlDiagnosticCodes.InvalidToken,
            string.Format(CultureInfo.InvariantCulture, "Unterminated quoted string starting at position {0}.", start),
            span));

        var text = _text[start.._position];
        return new RsqlToken(RsqlTokenKind.Invalid, text, text, RsqlTextSpan.FromBounds(start, _position));
    }

    private RsqlToken Invalid(int start, string message, int? end = null)
    {
        _position = end ?? Math.Min(_position + 1, _text.Length);
        var span = RsqlTextSpan.FromBounds(start, _position);
        _diagnostics.Add(_sourceText.CreateDiagnostic(RsqlDiagnosticCodes.InvalidToken, message, span));
        var text = _text[start.._position];
        return new RsqlToken(RsqlTokenKind.Invalid, text, text, span);
    }

    private bool SkipWhiteSpace()
    {
        var hadWhiteSpace = false;
        while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
        {
            hadWhiteSpace = true;
            _position++;
        }

        return hadWhiteSpace;
    }

    private char? Peek(int offset)
    {
        var index = _position + offset;
        return index < _text.Length ? _text[index] : null;
    }

    private static bool IsReserved(char value)
    {
        return value is '"' or '\'' or '(' or ')' or ';' or ',' or '=' or '!' or '~' or '<' or '>';
    }

    private bool HasTrailingWhiteSpace()
    {
        return _position < _text.Length && char.IsWhiteSpace(_text[_position]);
    }

    private static bool IsLogicalWord(string value, out RsqlTokenKind kind)
    {
        if (string.Equals(value, "and", StringComparison.OrdinalIgnoreCase))
        {
            kind = RsqlTokenKind.And;
            return true;
        }

        if (string.Equals(value, "or", StringComparison.OrdinalIgnoreCase))
        {
            kind = RsqlTokenKind.Or;
            return true;
        }

        kind = RsqlTokenKind.Invalid;
        return false;
    }

    private bool IsConfiguredCustomOperator(string value)
    {
        return _options.CustomOperators.Any(x => string.Equals(x.Text, value, StringComparison.OrdinalIgnoreCase));
    }
}
