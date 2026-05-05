using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using System.Reflection;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RsqlParserNet.Swashbuckle.Tests;

public sealed class RsqlSwaggerQueryParametersOperationFilterTests
{
    [Fact]
    public void Apply_DocumentsMarkedEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext(new RsqlSwaggerQueryMetadata(new RsqlOpenApiQueryOptions()));
        var filter = new RsqlSwaggerQueryParametersOperationFilter();

        filter.Apply(operation, context);

        Assert.Contains(operation.Parameters!, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
        Assert.Contains(operation.Parameters!, parameter => parameter.Name == RsqlSortQuery.DefaultSortParameterName);
        Assert.Contains(operation.Parameters!, parameter => parameter.Name == RsqlPageQuery.DefaultPageParameterName);
        Assert.Contains(operation.Parameters!, parameter => parameter.Name == RsqlPageQuery.DefaultPageSizeParameterName);
    }

    [Fact]
    public void Apply_SkipsUnmarkedEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext();
        var filter = new RsqlSwaggerQueryParametersOperationFilter();

        filter.Apply(operation, context);

        Assert.Null(operation.Parameters);
    }

    [Fact]
    public void Apply_UsesMarkedEndpointOptions()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext(new RsqlSwaggerQueryMetadata(new RsqlOpenApiQueryOptions
        {
            FilterParameterName = "q",
            IncludeSort = false,
            IncludePagination = false
        }));
        var filter = new RsqlSwaggerQueryParametersOperationFilter();

        filter.Apply(operation, context);

        var parameter = Assert.Single(operation.Parameters!);
        Assert.Equal("q", parameter.Name);
    }

    [Fact]
    public void ApplyAll_DocumentsEveryEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext();
        var filter = new RsqlSwaggerAllQueryParametersOperationFilter(new RsqlOpenApiQueryOptions
        {
            SortParameterName = "orderBy"
        });

        filter.Apply(operation, context);

        Assert.Contains(operation.Parameters!, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
        Assert.Contains(operation.Parameters!, parameter => parameter.Name == "orderBy");
    }

    private static OperationFilterContext CreateContext(params object[] endpointMetadata)
    {
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata
            }
        };

        return new OperationFilterContext(
            apiDescription,
            null!,
            null!,
            new OpenApiDocument(),
            typeof(RsqlSwaggerQueryParametersOperationFilterTests).GetMethod(nameof(DummyEndpoint), BindingFlags.NonPublic | BindingFlags.Static)!);
    }

    private static void DummyEndpoint()
    {
    }
}
