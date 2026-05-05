using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace RsqlParserNet.Linq.Tests;

public sealed class RsqlEfCoreTranslationTests
{
    [Fact]
    public void ApplyRsql_TranslatesScalarOperatorsWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("status==active;count>=10", new SqliteProductRsqlProfile())
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Bike", productName);
    }

    [Fact]
    public void ApplyRsql_TranslatesInOperatorWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("status=in=(active,draft)", new SqliteProductRsqlProfile())
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Board"], result);
    }

    [Fact]
    public void ApplyRsql_TranslatesWildcardOperatorWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("name==B*", new SqliteProductRsqlProfile())
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Board"], result);
    }

    [Fact]
    public void ApplyRsql_TranslatesStringContainsCustomOperatorWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("name=contains=ik", new SqliteProductRsqlProfile())
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Bike", productName);
    }

    [Fact]
    public void ApplyRsql_TranslatesCollectionAnyOperatorWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("tags=any=(outdoor)", new SqliteProductRsqlProfile())
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal(["Bike", "Helmet"], result);
    }

    [Fact]
    public void ApplyRsql_TranslatesCollectionAllOperatorWithSqlite()
    {
        using var database = SqliteProductDatabase.Create();

        var result = database.Context.Products
            .ApplyRsql("tags=all=(bike,outdoor)", new SqliteProductRsqlProfile())
            .Select(x => x.Name)
            .ToArray();

        var productName = Assert.Single(result);
        Assert.Equal("Bike", productName);
    }

    private sealed class SqliteProductRsqlProfile : RsqlLinqProfile<SqliteProduct>
    {
        public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
        {
            return options.WithLinqOperators();
        }

        public override void Configure(RsqlLinqOptions<SqliteProduct> options)
        {
            options.Allow("name", x => x.Name);
            options.Allow("status", x => x.Status);
            options.Allow("count", x => x.Count);
            options.Allow("tags", x => x.Tags);
            options.AllowStringContainsOperator();
            options.AllowCollectionAnyOperator();
            options.AllowCollectionAllOperator();
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
                new SqliteProduct
                {
                    Name = "Bike",
                    Status = "active",
                    Count = 10,
                    Tags = ["bike", "outdoor"]
                },
                new SqliteProduct
                {
                    Name = "Board",
                    Status = "draft",
                    Count = 20,
                    Tags = ["board"]
                },
                new SqliteProduct
                {
                    Name = "Helmet",
                    Status = "review",
                    Count = 30,
                    Tags = ["helmet", "outdoor"]
                });
            context.SaveChanges();

            return new SqliteProductDatabase(connection, context);
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }

    private sealed class SqliteProductDbContext(DbContextOptions<SqliteProductDbContext> options)
        : DbContext(options)
    {
        public DbSet<SqliteProduct> Products => Set<SqliteProduct>();
    }

    private sealed class SqliteProduct
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int Count { get; set; }

        public List<string> Tags { get; set; } = [];
    }
}
