using Microsoft.OpenApi;
using RsqlParserNet.AspNetCore;

namespace RsqlParserNet.OpenApi.Tests;

public sealed class RsqlOpenApiOperationDocumenterTests
{
    [Fact]
    public void Apply_AddsDefaultRsqlQueryParameters()
    {
        var operation = new OpenApiOperation();

        RsqlOpenApiOperationDocumenter.Apply(operation);
        var parameters = operation.Parameters ?? throw new InvalidOperationException("Parameters should be initialized.");

        Assert.Contains(parameters, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName && parameter.In == ParameterLocation.Query);
        Assert.Contains(parameters, parameter => parameter.Name == RsqlSortQuery.DefaultSortParameterName && parameter.In == ParameterLocation.Query);
        Assert.Contains(parameters, parameter => parameter.Name == RsqlPageQuery.DefaultPageParameterName && parameter.Schema?.Type == JsonSchemaType.Integer);
        Assert.Contains(parameters, parameter => parameter.Name == RsqlPageQuery.DefaultPageSizeParameterName && parameter.Schema?.Type == JsonSchemaType.Integer);
    }

    [Fact]
    public void Apply_UsesConfiguredParameterNames()
    {
        var operation = new OpenApiOperation();
        var options = new RsqlOpenApiQueryOptions
        {
            FilterParameterName = "q",
            SortParameterName = "orderBy",
            PageParameterName = "p",
            PageSizeParameterName = "take"
        };

        RsqlOpenApiOperationDocumenter.Apply(operation, options);
        var parameters = operation.Parameters ?? throw new InvalidOperationException("Parameters should be initialized.");

        Assert.Contains(parameters, parameter => parameter.Name == "q");
        Assert.Contains(parameters, parameter => parameter.Name == "orderBy");
        Assert.Contains(parameters, parameter => parameter.Name == "p");
        Assert.Contains(parameters, parameter => parameter.Name == "take");
    }

    [Fact]
    public void Apply_DoesNotDuplicateExistingQueryParameter()
    {
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = RsqlQueryFilter.DefaultQueryParameterName,
                    In = ParameterLocation.Query,
                    Description = "Existing filter."
                }
            ]
        };

        RsqlOpenApiOperationDocumenter.Apply(operation);

        Assert.Single(operation.Parameters, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
    }

    [Fact]
    public void Apply_CanDisableParameterGroups()
    {
        var operation = new OpenApiOperation();

        RsqlOpenApiOperationDocumenter.Apply(operation, new RsqlOpenApiQueryOptions
        {
            IncludeSort = false,
            IncludePagination = false
        });

        var parameters = operation.Parameters ?? throw new InvalidOperationException("Parameters should be initialized.");
        var parameter = Assert.Single(parameters);
        Assert.Equal(RsqlQueryFilter.DefaultQueryParameterName, parameter.Name);
    }
}
