using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RsqlParserNet.Linq;

namespace RsqlParserNet.EntityFrameworkCore.Tests;

public sealed class RsqlEntityFrameworkQueryableExtensionsTests
{
    [Fact]
    public async Task ToRsqlPageAsync_ReturnsPagedResult()
    {
        using var database = SqliteProductDatabase.Create();

        var result = await database.Context.Products
            .OrderBy(product => product.Id)
            .ToRsqlPageAsync(new RsqlPageRequest(page: 2, pageSize: 1));

        var product = Assert.Single(result.Items);
        Assert.Equal("Board", product.Name);
        Assert.Equal(2, result.Pagination.Page);
        Assert.Equal(1, result.Pagination.PageSize);
        Assert.Equal(3, result.Pagination.TotalItems);
        Assert.Equal(3, result.Pagination.TotalPages);
        Assert.True(result.Pagination.HasPreviousPage);
        Assert.True(result.Pagination.HasNextPage);
    }

    [Fact]
    public async Task ToRsqlPageAsync_AppliesParsedRsqlQueryBeforePaging()
    {
        using var database = SqliteProductDatabase.Create();
        var query = RsqlParser.Parse("status==active");

        var result = await database.Context.Products
            .OrderBy(product => product.Id)
            .ToRsqlPageAsync(query, new SqliteProductRsqlProfile(), new RsqlPageRequest(page: 1, pageSize: 10));

        var product = Assert.Single(result.Items);
        Assert.Equal("Bike", product.Name);
        Assert.Equal(1, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task ToRsqlPageAsync_AppliesSortBeforePaging()
    {
        using var database = SqliteProductDatabase.Create();

        var result = await database.Context.Products
            .ToRsqlPageAsync(
                RsqlSortRequest.Parse("-name"),
                new SqliteProductRsqlProfile(),
                new RsqlPageRequest(page: 1, pageSize: 2));

        Assert.Equal(["Helmet", "Board"], result.Items.Select(product => product.Name).ToArray());
        Assert.Equal(3, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task ToRsqlPageAsync_AppliesParsedRsqlQueryAndSortBeforePaging()
    {
        using var database = SqliteProductDatabase.Create();
        var query = RsqlParser.Parse("name=starts=B", new SqliteProductRsqlProfile().ConfigureParseOptions(RsqlParseOptions.Default));

        var result = await database.Context.Products
            .ToRsqlPageAsync(
                query,
                RsqlSortRequest.Parse("-name"),
                new SqliteProductRsqlProfile(),
                new RsqlPageRequest(page: 1, pageSize: 10));

        Assert.Equal(["Board", "Bike"], result.Items.Select(product => product.Name).ToArray());
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task ToRsqlPageAsync_ParsesAndAppliesExpressionBeforePaging()
    {
        using var database = SqliteProductDatabase.Create();

        var result = await database.Context.Products
            .OrderBy(product => product.Id)
            .ToRsqlPageAsync("name=starts=B", new SqliteProductRsqlProfile(), new RsqlPageRequest(page: 1, pageSize: 10));

        Assert.Equal(["Bike", "Board"], result.Items.Select(product => product.Name).ToArray());
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    private sealed class SqliteProductRsqlProfile : RsqlLinqProfile<SqliteProduct>
    {
        public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
        {
            return options.WithLinqOperators();
        }

        public override void Configure(RsqlLinqOptions<SqliteProduct> options)
        {
            options.Allow("name", product => product.Name);
            options.Allow("status", product => product.Status);
            options.AllowStringStartsWithOperator();
        }
    }

    private sealed class SqliteProductDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        private SqliteProductDatabase(SqliteConnection connection, SqliteProductDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public SqliteProductDbContext Context { get; }

        public static SqliteProductDatabase Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SqliteProductDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new SqliteProductDbContext(options);
            context.Database.EnsureCreated();
            context.Products.AddRange(
                new SqliteProduct { Name = "Bike", Status = "active" },
                new SqliteProduct { Name = "Board", Status = "draft" },
                new SqliteProduct { Name = "Helmet", Status = "review" });
            context.SaveChanges();

            return new SqliteProductDatabase(connection, context);
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }

    private sealed class SqliteProductDbContext(DbContextOptions<SqliteProductDbContext> options) : DbContext(options)
    {
        public DbSet<SqliteProduct> Products => Set<SqliteProduct>();
    }

    private sealed class SqliteProduct
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
