using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.Linq;

namespace RsqlParserNet.FastEndpoints.Tests;

public sealed class RsqlFastEndpointExtensionsTests
{
    [Fact]
    public void BindRsqlQueryRequest_BindsFilterSortAndPageFromHttpContext()
    {
        var services = new ServiceCollection();
        services.AddRsqlQueryRequest(
            configureFilter: options => options.ParseOptions = RsqlParseOptions.Default,
            configureSort: options => options.SortParameterName = "orderBy",
            configurePage: options => options.MaxPageSize = 25);
        var endpoint = new TestEndpoint(CreateContext("?filter=status%3D%3Dactive&orderBy=-name&page=2&pageSize=100", services));

        var request = endpoint.BindRsqlQueryRequest();

        Assert.True(request.IsValid);
        Assert.True(request.Filter.HasQuery);
        Assert.NotNull(request.Sort.Request);
        Assert.Equal("name", request.Sort.Request.Field);
        Assert.Equal(RsqlSortDirection.Descending, request.Sort.Request.Direction);
        Assert.Equal(2, request.PageRequest.Page);
        Assert.Equal(25, request.PageRequest.PageSize);
    }

    [Fact]
    public void AddRsqlValidationFailures_AddsFilterSortAndPageFailures()
    {
        var endpoint = new TestEndpoint(CreateContext("?filter=status%3D%3D&sort=-&page=0&pageSize=bad"));
        var request = endpoint.BindRsqlQueryRequest();

        endpoint.AddRsqlValidationFailures(request);

        Assert.False(request.IsValid);
        Assert.Contains(endpoint.ValidationFailures, failure => failure.PropertyName == RsqlQueryFilter.DefaultQueryParameterName && failure.ErrorCode.StartsWith("RSQL", StringComparison.Ordinal));
        Assert.Contains(endpoint.ValidationFailures, failure => failure.PropertyName == RsqlSortQuery.DefaultSortParameterName);
        Assert.Contains(endpoint.ValidationFailures, failure => failure.PropertyName == RsqlPageQuery.DefaultPageParameterName);
        Assert.Contains(endpoint.ValidationFailures, failure => failure.PropertyName == RsqlPageQuery.DefaultPageSizeParameterName);
    }

    [Fact]
    public void BindRsqlQueryRequestAndAddErrors_ReturnsRequestAndAddsFailures()
    {
        var endpoint = new TestEndpoint(CreateContext("?page=0"));

        var request = endpoint.BindRsqlQueryRequestAndAddErrors();

        Assert.False(request.IsValid);
        var failure = Assert.Single(endpoint.ValidationFailures);
        Assert.Equal(RsqlPageQuery.DefaultPageParameterName, failure.PropertyName);
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

    private sealed class TestEndpoint : IEndpoint
    {
        public TestEndpoint(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public EndpointDefinition Definition { get; } = null!;

        public HttpContext HttpContext { get; }

        public List<ValidationFailure> ValidationFailures { get; } = [];
    }
}
