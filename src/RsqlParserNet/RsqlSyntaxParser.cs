using System.Globalization;

namespace RsqlParserNet;

/// <summary>
/// Parses RSQL tokens into a typed abstract syntax tree.
/// </summary>
internal sealed class RsqlSyntaxParser
{
    private readonly List<RsqlDiagnostic> _diagnostics = [];
    private readonly RsqlParseOptions _options;
    private readonly RsqlSourceText _sourceText;
    private readonly string _text;
    private RsqlTokenizer _tokenizer = null!;
    private RsqlToken _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSyntaxParser"/> class.
    /// </summary>
    /// <param name="text">The expression text to parse.</param>
    /// <param name="options">The parser options.</param>
    public RsqlSyntaxParser(string text, RsqlParseOptions options)
    {
        _text = text;
        _options = options;
        _sourceText = new RsqlSourceText(text);
    }

    /// <summary>
    /// Parses the configured expression text.
    /// </summary>
    /// <returns>The parse result, including diagnostics when parsing fails.</returns>
    public RsqlParseResult Parse()
    {
        if (string.IsNullOrWhiteSpace(_text))
        {
            return Failure(_sourceText.CreateDiagnostic(
                RsqlDiagnosticCodes.EmptyExpression,
                "RSQL expression must not be empty.",
                new RsqlTextSpan(0, _text.Length)));
        }

        _tokenizer = new RsqlTokenizer(_text, _options);
        Next();

        var root = ParseOr();

        if (_current.Kind != RsqlTokenKind.EndOfInput)
        {
            AddUnexpectedToken("end of input");
        }

        _diagnostics.AddRange(_tokenizer.Diagnostics);

        return _diagnostics.Count == 0 && root is not null
            ? new RsqlParseResult(new RsqlQuery(_text, root), [])
            : new RsqlParseResult(null, _diagnostics);
    }

    private RsqlNode? ParseOr()
    {
        var left = ParseAnd();

        while (_current.Kind is RsqlTokenKind.Comma or RsqlTokenKind.Or)
        {
            Next();
            if (!IsPrimaryStart(_current.Kind))
            {
                AddUnexpectedToken("expression");
                return null;
            }

            var right = ParseAnd();
            if (left is null || right is null)
            {
                return null;
            }

            left = CreateLogicalNode(RsqlLogicalOperator.Or, left, right);
        }

        return left;
    }

    private RsqlNode? ParseAnd()
    {
        var left = ParsePrimary();

        while (_current.Kind is RsqlTokenKind.Semicolon or RsqlTokenKind.And)
        {
            Next();
            if (!IsPrimaryStart(_current.Kind))
            {
                AddUnexpectedToken("expression");
                return null;
            }

            var right = ParsePrimary();
            if (left is null || right is null)
            {
                return null;
            }

            left = CreateLogicalNode(RsqlLogicalOperator.And, left, right);
        }

        return left;
    }

    private RsqlNode? ParsePrimary()
    {
        if (_current.Kind != RsqlTokenKind.OpenParen)
        {
            return ParseComparison();
        }

        var openParen = _current;
        Next();

        if (_current.Kind == RsqlTokenKind.CloseParen)
        {
            AddUnexpectedToken("expression");
            return null;
        }

        var expression = ParseOr();
        var closeParen = Expect(RsqlTokenKind.CloseParen, "')'");

        if (expression is null)
        {
            return null;
        }

        return closeParen is null
            ? expression
            : expression with { Span = RsqlTextSpan.FromBounds(openParen.Span.Start, closeParen.Value.Span.End) };
    }

    private RsqlComparisonNode? ParseComparison()
    {
        var selector = Expect(RsqlTokenKind.Identifier, "selector");
        var op = ParseOperator();
        var values = op is null ? [] : ParseArguments(op.Value);

        if (selector is not null && !RsqlSelectorValidator.IsValid(selector.Value.Text, _options))
        {
            AddInvalidSelector(selector.Value);
            return null;
        }

        if (selector is null || op is null || values.Count == 0)
        {
            return null;
        }

        var end = values[^1].Span.End;
        return new RsqlComparisonNode(
            selector.Value.Text,
            op.Value.Operator,
            op.Value.Text,
            values,
            RsqlTextSpan.FromBounds(selector.Value.Span.Start, end));
    }

    private (RsqlComparisonOperator Operator, string Text, RsqlTextSpan Span)? ParseOperator()
    {
        var op = _current.Kind switch
        {
            RsqlTokenKind.Equal => RsqlComparisonOperator.Equal,
            RsqlTokenKind.NotEqual => RsqlComparisonOperator.NotEqual,
            RsqlTokenKind.GreaterThan => RsqlComparisonOperator.GreaterThan,
            RsqlTokenKind.GreaterThanOrEqual => RsqlComparisonOperator.GreaterThanOrEqual,
            RsqlTokenKind.LessThan => RsqlComparisonOperator.LessThan,
            RsqlTokenKind.LessThanOrEqual => RsqlComparisonOperator.LessThanOrEqual,
            RsqlTokenKind.In => RsqlComparisonOperator.In,
            RsqlTokenKind.NotIn => RsqlComparisonOperator.NotIn,
            RsqlTokenKind.CustomOperator => RsqlComparisonOperator.Custom,
            _ => (RsqlComparisonOperator?)null
        };

        if (op is null)
        {
            AddUnexpectedToken("comparison operator");
            return null;
        }

        var span = _current.Span;
        var text = _current.Text;
        Next();
        return (op.Value, text, span);
    }

