using RsqlParserNet.Linq;

namespace RsqlParserNet.Linq.Tests;

public sealed class RsqlQueryableExtensionsTests
{
    [Fact]
    public void ApplyRsql_ParsesAndFiltersExpressionText()
    {
        var result = SampleProducts()
            .ApplyRsql("status==active", options => options.Allow("status", x => x.Status))
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Bike", productName);
    }

    [Fact]
    public void ApplyRsql_FiltersByAllowlistedStringEquality()
    {
        var products = new[]
        {
            new Product("Bike", "active", 10, true, Category.Gear, new DateOnly(2026, 1, 10), null),
            new Product("Board", "draft", 20, false, Category.Board, new DateOnly(2026, 2, 10), "archived")
        }.AsQueryable();
        var query = RsqlParser.Parse("status==active");

        var result = products.ApplyRsql(query, options => options.Allow("status", x => x.Status)).ToArray();

        var product = Assert.Single(result);
        Assert.Equal("Bike", product.Name);
    }

    [Fact]
    public void ApplyRsql_FiltersByAllowlistedNumericEquality()
    {
        var products = new[]
        {
            new Product("Bike", "active", 10, true, Category.Gear, new DateOnly(2026, 1, 10), null),
            new Product("Board", "draft", 20, false, Category.Board, new DateOnly(2026, 2, 10), "archived")
        }.AsQueryable();
        var query = RsqlParser.Parse("count==20");

        var result = products.ApplyRsql(query, options => options.Allow("count", x => x.Count)).ToArray();

        var product = Assert.Single(result);
        Assert.Equal("Board", product.Name);
    }

    [Fact]
    public void ApplyRsql_FiltersLogicalAnd()
    {
        var products = new[]
        {
            new Product("Bike", "active", 10, true, Category.Gear, new DateOnly(2026, 1, 10), null),
            new Product("Board", "active", 20, false, Category.Board, new DateOnly(2026, 2, 10), "archived"),
            new Product("Helmet", "draft", 10, true, Category.Gear, new DateOnly(2026, 3, 10), null)
        }.AsQueryable();
        var query = RsqlParser.Parse("status==active;count==10");

        var result = products.ApplyRsql(query, options =>
        {
            options.Allow("status", x => x.Status);
            options.Allow("count", x => x.Count);
        }).ToArray();

        var product = Assert.Single(result);
        Assert.Equal("Bike", product.Name);
    }

