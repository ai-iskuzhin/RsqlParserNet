using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RsqlParserNet.AspNetCore.Tests;

public sealed class RsqlQueryFilterTests
{
    [Fact]
    public void Parse_ReturnsEmptyFilterForMissingExpression()
    {
        var filter = RsqlQueryFilter.Parse(null);

        Assert.False(filter.IsSpecified);
        Assert.True(filter.IsValid);
        Assert.False(filter.HasQuery);
        Assert.Null(filter.Query);
        Assert.Empty(filter.Diagnostics);
    }

    [Fact]
    public void Parse_ReturnsParsedQueryForValidExpression()
    {
        var filter = RsqlQueryFilter.Parse("status==active");

        Assert.True(filter.IsSpecified);
        Assert.True(filter.IsValid);
        Assert.True(filter.HasQuery);
        Assert.NotNull(filter.Query);
        Assert.Equal("status==active", filter.Query.Expression);
    }

    [Fact]
    public void Parse_ReturnsDiagnosticsForInvalidExpression()
    {
        var filter = RsqlQueryFilter.Parse("status==");

        Assert.True(filter.IsSpecified);
        Assert.False(filter.IsValid);
        Assert.False(filter.HasQuery);
        Assert.Null(filter.Query);
        Assert.NotEmpty(filter.Diagnostics);
        Assert.NotEmpty(filter.ToValidationErrors());
    }

    [Fact]
    public async Task BindAsync_BindsDefaultFilterParameter()
    {
        var context = CreateContext("?filter=status%3D%3Dactive");
        var parameter = GetParameter(nameof(DefaultEndpoint));

        var filter = await RsqlQueryFilter.BindAsync(context, parameter);

        Assert.Equal(RsqlQueryFilter.DefaultQueryParameterName, filter.ParameterName);
        Assert.True(filter.HasQuery);
        Assert.Equal("status==active", filter.Expression);
    }

    [Fact]
    public async Task BindAsync_UsesConfiguredParameterName()
    {
        var services = new ServiceCollection();
        services.AddRsqlQueryFilter(options => options.QueryParameterName = "q");

        var context = CreateContext("?q=status%3D%3Dactive", services);
        var parameter = GetParameter(nameof(DefaultEndpoint));

        var filter = await RsqlQueryFilter.BindAsync(context, parameter);

        Assert.Equal("q", filter.ParameterName);
        Assert.True(filter.HasQuery);
    }

    [Fact]
    public async Task BindAsync_UsesConfiguredParseOptions()
    {
        var services = new ServiceCollection();
        services.AddRsqlQueryFilter(options =>
        {
            options.ParseOptions = RsqlParseOptions.Default with
            {
                CustomOperators = [new RsqlCustomOperator("=contains=")]
            };
        });

        var context = CreateContext("?filter=name%3Dcontains%3Dik", services);
        var parameter = GetParameter(nameof(DefaultEndpoint));

        var filter = await RsqlQueryFilter.BindAsync(context, parameter);

        Assert.True(filter.IsValid);
        Assert.True(filter.HasQuery);
    }

    [Fact]
    public async Task MinimalApi_BindsEndpointParameter()
    {
        RsqlQueryFilter? boundFilter = null;
        var requestDelegate = RequestDelegateFactory
            .Create((RsqlQueryFilter filter) =>
            {
                boundFilter = filter;
            })
            .RequestDelegate;
        var context = CreateContext("?filter=status%3D%3Dactive");

        await requestDelegate(context);

        Assert.NotNull(boundFilter);
        Assert.True(boundFilter.HasQuery);
        Assert.Equal("status==active", boundFilter.Expression);
    }

    [Fact]
    public async Task MinimalApi_UsesConfiguredParameterName()
    {
        RsqlQueryFilter? boundFilter = null;
        var requestDelegate = RequestDelegateFactory
            .Create((RsqlQueryFilter filter) =>
            {
                boundFilter = filter;
            })
            .RequestDelegate;
        var services = new ServiceCollection();
        services.AddRsqlQueryFilter(options => options.QueryParameterName = "q");
        var context = CreateContext("?q=status%3D%3Dactive", services);

        await requestDelegate(context);

        Assert.NotNull(boundFilter);
        Assert.Equal("q", boundFilter.ParameterName);
        Assert.True(boundFilter.HasQuery);
    }

    private static DefaultHttpContext CreateContext(string queryString, ServiceCollection? services = null)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = (services ?? new ServiceCollection()).BuildServiceProvider()
        };
        context.Request.QueryString = new QueryString(queryString);
        return context;
    }

    private static ParameterInfo GetParameter(string methodName)
    {
        return typeof(RsqlQueryFilterTests)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!
            .GetParameters()[0];
    }

    private static void DefaultEndpoint(RsqlQueryFilter filter)
    {
    }

}