    private List<RsqlValue> ParseArguments((RsqlComparisonOperator Operator, string Text, RsqlTextSpan Span) op)
    {
        if (RequiresParenthesizedValueList(op) && _current.Kind != RsqlTokenKind.OpenParen)
        {
            AddUnexpectedToken("parenthesized value list");
            return [];
        }

        if (_current.Kind == RsqlTokenKind.OpenParen)
        {
            return ParseValueList();
        }

        var value = ParseValue();
        return value is null ? [] : [value];
    }

    private bool RequiresParenthesizedValueList((RsqlComparisonOperator Operator, string Text, RsqlTextSpan Span) op)
    {
        if (op.Operator is RsqlComparisonOperator.In or RsqlComparisonOperator.NotIn)
        {
            return true;
        }

        return op.Operator == RsqlComparisonOperator.Custom
            && _options.CustomOperators.Any(x =>
                x.RequiresMultipleValues &&
                string.Equals(x.Text, op.Text, StringComparison.OrdinalIgnoreCase));
    }

    private List<RsqlValue> ParseValueList()
    {
        Next();
        var values = new List<RsqlValue>();

        if (_current.Kind == RsqlTokenKind.CloseParen)
        {
            AddUnexpectedToken("value");
            return values;
        }

        while (_current.Kind != RsqlTokenKind.EndOfInput && _current.Kind != RsqlTokenKind.CloseParen)
        {
            if (_current.Kind == RsqlTokenKind.Comma)
            {
                AddUnexpectedToken("value");
                break;
            }

            var value = ParseValue();
            if (value is not null)
            {
                values.Add(value);
            }

            if (_current.Kind == RsqlTokenKind.Comma)
            {
                Next();
                if (_current.Kind is RsqlTokenKind.CloseParen or RsqlTokenKind.EndOfInput)
                {
                    AddUnexpectedToken("value");
                    break;
                }

                continue;
            }

            if (_current.Kind != RsqlTokenKind.CloseParen)
            {
                AddUnexpectedToken("',' or ')'");
                break;
            }
        }

        Expect(RsqlTokenKind.CloseParen, "')'");
        return values;
    }

    private RsqlValue? ParseValue()
    {
        if (_current.Kind is not (RsqlTokenKind.Identifier or RsqlTokenKind.String))
        {
            AddUnexpectedToken("value");
            return null;
        }

        var token = _current;
        Next();

        if (token.Kind == RsqlTokenKind.String)
        {
            return new RsqlValue(RsqlValueKind.String, token.Text, token.RawText, token.Span);
        }

        if (bool.TryParse(token.Text, out var boolean))
        {
            return new RsqlValue(RsqlValueKind.Boolean, boolean.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(), token.RawText, token.Span);
        }

        if (string.Equals(token.Text, "null", StringComparison.OrdinalIgnoreCase))
        {
            return new RsqlValue(RsqlValueKind.Null, null, token.RawText, token.Span);
        }

        if (decimal.TryParse(token.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            return new RsqlValue(RsqlValueKind.Number, token.Text, token.RawText, token.Span);
        }

        return new RsqlValue(RsqlValueKind.String, token.Text, token.RawText, token.Span);
    }

    private RsqlToken? Expect(RsqlTokenKind kind, string expected)
    {
        if (_current.Kind == kind)
        {
            var token = _current;
            Next();
            return token;
        }

        AddUnexpectedToken(expected);
        return null;
    }

    private void Next()
    {
        do
        {
            _current = _tokenizer.Next();
        }
        while (_current.Kind == RsqlTokenKind.Invalid && _current.Kind != RsqlTokenKind.EndOfInput);
    }

    private void AddUnexpectedToken(string expected)
    {
        _diagnostics.Add(_sourceText.CreateDiagnostic(
            RsqlDiagnosticCodes.UnexpectedToken,
            string.Format(
                CultureInfo.InvariantCulture,
                "Expected {0} at position {1}.",
                expected,
                _current.Span.Start),
            _current.Span));
    }

    private void AddInvalidSelector(RsqlToken selector)
    {
        _diagnostics.Add(_sourceText.CreateDiagnostic(
            RsqlDiagnosticCodes.InvalidSelector,
            string.Format(
                CultureInfo.InvariantCulture,
                "Selector '{0}' is not valid at position {1}.",
                selector.Text,
                selector.Span.Start),
            selector.Span));
    }

    private static bool IsPrimaryStart(RsqlTokenKind kind)
    {
        return kind is RsqlTokenKind.Identifier or RsqlTokenKind.OpenParen;
    }

    private static RsqlLogicalNode CreateLogicalNode(RsqlLogicalOperator op, RsqlNode left, RsqlNode right)
    {
        var children = new List<RsqlNode>();
        AddLogicalChild(children, op, left);
        AddLogicalChild(children, op, right);

        return new RsqlLogicalNode(
            op,
            children,
            RsqlTextSpan.FromBounds(left.Span.Start, right.Span.End));
    }

    private static void AddLogicalChild(List<RsqlNode> children, RsqlLogicalOperator op, RsqlNode node)
    {
        if (node is RsqlLogicalNode logicalNode && logicalNode.Operator == op)
        {
            children.AddRange(logicalNode.Children);
            return;
        }

        children.Add(node);
    }

    private static RsqlParseResult Failure(RsqlDiagnostic diagnostic)
    {
        return new RsqlParseResult(null, [diagnostic]);
    }
}
