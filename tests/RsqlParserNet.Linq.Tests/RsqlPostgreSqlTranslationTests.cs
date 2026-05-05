using Microsoft.EntityFrameworkCore;

namespace RsqlParserNet.Linq.Tests;

public sealed class RsqlPostgreSqlTranslationTests
{
    [Fact]
    public void ApplyRsql_TranslatesScalarOperatorsWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplyRsql("status==active;count>=10", new PostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("WHERE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("active", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyRsql_TranslatesInOperatorWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplyRsql("status=in=(active,draft)", new PostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("active", sql, StringComparison.Ordinal);
        Assert.Contains("draft", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyRsql_TranslatesWildcardOperatorWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplyRsql("name==B*", new PostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyRsql_TranslatesStringContainsOperatorWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplyRsql("name=contains=ik", new PostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyRsql_TranslatesCaseInsensitiveStringModeWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplyRsql("name=contains=IK", new CaseInsensitivePostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("upper", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplySort_TranslatesMultiSortWithPostgreSql()
    {
        using var context = CreateContext();

        var sql = context.Products
            .ApplySort(RsqlSortRequest.ParseMany("-count,name"), new PostgresProductRsqlProfile())
            .ToQueryString();

        Assert.Contains("ORDER BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DESC", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static PostgresProductDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PostgresProductDbContext>()
            .UseNpgsql("Host=localhost;Database=rsql_parser_net_translation;Username=postgres;Password=postgres")
            .Options;

        return new PostgresProductDbContext(options);
    }

    private class PostgresProductRsqlProfile : RsqlLinqProfile<PostgresProduct>
    {
        public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
        {
            return options.WithLinqOperators();
        }

        public override void Configure(RsqlLinqOptions<PostgresProduct> options)
        {
            options.Allow("name", x => x.Name);
            options.Allow("status", x => x.Status);
            options.Allow("count", x => x.Count);
            options.AllowStringContainsOperator();
            options.AllowStringStartsWithOperator();
            options.AllowStringEndsWithOperator();
        }
    }

    private sealed class CaseInsensitivePostgresProductRsqlProfile : PostgresProductRsqlProfile
    {
        public override void Configure(RsqlLinqOptions<PostgresProduct> options)
        {
            options.StringComparisonMode = RsqlStringComparisonMode.CaseInsensitive;
            base.Configure(options);
        }
    }

    private sealed class PostgresProductDbContext(DbContextOptions<PostgresProductDbContext> options)
        : DbContext(options)
    {
        public DbSet<PostgresProduct> Products => Set<PostgresProduct>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostgresProduct>().ToTable("products");
        }
    }

    private sealed class PostgresProduct
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
