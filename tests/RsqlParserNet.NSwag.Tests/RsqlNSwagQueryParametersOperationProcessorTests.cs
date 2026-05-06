using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Contexts;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.NSwag;
using RsqlParserNet.OpenApi;

namespace RsqlParserNet.NSwag.Tests;

public sealed class RsqlNSwagQueryParametersOperationProcessorTests
{
    [Fact]
    public void Apply_DocumentsMarkedEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateAspNetCoreContext(operation, new RsqlNSwagQueryMetadata(new RsqlOpenApiQueryOptions()));
        var processor = new RsqlNSwagQueryParametersOperationProcessor();

        var keepOperation = processor.Process(context);

        Assert.True(keepOperation);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == RsqlSortQuery.DefaultSortParameterName);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == RsqlPageQuery.DefaultPageParameterName);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == RsqlPageQuery.DefaultPageSizeParameterName);
    }

    [Fact]
    public void Apply_SkipsUnmarkedEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateAspNetCoreContext(operation);
        var processor = new RsqlNSwagQueryParametersOperationProcessor();

        var keepOperation = processor.Process(context);

        Assert.True(keepOperation);
        Assert.Empty(operation.Parameters);
    }

    [Fact]
    public void Apply_UsesMarkedEndpointOptions()
    {
        var operation = new OpenApiOperation();
        var context = CreateAspNetCoreContext(operation, new RsqlNSwagQueryMetadata(new RsqlOpenApiQueryOptions
        {
            FilterParameterName = "q",
            IncludeSort = false,
            IncludePagination = false
        }));
        var processor = new RsqlNSwagQueryParametersOperationProcessor();

        processor.Process(context);

        var parameter = Assert.Single(operation.Parameters);
        Assert.Equal("q", parameter.Name);
    }

    [Fact]
    public void ApplyAll_DocumentsEveryEndpoint()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationContext(operation);
        var processor = new RsqlNSwagAllQueryParametersOperationProcessor(new RsqlOpenApiQueryOptions
        {
            SortParameterName = "orderBy"
        });

        var keepOperation = processor.Process(context);

        Assert.True(keepOperation);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == "orderBy");
    }

    [Fact]
    public void Apply_DoesNotDuplicateExistingQueryParameter()
    {
        var operation = new OpenApiOperation();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = RsqlQueryFilter.DefaultQueryParameterName,
            Kind = OpenApiParameterKind.Query
        });

        RsqlNSwagOperationDocumenter.Apply(operation);

        Assert.Single(operation.Parameters, parameter => parameter.Name == RsqlQueryFilter.DefaultQueryParameterName);
    }

    private static AspNetCoreOperationProcessorContext CreateAspNetCoreContext(
        OpenApiOperation operation,
        params object[] endpointMetadata)
    {
        var context = new AspNetCoreOperationProcessorContext(
            new OpenApiDocument(),
            CreateOperationDescription(operation),
            controllerType: null!,
            methodInfo: typeof(RsqlNSwagQueryParametersOperationProcessorTests).GetMethod(nameof(DummyEndpoint), BindingFlags.NonPublic | BindingFlags.Static)!,
            documentGenerator: null!,
            schemaResolver: null!,
            settings: new OpenApiDocumentGeneratorSettings(),
            allOperationDescriptions: []);
        context.ApiDescription = new ApiDescription
        {
            ActionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata
            }
        };

        return context;
    }

    private static OperationProcessorContext CreateOperationContext(OpenApiOperation operation)
    {
        return new OperationProcessorContext(
            new OpenApiDocument(),
            CreateOperationDescription(operation),
            controllerType: null!,
            methodInfo: typeof(RsqlNSwagQueryParametersOperationProcessorTests).GetMethod(nameof(DummyEndpoint), BindingFlags.NonPublic | BindingFlags.Static)!,
            documentGenerator: null!,
            schemaResolver: null!,
            settings: new OpenApiDocumentGeneratorSettings(),
            allOperationDescriptions: []);
    }

    private static OpenApiOperationDescription CreateOperationDescription(OpenApiOperation operation)
    {
        return new OpenApiOperationDescription
        {
            Path = "/products",
            Method = OpenApiOperationMethod.Get,
            Operation = operation
        };
    }

    private static void DummyEndpoint()
    {
    }
}
