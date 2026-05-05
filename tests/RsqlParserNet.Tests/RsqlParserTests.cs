namespace RsqlParserNet.Tests;

public sealed class RsqlParserTests
{
    [Fact]
    public void Parse_ReturnsComparisonAstForQuotedString()
    {
        var query = RsqlParser.Parse("status==\"active\"");

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal("status==\"active\"", query.Expression);
        Assert.Equal("status", comparison.Selector);
        Assert.Equal(RsqlComparisonOperator.Equal, comparison.Operator);
        Assert.Equal("==", comparison.OperatorText);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(RsqlValueKind.String, value.Kind);
        Assert.Equal("active", value.Text);
        Assert.Equal("\"active\"", value.RawText);
        Assert.Equal(new RsqlTextSpan(0, 16), comparison.Span);
    }

    [Fact]
    public void Parse_ReturnsComparisonAstForUnquotedString()
    {
        var query = RsqlParser.Parse("status==active");

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal("status", comparison.Selector);
        Assert.Equal(RsqlComparisonOperator.Equal, comparison.Operator);
        Assert.Equal("==", comparison.OperatorText);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(RsqlValueKind.String, value.Kind);
        Assert.Equal("active", value.Text);
        Assert.Equal("active", value.RawText);
    }

    [Theory]
    [InlineData("count>10", RsqlComparisonOperator.GreaterThan, RsqlValueKind.Number, "10")]
    [InlineData("count>=10", RsqlComparisonOperator.GreaterThanOrEqual, RsqlValueKind.Number, "10")]
    [InlineData("count<10", RsqlComparisonOperator.LessThan, RsqlValueKind.Number, "10")]
    [InlineData("count<=10", RsqlComparisonOperator.LessThanOrEqual, RsqlValueKind.Number, "10")]
    [InlineData("enabled==true", RsqlComparisonOperator.Equal, RsqlValueKind.Boolean, "true")]
    [InlineData("deletedAt==null", RsqlComparisonOperator.Equal, RsqlValueKind.Null, null)]
    [InlineData("status!=draft", RsqlComparisonOperator.NotEqual, RsqlValueKind.String, "draft")]
    [InlineData("count=gt=10", RsqlComparisonOperator.GreaterThan, RsqlValueKind.Number, "10")]
    [InlineData("count=ge=10", RsqlComparisonOperator.GreaterThanOrEqual, RsqlValueKind.Number, "10")]
    [InlineData("count=lt=10", RsqlComparisonOperator.LessThan, RsqlValueKind.Number, "10")]
    [InlineData("count=le=10", RsqlComparisonOperator.LessThanOrEqual, RsqlValueKind.Number, "10")]
    public void Parse_MapsOperatorsAndValueKinds(
        string expression,
        RsqlComparisonOperator expectedOperator,
        RsqlValueKind expectedValueKind,
        string? expectedText)
    {
        var query = RsqlParser.Parse(expression);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal(expectedOperator, comparison.Operator);
        Assert.NotEmpty(comparison.OperatorText);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(expectedValueKind, value.Kind);
        Assert.Equal(expectedText, value.Text);
        Assert.Equal(expression[(expression.LastIndexOfAny(['=', '>', '<']) + 1)..], value.RawText);
    }

    [Theory]
    [InlineData("status=in=(active,draft)", RsqlComparisonOperator.In, "active", "draft")]
    [InlineData("status=out=(archived,deleted)", RsqlComparisonOperator.NotIn, "archived", "deleted")]
    public void Parse_ReturnsMultiValueComparison(
        string expression,
        RsqlComparisonOperator expectedOperator,
        string firstValue,
        string secondValue)
    {
        var query = RsqlParser.Parse(expression);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal(expectedOperator, comparison.Operator);
        Assert.False(comparison.IsCustomOperator);
        Assert.True(comparison.HasMultipleValues);
        Assert.Equal([firstValue, secondValue], comparison.Values.Select(x => x.Text!).ToArray());
    }

    [Fact]
    public void Parse_UnescapesQuotedString()
    {
        var query = RsqlParser.Parse("title==\"SUP\\\" board\"");

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        var value = Assert.Single(comparison.Values);
        Assert.Equal("SUP\" board", value.Text);
        Assert.Equal("\"SUP\\\" board\"", value.RawText);
    }