    [Fact]
    public void ApplyRsql_FiltersByNotEqualOperator()
    {
        var query = RsqlParser.Parse("status!=draft");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("status", x => x.Status))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Helmet"], result);
    }

    [Theory]
    [InlineData("name==B*", "Bike", "Board")]
    [InlineData("name==*met", "Helmet")]
    [InlineData("name==*ik*", "Bike")]
    [InlineData("name==Bo*d", "Board")]
    public void ApplyRsql_FiltersStringWildcards(string expression, params string[] expectedNames)
    {
        var result = SampleProducts()
            .ApplyRsql(expression, options => options.Allow("name", x => x.Name))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(expectedNames, result);
    }

    [Fact]
    public void ApplyRsql_NegatesStringWildcardForNotEqualOperator()
    {
        var result = SampleProducts()
            .ApplyRsql("name!=B*", options => options.Allow("name", x => x.Name))
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Helmet", productName);
    }

    [Fact]
    public void ApplyRsql_TreatsWildcardAsLiteralWhenWildcardModeIsDisabled()
    {
        var result = SampleProducts()
            .ApplyRsql("name==B*", options =>
            {
                options.StringWildcardMode = RsqlStringWildcardMode.Disabled;
                options.Allow("name", x => x.Name);
            })
            .Select(x => x.Name)
            .ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyRsql_RejectsUnsupportedStringWildcardPattern()
    {
        Assert.Throws<RsqlLinqException>(() => SampleProducts()
            .ApplyRsql("name==*i*k*", options => options.Allow("name", x => x.Name))
            .ToArray());
    }

    [Fact]
    public void ApplyRsql_FiltersLogicalOr()
    {
        var products = SampleProducts();
        var query = RsqlParser.Parse("status==draft,count>20");

        var result = products.ApplyRsql(query, options =>
        {
            options.Allow("status", x => x.Status);
            options.Allow("count", x => x.Count);
        }).Select(x => x.Name).ToArray();

        Assert.Equal(["Board", "Helmet"], result);
    }

    [Theory]
    [InlineData("count>10", "Board", "Helmet")]
    [InlineData("count>=20", "Board", "Helmet")]
    [InlineData("count<20", "Bike")]
    [InlineData("count<=20", "Bike", "Board")]
    public void ApplyRsql_FiltersByNumericComparison(string expression, params string[] expectedNames)
    {
        var query = RsqlParser.Parse(expression);

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("count", x => x.Count))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(expectedNames, result);
    }

    [Fact]
    public void ApplyRsql_FiltersByInOperator()
    {
        var query = RsqlParser.Parse("status=in=(active,draft)");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("status", x => x.Status))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Board"], result);
    }

    [Fact]
    public void ApplyRsql_FiltersByNotInOperator()
    {
        var query = RsqlParser.Parse("status=out=(active,draft)");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("status", x => x.Status))
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Helmet", productName);
    }

    [Fact]
    public void ApplyRsql_FiltersByBooleanValue()
    {
        var query = RsqlParser.Parse("active==true");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("active", x => x.Active))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Helmet"], result);
    }

    [Fact]
    public void ApplyRsql_FiltersByEnumValue()
    {
        var query = RsqlParser.Parse("category==board");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("category", x => x.Category))
            .Select(x => x.Name)
            .ToArray();

        var product = Assert.Single(result);
        Assert.Equal("Board", product);
    }

    [Fact]
    public void ApplyRsql_FiltersByDateOnlyValue()
    {
        var query = RsqlParser.Parse("createdAt>=2026-02-01");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("createdAt", x => x.CreatedAt))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Board", "Helmet"], result);
    }

    [Fact]
    public void ApplyRsql_FiltersByNullableNullEquality()
    {
        var query = RsqlParser.Parse("archivedReason==null");

        var result = SampleProducts()
            .ApplyRsql(query, options => options.Allow("archivedReason", x => x.ArchivedReason))
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Helmet"], result);
    }

    [Fact]
    public void ApplyRsql_RejectsUnmappedSelector()
    {
        var products = Array.Empty<Product>().AsQueryable();
        var query = RsqlParser.Parse("status==active");

        Assert.Throws<RsqlLinqException>(() => products.ApplyRsql(query, _ => { }).ToArray());
    }

    [Fact]
    public void ApplyRsql_UsesCaseSensitiveSelectorMappings()
    {
        var query = RsqlParser.Parse("status==active");

        Assert.Throws<RsqlLinqException>(() => SampleProducts()
            .ApplyRsql(query, options => options.Allow("Status", x => x.Status))
            .ToArray());
    }

    [Fact]
    public void ApplyRsql_RejectsNullForNonNullableValueType()
    {
        var query = RsqlParser.Parse("count==null");

        Assert.Throws<RsqlLinqException>(() => SampleProducts()
            .ApplyRsql(query, options => options.Allow("count", x => x.Count))
            .ToArray());
    }

    [Fact]
    public void ApplyRsql_RejectsInvalidValueConversion()
    {
        var query = RsqlParser.Parse("count==abc");

        Assert.Throws<RsqlLinqException>(() => SampleProducts()
            .ApplyRsql(query, options => options.Allow("count", x => x.Count))
            .ToArray());
    }

    [Fact]
    public void ApplyRsql_RejectsCustomOperator()
    {
        var options = RsqlParseOptions.Default with
        {
            CustomOperators = [new RsqlCustomOperator("=contains=")]
        };
        var query = RsqlParser.Parse("status=contains=active", options);

        Assert.Throws<RsqlLinqException>(() => SampleProducts()
            .ApplyRsql(query, linqOptions => linqOptions.Allow("status", x => x.Status))
            .ToArray());
    }

    private static IQueryable<Product> SampleProducts()
    {
        return new[]
        {
            new Product("Bike", "active", 10, true, Category.Gear, new DateOnly(2026, 1, 10), null),
            new Product("Board", "draft", 20, false, Category.Board, new DateOnly(2026, 2, 10), "archived"),
            new Product("Helmet", "review", 30, true, Category.Gear, new DateOnly(2026, 3, 10), null)
        }.AsQueryable();
    }

    private enum Category
    {
        Gear,
        Board
    }

    private sealed record Product(
        string Name,
        string Status,
        int Count,
        bool Active,
        Category Category,
        DateOnly CreatedAt,
        string? ArchivedReason);
}
