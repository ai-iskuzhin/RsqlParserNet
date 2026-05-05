using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RsqlParserNet.Linq;

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
        var errors = filter.ToValidationErrors();

        Assert.True(filter.IsSpecified);
        Assert.False(filter.IsValid);
        Assert.False(filter.HasQuery);
        Assert.Null(filter.Query);
        Assert.NotEmpty(filter.Diagnostics);
        Assert.Contains(RsqlQueryFilter.DefaultQueryParameterName, errors.Keys);
        Assert.StartsWith("RSQL", errors[RsqlQueryFilter.DefaultQueryParameterName][0], StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_UsesParameterNameForValidationErrors()
    {
        var filter = RsqlQueryFilter.Parse("status==", parameterName: "q");
        var errors = filter.ToValidationErrors();

        Assert.False(filter.IsValid);
        Assert.Contains("q", errors.Keys);
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

    [Fact]
    public void PageQuery_ParseUsesDefaultsWhenValuesAreMissing()
    {
        var pageQuery = RsqlPageQuery.Parse(null, null);

        Assert.True(pageQuery.IsValid);
        Assert.NotNull(pageQuery.Request);
        Assert.Equal(1, pageQuery.Request.Page);
        Assert.Equal(50, pageQuery.Request.PageSize);
    }

    [Fact]
    public void PageQuery_ParseClampsPageSizeToMaximum()
    {
        var options = new RsqlPageQueryOptions { MaxPageSize = 25 };

        var pageQuery = RsqlPageQuery.Parse("2", "100", options);

        Assert.True(pageQuery.IsValid);
        Assert.NotNull(pageQuery.Request);
        Assert.Equal(2, pageQuery.Request.Page);
        Assert.Equal(25, pageQuery.Request.PageSize);
    }

    [Fact]
    public void PageQuery_ParseReturnsErrorsForInvalidValues()
    {
        var pageQuery = RsqlPageQuery.Parse("0", "abc");

        Assert.False(pageQuery.IsValid);
        Assert.Null(pageQuery.Request);
        Assert.Contains(RsqlPageQuery.DefaultPageParameterName, pageQuery.Errors.Keys);
        Assert.Contains(RsqlPageQuery.DefaultPageSizeParameterName, pageQuery.Errors.Keys);
        Assert.NotEmpty(pageQuery.ToValidationErrors());
    }

    [Fact]
    public async Task PageQuery_BindAsyncUsesConfiguredParameterNames()
    {
        var services = new ServiceCollection();
        services.AddRsqlPageQuery(options =>
        {
            options.PageParameterName = "p";
            options.PageSizeParameterName = "take";
            options.MaxPageSize = 30;
        });
        var context = CreateContext("?p=3&take=100", services);
        var parameter = GetParameter(nameof(PageEndpoint));

        var pageQuery = await RsqlPageQuery.BindAsync(context, parameter);

        Assert.True(pageQuery.IsValid);
        Assert.NotNull(pageQuery.Request);
        Assert.Equal(3, pageQuery.Request.Page);
        Assert.Equal(30, pageQuery.Request.PageSize);
    }

    [Fact]
    public async Task MinimalApi_BindsPageQueryParameter()
    {
        RsqlPageQuery? boundPage = null;
        var requestDelegate = RequestDelegateFactory
            .Create((RsqlPageQuery page) =>
            {
                boundPage = page;
            })
            .RequestDelegate;
        var context = CreateContext("?page=2&pageSize=10");

        await requestDelegate(context);

        Assert.NotNull(boundPage);
        Assert.True(boundPage.IsValid);
        Assert.NotNull(boundPage.Request);
        Assert.Equal(2, boundPage.Request.Page);
        Assert.Equal(10, boundPage.Request.PageSize);
    }

    [Fact]
    public void SortQuery_ParseReturnsAscendingSort()
    {
        var sortQuery = RsqlSortQuery.Parse("name");

        Assert.True(sortQuery.IsValid);
        Assert.True(sortQuery.HasRequest);
        Assert.NotNull(sortQuery.Request);
        Assert.Equal("name", sortQuery.Request.Field);
        Assert.Equal(RsqlSortDirection.Ascending, sortQuery.Request.Direction);
    }

    [Fact]
    public void SortQuery_ParseReturnsDescendingSort()
    {
        var sortQuery = RsqlSortQuery.Parse("-createdAt");

        Assert.True(sortQuery.IsValid);
        Assert.True(sortQuery.HasRequest);
        Assert.NotNull(sortQuery.Request);
        Assert.Equal("createdAt", sortQuery.Request.Field);
        Assert.Equal(RsqlSortDirection.Descending, sortQuery.Request.Direction);
    }

    [Fact]
    public void SortQuery_ParseReturnsMultipleSorts()
    {
        var sortQuery = RsqlSortQuery.Parse("status,-name");

        Assert.True(sortQuery.IsValid);
        Assert.True(sortQuery.HasRequest);
        Assert.Equal(2, sortQuery.Requests.Count);
        Assert.NotNull(sortQuery.Request);
        Assert.Equal("status", sortQuery.Request.Field);
        Assert.Equal("name", sortQuery.Requests[1].Field);
        Assert.Equal(RsqlSortDirection.Descending, sortQuery.Requests[1].Direction);
    }

    [Fact]
    public void SortQuery_ParseReturnsErrorsForMissingField()
    {
        var sortQuery = RsqlSortQuery.Parse("-");

        Assert.False(sortQuery.IsValid);
        Assert.False(sortQuery.HasRequest);
        Assert.Contains(RsqlSortQuery.DefaultSortParameterName, sortQuery.Errors.Keys);
        Assert.NotEmpty(sortQuery.ToValidationErrors());
    }

    [Fact]
    public void SortQuery_ParseReturnsErrorsForInvalidFieldSyntax()
    {
        var sortQuery = RsqlSortQuery.Parse("9name");

        Assert.False(sortQuery.IsValid);
        Assert.False(sortQuery.HasRequest);
        Assert.Contains(RsqlSortQuery.DefaultSortParameterName, sortQuery.Errors.Keys);
    }

    [Fact]
    public async Task SortQuery_BindAsyncUsesConfiguredParameterName()
    {
        var services = new ServiceCollection();
        services.AddRsqlSortQuery(options => options.SortParameterName = "orderBy");
        var context = CreateContext("?orderBy=-name", services);
        var parameter = GetParameter(nameof(SortEndpoint));

        var sortQuery = await RsqlSortQuery.BindAsync(context, parameter);

        Assert.True(sortQuery.IsValid);
        Assert.NotNull(sortQuery.Request);
        Assert.Equal("orderBy", sortQuery.ParameterName);
        Assert.Equal("name", sortQuery.Request.Field);
        Assert.Equal(RsqlSortDirection.Descending, sortQuery.Request.Direction);
    }

    [Fact]
    public async Task MinimalApi_BindsSortQueryParameter()
    {
        RsqlSortQuery? boundSort = null;
        var requestDelegate = RequestDelegateFactory
            .Create((RsqlSortQuery sort) =>
            {
                boundSort = sort;
            })
            .RequestDelegate;
        var context = CreateContext("?sort=-name");

        await requestDelegate(context);

        Assert.NotNull(boundSort);
        Assert.True(boundSort.IsValid);
        Assert.NotNull(boundSort.Request);
        Assert.Equal("name", boundSort.Request.Field);
        Assert.Equal(RsqlSortDirection.Descending, boundSort.Request.Direction);
    }

    [Fact]
    public async Task QueryRequest_BindAsyncBindsFilterSortAndPage()
    {
        var services = new ServiceCollection();
        services.AddRsqlQueryRequest(
            configureFilter: options => options.ParseOptions = RsqlParseOptions.Default,
            configurePage: options => options.MaxPageSize = 25);
        var context = CreateContext("?filter=status%3D%3Dactive&sort=-name,status&page=2&pageSize=100", services);
        var parameter = GetParameter(nameof(QueryRequestEndpoint));

        var request = await RsqlQueryRequest.BindAsync(context, parameter);

        Assert.True(request.IsValid);
        Assert.True(request.Filter.HasQuery);
        Assert.NotNull(request.Sort.Request);
        Assert.NotNull(request.Page.Request);
        Assert.Equal("name", request.Sort.Request.Field);
        Assert.Equal(RsqlSortDirection.Descending, request.Sort.Request.Direction);
        Assert.Equal(2, request.Sort.Requests.Count);
        Assert.Equal(2, request.PageRequest.Page);
        Assert.Equal(25, request.PageRequest.PageSize);
    }

    [Fact]
    public async Task QueryRequest_MergesValidationErrors()
    {
        var context = CreateContext("?filter=status%3D%3D&sort=-&page=0&pageSize=bad");
        var parameter = GetParameter(nameof(QueryRequestEndpoint));

        var request = await RsqlQueryRequest.BindAsync(context, parameter);
        var errors = request.ToValidationErrors();

        Assert.False(request.IsValid);
        Assert.NotEmpty(errors);
        Assert.Contains(RsqlQueryFilter.DefaultQueryParameterName, errors.Keys);
        Assert.Contains(RsqlSortQuery.DefaultSortParameterName, errors.Keys);
        Assert.Contains(RsqlPageQuery.DefaultPageParameterName, errors.Keys);
        Assert.Contains(RsqlPageQuery.DefaultPageSizeParameterName, errors.Keys);
    }

    [Fact]
    public async Task QueryRequest_GetErrorsReturnsStructuredErrors()
    {
        var context = CreateContext("?filter=status%3D%3D&sort=-&page=0&pageSize=bad");
        var parameter = GetParameter(nameof(QueryRequestEndpoint));

        var request = await RsqlQueryRequest.BindAsync(context, parameter);
        var errors = request.GetErrors();

        Assert.False(request.IsValid);
        Assert.Contains(errors, error =>
            error.Source == RsqlQueryErrorSource.Filter
            && error.ParameterName == RsqlQueryFilter.DefaultQueryParameterName
            && error.Code is not null
            && error.Start is not null);
        Assert.Contains(errors, error =>
            error.Source == RsqlQueryErrorSource.Sort
            && error.ParameterName == RsqlSortQuery.DefaultSortParameterName
            && error.Code is null);
        Assert.Contains(errors, error =>
            error.Source == RsqlQueryErrorSource.Page
            && error.ParameterName == RsqlPageQuery.DefaultPageParameterName);
    }

    [Fact]
    public async Task QueryRequest_ToValidationProblemDetailsIncludesStructuredErrorsExtension()
    {
        var context = CreateContext("?filter=status%3D%3D");
        var parameter = GetParameter(nameof(QueryRequestEndpoint));

        var request = await RsqlQueryRequest.BindAsync(context, parameter);
        var problemDetails = request.ToValidationProblemDetails();

        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Contains(RsqlQueryFilter.DefaultQueryParameterName, problemDetails.Errors.Keys);
        var errors = Assert.IsAssignableFrom<IReadOnlyList<RsqlQueryError>>(problemDetails.Extensions["rsqlErrors"]);
        var error = Assert.Single(errors);
        Assert.Equal(RsqlQueryErrorSource.Filter, error.Source);
        Assert.NotNull(error.Code);
    }

    [Fact]
    public async Task QueryRequest_PageRequestThrowsWhenPageQueryIsInvalid()
    {
        var context = CreateContext("?page=0");
        var parameter = GetParameter(nameof(QueryRequestEndpoint));

        var request = await RsqlQueryRequest.BindAsync(context, parameter);

        Assert.False(request.IsValid);
        Assert.Throws<InvalidOperationException>(() => request.PageRequest);
    }

    [Fact]
    public void QueryRequest_ApplyToAppliesFilterAndSort()
    {
        var filter = RsqlQueryFilter.Parse("status==active");
        var sort = RsqlSortQuery.Parse("status,-name");
        var page = RsqlPageQuery.Parse(null, null);
        var request = new RsqlQueryRequest(filter, sort, page);

        var result = request
            .ApplyTo(SampleProducts(), new ProductRsqlProfile())
            .Select(product => product.Name)
            .ToArray();

        Assert.Equal(["Helmet", "Bike"], result);
    }

    [Fact]
    public async Task MinimalApi_BindsQueryRequestParameter()
    {
        RsqlQueryRequest? boundRequest = null;
        var requestDelegate = RequestDelegateFactory
            .Create((RsqlQueryRequest query) =>
            {
                boundRequest = query;
            })
            .RequestDelegate;
        var context = CreateContext("?filter=status%3D%3Dactive&sort=name&page=1&pageSize=10");

        await requestDelegate(context);

        Assert.NotNull(boundRequest);
        Assert.True(boundRequest.IsValid);
        Assert.True(boundRequest.Filter.HasQuery);
        Assert.NotNull(boundRequest.Sort.Request);
        Assert.NotNull(boundRequest.Page.Request);
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

    private static void PageEndpoint(RsqlPageQuery page)
    {
    }

    private static void SortEndpoint(RsqlSortQuery sort)
    {
    }

    private static void QueryRequestEndpoint(RsqlQueryRequest query)
    {
    }

    private static IQueryable<Product> SampleProducts()
    {
        return new[]
        {
            new Product("Bike", "active"),
            new Product("Board", "draft"),
            new Product("Helmet", "active")
        }.AsQueryable();
    }

    private sealed class ProductRsqlProfile : RsqlLinqProfile<Product>
    {
        public override void Configure(RsqlLinqOptions<Product> options)
        {
            options.Allow("name", product => product.Name);
            options.Allow("status", product => product.Status);
        }
    }

    private sealed record Product(string Name, string Status);
}