    [Fact]
    public void Parse_UnescapesSingleQuotedString()
    {
        var query = RsqlParser.Parse("title=='SUP\\' board'");

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        var value = Assert.Single(comparison.Values);
        Assert.Equal("SUP' board", value.Text);
        Assert.Equal("'SUP\\' board'", value.RawText);
    }

    [Fact]
    public void Parse_AllowsWhitespaceAroundComparisonOperator()
    {
        var query = RsqlParser.Parse(" status == 'active' ");

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal("status", comparison.Selector);
        Assert.Equal(RsqlComparisonOperator.Equal, comparison.Operator);
        Assert.Equal(new RsqlTextSpan(1, 18), comparison.Span);
        var value = Assert.Single(comparison.Values);
        Assert.Equal("active", value.Text);
    }

    [Fact]
    public void Parse_ReturnsAndLogicalNodeForSemicolon()
    {
        var query = RsqlParser.Parse("status==active;title==Bike");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.And, logical.Operator);
        Assert.Equal(2, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "title", RsqlComparisonOperator.Equal, "Bike");
    }

    [Fact]
    public void Parse_ReturnsOrLogicalNodeForComma()
    {
        var query = RsqlParser.Parse("status==active,status==draft");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.Or, logical.Operator);
        Assert.Equal(2, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "status", RsqlComparisonOperator.Equal, "draft");
    }

    [Fact]
    public void Parse_FlattensAdjacentAndExpressions()
    {
        var query = RsqlParser.Parse("status==active;title==Bike;count>10");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.And, logical.Operator);
        Assert.Equal(3, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "title", RsqlComparisonOperator.Equal, "Bike");
        AssertComparison(logical.Children[2], "count", RsqlComparisonOperator.GreaterThan, "10");
    }

    [Fact]
    public void Parse_FlattensAdjacentOrExpressions()
    {
        var query = RsqlParser.Parse("status==active,status==draft,status==archived");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.Or, logical.Operator);
        Assert.Equal(3, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "status", RsqlComparisonOperator.Equal, "draft");
        AssertComparison(logical.Children[2], "status", RsqlComparisonOperator.Equal, "archived");
    }

    [Fact]
    public void Parse_GivesAndPrecedenceOverOr()
    {
        var query = RsqlParser.Parse("status==active;title==Bike,status==draft");

        var root = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.Or, root.Operator);
        Assert.Equal(2, root.Children.Count);

        var left = Assert.IsType<RsqlLogicalNode>(root.Children[0]);
        Assert.Equal(RsqlLogicalOperator.And, left.Operator);
        Assert.Equal(2, left.Children.Count);
        AssertComparison(left.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(left.Children[1], "title", RsqlComparisonOperator.Equal, "Bike");
        AssertComparison(root.Children[1], "status", RsqlComparisonOperator.Equal, "draft");
    }

    [Fact]
    public void Parse_ParenthesesOverridePrecedence()
    {
        var query = RsqlParser.Parse("status==active;(title==Bike,status==draft)");

        var root = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.And, root.Operator);
        Assert.Equal(2, root.Children.Count);
        AssertComparison(root.Children[0], "status", RsqlComparisonOperator.Equal, "active");

        var right = Assert.IsType<RsqlLogicalNode>(root.Children[1]);
        Assert.Equal(RsqlLogicalOperator.Or, right.Operator);
        Assert.Equal(new RsqlTextSpan(15, 27), right.Span);
        Assert.Equal(2, right.Children.Count);
        AssertComparison(right.Children[0], "title", RsqlComparisonOperator.Equal, "Bike");
        AssertComparison(right.Children[1], "status", RsqlComparisonOperator.Equal, "draft");
    }

    [Fact]
    public void Parse_SupportsWordAndOperator()
    {
        var query = RsqlParser.Parse("status==active and title==Bike");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.And, logical.Operator);
        Assert.Equal(2, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "title", RsqlComparisonOperator.Equal, "Bike");
    }

    [Fact]
    public void Parse_SupportsWordOrOperator()
    {
        var query = RsqlParser.Parse("status==active OR status==draft");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.Or, logical.Operator);
        Assert.Equal(2, logical.Children.Count);
        AssertComparison(logical.Children[0], "status", RsqlComparisonOperator.Equal, "active");
        AssertComparison(logical.Children[1], "status", RsqlComparisonOperator.Equal, "draft");
    }

    [Fact]
    public void Parse_UsesDefaultOptionsWhenOptionsAreOmitted()
    {
        var query = RsqlParser.Parse("status==active and title==Bike");

        var logical = Assert.IsType<RsqlLogicalNode>(query.Root);
        Assert.Equal(RsqlLogicalOperator.And, logical.Operator);
    }

    [Fact]
    public void TryParse_RejectsWordLogicalOperatorsWhenDisabled()
    {
        var options = RsqlParseOptions.Default with { AllowWordLogicalOperators = false };

        var result = RsqlParser.TryParse("status==active and title==Bike", options);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken);
    }

    [Fact]
    public void TryParse_AllowsSymbolicLogicalOperatorsWhenWordOperatorsAreDisabled()
    {
        var options = RsqlParseOptions.Default with { AllowWordLogicalOperators = false };

        var result = RsqlParser.TryParse("status==active;title==Bike", options);

        Assert.True(result.Success);
        var logical = Assert.IsType<RsqlLogicalNode>(result.Query!.Root);
        Assert.Equal(RsqlLogicalOperator.And, logical.Operator);
    }

    [Fact]
    public void Parse_TreatsAndAsValueWhenNotStandaloneLogicalOperator()
    {
        var query = RsqlParser.Parse("status==and");

        AssertComparison(query.Root, "status", RsqlComparisonOperator.Equal, "and");
    }

    [Fact]
    public void Parse_TreatsAndAsSelectorWhenNotStandaloneLogicalOperator()
    {
        var query = RsqlParser.Parse("and==true");

        AssertComparison(query.Root, "and", RsqlComparisonOperator.Equal, "true");
    }

    [Theory]
    [InlineData("status==active", "status")]
    [InlineData("_status==active", "_status")]
    [InlineData("product-id==10", "product-id")]
    [InlineData("customer.name==Ada", "customer.name")]
    [InlineData("customer.primary-address.city==London", "customer.primary-address.city")]
    public void Parse_AcceptsValidSelectors(string expression, string expectedSelector)
    {
        var query = RsqlParser.Parse(expression);

        AssertComparison(query.Root, expectedSelector, RsqlComparisonOperator.Equal, query.Root is RsqlComparisonNode node ? node.Values[0].Text : null);
    }

    [Theory]
    [InlineData("actor==*Bale", "*Bale")]
    [InlineData("director==Que*Tarantino", "Que*Tarantino")]
    [InlineData("title==\"SUP*\"", "SUP*")]
    public void Parse_PreservesWildcardCharactersAsStringText(string expression, string expectedText)
    {
        var query = RsqlParser.Parse(expression);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(RsqlValueKind.String, value.Kind);
        Assert.Equal(expectedText, value.Text);
    }

    [Theory]
    [InlineData("createdAt==2026-01-01", "2026-01-01")]
    [InlineData("createdAt>=2026-01-01T10:15:30Z", "2026-01-01T10:15:30Z")]
    public void Parse_PreservesDateLikeValuesAsStringText(string expression, string expectedText)
    {
        var query = RsqlParser.Parse(expression);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(RsqlValueKind.String, value.Kind);
        Assert.Equal(expectedText, value.Text);
        Assert.Equal(expectedText, value.RawText);
    }

    [Theory]
    [InlineData("name==\"Kill Bill\";year=gt=2003")]
    [InlineData("name==\"Kill Bill\" and year>2003")]
    [InlineData("genres=in=(sci-fi,action);(director=='Christopher Nolan',actor==*Bale);year=ge=2000")]
    [InlineData("genres=in=(sci-fi,action) and (director=='Christopher Nolan' or actor==*Bale) and year>=2000")]
    [InlineData("director.lastName==Nolan;year=ge=2000;year=lt=2010")]
    [InlineData("director.lastName==Nolan and year>=2000 and year<2010")]
    [InlineData("genres=in=(sci-fi,action);genres=out=(romance,animated,horror),director==Que*Tarantino")]
    [InlineData("genres=in=(sci-fi,action) and genres=out=(romance,animated,horror) or director==Que*Tarantino")]
    public void Parse_AcceptsOriginalJavaRsqlParserReadmeExamples(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.NotEmpty(result.Query.Root.Comparisons());
    }

    [Fact]
    public void Parse_AcceptsConfiguredCustomOperator()
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator("=contains=")]
        };

        var query = RsqlParser.Parse("title=contains=Bike", options);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal("title", comparison.Selector);
        Assert.Equal(RsqlComparisonOperator.Custom, comparison.Operator);
        Assert.True(comparison.IsCustomOperator);
        Assert.False(comparison.HasMultipleValues);
        Assert.Equal("=contains=", comparison.OperatorText);
        var value = Assert.Single(comparison.Values);
        Assert.Equal("Bike", value.Text);
    }

    [Fact]
    public void TryParse_RejectsUnconfiguredCustomOperator()
    {
        var result = RsqlParser.TryParse("title=contains=Bike");

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.InvalidToken);
    }

    [Theory]
    [InlineData("status=in=active")]
    [InlineData("status=out=archived")]
    public void TryParse_ReturnsDiagnosticWhenMultiValueBuiltInOperatorHasSingleValue(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x =>
            x.Code == RsqlDiagnosticCodes.UnexpectedToken &&
            x.Message.StartsWith("Expected parenthesized value list", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_AcceptsConfiguredCustomOperatorThatRequiresMultipleValues()
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator("=all=", RequiresMultipleValues: true)]
        };

        var query = RsqlParser.Parse("tags=all=(green,fast)", options);

        var comparison = Assert.IsType<RsqlComparisonNode>(query.Root);
        Assert.Equal(RsqlComparisonOperator.Custom, comparison.Operator);
        Assert.Equal("=all=", comparison.OperatorText);
        Assert.Equal(["green", "fast"], comparison.Values.Select(x => x.Text!).ToArray());
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticWhenCustomMultiValueOperatorHasSingleValue()
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator("=all=", RequiresMultipleValues: true)]
        };

        var result = RsqlParser.TryParse("tags=all=green", options);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x =>
            x.Code == RsqlDiagnosticCodes.UnexpectedToken &&
            x.Message.StartsWith("Expected parenthesized value list", StringComparison.Ordinal));
    }

    [Fact]
    public void TryParse_ThrowsForNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => RsqlParser.TryParse("status==active", options: null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("contains")]
    [InlineData("=contains")]
    [InlineData("contains=")]
    [InlineData("=contains!=")]
    [InlineData("=contains_=")]
    public void TryParse_ThrowsForInvalidCustomOperatorConfiguration(string operatorText)
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator(operatorText)]
        };

        Assert.Throws<ArgumentException>(() => RsqlParser.TryParse("status==active", options));
    }

    [Theory]
    [InlineData("==")]
    [InlineData("=in=")]
    [InlineData("=gt=")]
    public void TryParse_ThrowsWhenCustomOperatorConflictsWithBuiltInOperator(string operatorText)
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator(operatorText)]
        };

        Assert.Throws<ArgumentException>(() => RsqlParser.TryParse("status==active", options));
    }

    [Fact]
    public void TryParse_ThrowsWhenCustomOperatorIsDuplicated()
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators =
            [
                new RsqlCustomOperator("=contains="),
                new RsqlCustomOperator("=CONTAINS=")
            ]
        };

        Assert.Throws<ArgumentException>(() => RsqlParser.TryParse("status==active", options));
    }

    [Theory]
    [InlineData("1status==active")]
    [InlineData("-status==active")]
    [InlineData("customer..name==Ada")]
    [InlineData("customer.==Ada")]
    [InlineData(".customer==Ada")]
    public void TryParse_ReturnsDiagnosticForInvalidSelector(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.InvalidSelector);
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticForDottedSelectorWhenDisabled()
    {
        var options = RsqlParseOptions.Default with { AllowDottedSelectors = false };

        var result = RsqlParser.TryParse("customer.name==Ada", options);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.InvalidSelector);
    }

    [Fact]
    public void Parse_ThrowsForEmptyExpression()
    {
        Assert.Throws<ArgumentException>(() => RsqlParser.Parse(" "));
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticForMissingValue()
    {
        var result = RsqlParser.TryParse("status==");

        Assert.False(result.Success);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(RsqlDiagnosticCodes.UnexpectedToken, diagnostic.Code);
        Assert.Equal(new RsqlTextSpan(8, 0), diagnostic.Span);
        Assert.Equal(new RsqlSourceLocation(8, 0, 8), diagnostic.Start);
        Assert.Equal(new RsqlSourceLocation(8, 0, 8), diagnostic.End);
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticForUnterminatedString()
    {
        var result = RsqlParser.TryParse("status==\"active");

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.InvalidToken);
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticForMissingClosingParenthesis()
    {
        var result = RsqlParser.TryParse("(status==active");

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken && x.Span == new RsqlTextSpan(15, 0));
    }

    [Fact]
    public void TryParse_ReturnsDiagnosticSourceLocationForMultilineExpression()
    {
        var result = RsqlParser.TryParse("status==active;\n  title==");

        Assert.False(result.Success);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(RsqlDiagnosticCodes.UnexpectedToken, diagnostic.Code);
        Assert.Equal(new RsqlTextSpan(25, 0), diagnostic.Span);
        Assert.Equal(new RsqlSourceLocation(25, 1, 9), diagnostic.Start);
        Assert.Equal(new RsqlSourceLocation(25, 1, 9), diagnostic.End);
    }

    [Theory]
    [InlineData("status==active;")]
    [InlineData("status==active,")]
    [InlineData("status==active and ")]
    [InlineData("status==active or ")]
    public void TryParse_ReturnsDiagnosticForDanglingLogicalOperator(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken && x.Message.StartsWith("Expected expression", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("()")]
    [InlineData("( )")]
    [InlineData("status==active;;title==Bike")]
    [InlineData("status==active,,status==draft")]
    [InlineData("status==active;)")]
    [InlineData("status==active,)")]
    public void TryParse_ReturnsDiagnosticForMissingExpression(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken && x.Message.StartsWith("Expected expression", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("status=in=()")]
    [InlineData("status=in=(,active)")]
    [InlineData("status=in=(active,)")]
    [InlineData("status=out=()")]
    public void TryParse_ReturnsDiagnosticForMissingListValue(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken && x.Message.StartsWith("Expected value", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(")")]
    [InlineData(",status==draft")]
    [InlineData(";status==active")]
    public void TryParse_ReturnsDiagnosticForUnexpectedLeadingToken(string expression)
    {
        var result = RsqlParser.TryParse(expression);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, x => x.Code == RsqlDiagnosticCodes.UnexpectedToken);
    }

    [Fact]
    public void DescendantsAndSelf_ReturnsNodesInPreOrder()
    {
        var query = RsqlParser.Parse("status==active;title==Bike,status==draft");

        var nodes = query.Root.DescendantsAndSelf().ToArray();

        Assert.Collection(
            nodes,
            node => Assert.Equal(RsqlLogicalOperator.Or, Assert.IsType<RsqlLogicalNode>(node).Operator),
            node => Assert.Equal(RsqlLogicalOperator.And, Assert.IsType<RsqlLogicalNode>(node).Operator),
            node => AssertComparison(node, "status", RsqlComparisonOperator.Equal, "active"),
            node => AssertComparison(node, "title", RsqlComparisonOperator.Equal, "Bike"),
            node => AssertComparison(node, "status", RsqlComparisonOperator.Equal, "draft"));
    }

    [Fact]
    public void Comparisons_ReturnsComparisonNodesInPreOrder()
    {
        var query = RsqlParser.Parse("status==active;title==Bike,status==draft");

        var selectors = query.Root.Comparisons().Select(x => x.Selector).ToArray();

        Assert.Equal(["status", "title", "status"], selectors);
    }

    private static void AssertComparison(
        RsqlNode node,
        string expectedSelector,
        RsqlComparisonOperator expectedOperator,
        string? expectedValue)
    {
        var comparison = Assert.IsType<RsqlComparisonNode>(node);
        Assert.Equal(expectedSelector, comparison.Selector);
        Assert.Equal(expectedOperator, comparison.Operator);
        var value = Assert.Single(comparison.Values);
        Assert.Equal(expectedValue, value.Text);
    }
}
