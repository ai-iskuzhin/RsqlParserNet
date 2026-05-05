using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RsqlParserNet.AspNetCore;

namespace RsqlParserNet.FastEndpoints;

/// <summary>
/// Provides FastEndpoints helpers for RSQL query request binding and validation.
/// </summary>
public static class RsqlFastEndpointExtensions
{
    /// <summary>
    /// Binds RSQL filter, sort, and page query state from the endpoint HTTP context.
    /// </summary>
    /// <param name="endpoint">The current FastEndpoints endpoint.</param>
    /// <returns>The bound RSQL query request.</returns>
    public static RsqlQueryRequest BindRsqlQueryRequest(this IEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        var context = endpoint.HttpContext;
        var filterOptions = context.RequestServices.GetService<IOptions<RsqlQueryFilterOptions>>()?.Value
            ?? new RsqlQueryFilterOptions();
        var sortOptions = context.RequestServices.GetService<IOptions<RsqlSortQueryOptions>>()?.Value
            ?? new RsqlSortQueryOptions();
        var pageOptions = context.RequestServices.GetService<IOptions<RsqlPageQueryOptions>>()?.Value
            ?? new RsqlPageQueryOptions();

        return RsqlQueryRequest.Parse(
            ReadQueryValue(context, filterOptions.QueryParameterName),
            ReadQueryValue(context, sortOptions.SortParameterName),
            ReadQueryValue(context, pageOptions.PageParameterName),
            ReadQueryValue(context, pageOptions.PageSizeParameterName),
            filterOptions,
            sortOptions,
            pageOptions);
    }

    /// <summary>
    /// Binds RSQL query state and adds any RSQL validation failures to the endpoint.
    /// </summary>
    /// <remarks>
    /// Call FastEndpoints <c>ThrowIfAnyErrors()</c> after this method when the endpoint should stop on invalid query input.
    /// </remarks>
    /// <param name="endpoint">The current FastEndpoints endpoint.</param>
    /// <returns>The bound RSQL query request.</returns>
    public static RsqlQueryRequest BindRsqlQueryRequestAndAddErrors(this IEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        var request = endpoint.BindRsqlQueryRequest();
        endpoint.AddRsqlValidationFailures(request);

        return request;
    }

    /// <summary>
    /// Adds RSQL validation failures to the endpoint.
    /// </summary>
    /// <param name="endpoint">The current FastEndpoints endpoint.</param>
    /// <param name="request">The bound RSQL query request.</param>
    public static void AddRsqlValidationFailures(this IEndpoint endpoint, RsqlQueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(request);

        foreach (var failure in request.ToFastEndpointValidationFailures())
        {
            endpoint.ValidationFailures.Add(failure);
        }
    }

    /// <summary>
    /// Converts RSQL binding errors into FastEndpoints validation failures.
    /// </summary>
    /// <param name="request">The bound RSQL query request.</param>
    /// <returns>The validation failures.</returns>
    public static IReadOnlyList<ValidationFailure> ToFastEndpointValidationFailures(this RsqlQueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var failures = new List<ValidationFailure>();
        foreach (var diagnostic in request.Filter.Diagnostics)
        {
            failures.Add(new ValidationFailure(request.Filter.ParameterName, diagnostic.Message)
            {
                ErrorCode = diagnostic.Code
            });
        }

        AddFailures(failures, request.Sort.Errors);
        AddFailures(failures, request.Page.Errors);

        return failures;
    }

    private static string? ReadQueryValue(HttpContext context, string parameterName)
    {
        return context.Request.Query.TryGetValue(parameterName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static void AddFailures(
        List<ValidationFailure> failures,
        IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (propertyName, messages) in errors)
        {
            foreach (var message in messages)
            {
                failures.Add(new ValidationFailure(propertyName, message));
            }
        }
    }
}
