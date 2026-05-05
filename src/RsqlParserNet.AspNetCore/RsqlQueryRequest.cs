using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RsqlParserNet.Linq;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Represents filter, sort, and page query string state bound from an ASP.NET Core request.
/// </summary>
public sealed class RsqlQueryRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlQueryRequest"/> class.
    /// </summary>
    /// <param name="filter">The bound RSQL filter query.</param>
    /// <param name="sort">The bound sort query.</param>
    /// <param name="page">The bound page query.</param>
    public RsqlQueryRequest(RsqlQueryFilter filter, RsqlSortQuery sort, RsqlPageQuery page)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(page);

        Filter = filter;
        Sort = sort;
        Page = page;
    }

    /// <summary>
    /// Gets the bound RSQL filter query.
    /// </summary>
    public RsqlQueryFilter Filter { get; }

    /// <summary>
    /// Gets the bound sort query.
    /// </summary>
    public RsqlSortQuery Sort { get; }

    /// <summary>
    /// Gets the bound page query.
    /// </summary>
    public RsqlPageQuery Page { get; }

    /// <summary>
    /// Gets the parsed page request after the query request has been validated.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the page query is invalid.</exception>
    public RsqlPageRequest PageRequest
    {
        get
        {
            if (!Page.IsValid || Page.Request is null)
            {
                throw new InvalidOperationException("Cannot access the page request from an invalid RSQL query request.");
            }

            return Page.Request;
        }
    }

    /// <summary>
    /// Gets a value indicating whether all bound query components are valid.
    /// </summary>
    public bool IsValid => Filter.IsValid && Sort.IsValid && Page.IsValid;

    /// <summary>
    /// Parses filter, sort, and page query string values into a combined query request.
    /// </summary>
    /// <param name="filterExpression">The raw RSQL filter expression text.</param>
    /// <param name="sortExpression">The raw sort expression text.</param>
    /// <param name="page">The raw one-based page number text.</param>
    /// <param name="pageSize">The raw page size text.</param>
    /// <param name="filterOptions">Optional filter binding options.</param>
    /// <param name="sortOptions">Optional sort binding options.</param>
    /// <param name="pageOptions">Optional page binding options.</param>
    /// <returns>The parsed query request.</returns>
    public static RsqlQueryRequest Parse(
        string? filterExpression,
        string? sortExpression,
        string? page,
        string? pageSize,
        RsqlQueryFilterOptions? filterOptions = null,
        RsqlSortQueryOptions? sortOptions = null,
        RsqlPageQueryOptions? pageOptions = null)
    {
        var effectiveFilterOptions = filterOptions ?? new RsqlQueryFilterOptions();
        var effectiveSortOptions = sortOptions ?? new RsqlSortQueryOptions();
        var effectivePageOptions = pageOptions ?? new RsqlPageQueryOptions();

        return new RsqlQueryRequest(
            RsqlQueryFilter.Parse(
                filterExpression,
                effectiveFilterOptions.ParseOptions,
                effectiveFilterOptions.QueryParameterName),
            RsqlSortQuery.Parse(sortExpression, effectiveSortOptions.SortParameterName),
            RsqlPageQuery.Parse(page, pageSize, effectivePageOptions));
    }

    /// <summary>
    /// Binds filter, sort, and page query string state from the current ASP.NET Core request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="parameter">The endpoint parameter being bound.</param>
    /// <returns>The bound query request.</returns>
    public static ValueTask<RsqlQueryRequest> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        var filterOptions = context.RequestServices.GetService<IOptions<RsqlQueryFilterOptions>>()?.Value
            ?? new RsqlQueryFilterOptions();
        var sortOptions = context.RequestServices.GetService<IOptions<RsqlSortQueryOptions>>()?.Value
            ?? new RsqlSortQueryOptions();
        var pageOptions = context.RequestServices.GetService<IOptions<RsqlPageQueryOptions>>()?.Value
            ?? new RsqlPageQueryOptions();

        var filterExpression = ReadQueryValue(context, filterOptions.QueryParameterName);
        var sortExpression = ReadQueryValue(context, sortOptions.SortParameterName);
        var page = ReadQueryValue(context, pageOptions.PageParameterName);
        var pageSize = ReadQueryValue(context, pageOptions.PageSizeParameterName);

        return ValueTask.FromResult(Parse(
            filterExpression,
            sortExpression,
            page,
            pageSize,
            filterOptions,
            sortOptions,
            pageOptions));
    }

    /// <summary>
    /// Applies valid filter and sort query components to a queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <returns>The filtered and sorted queryable.</returns>
    public IQueryable<T> ApplyTo<T>(IQueryable<T> source, RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(profile);

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot apply an invalid RSQL query request.");
        }

        var query = Filter.HasQuery
            ? source.ApplyRsql(Filter.Query!, profile)
            : source;

        return Sort.HasRequest
            ? query.ApplySort(Sort.Requests, profile)
            : query;
    }

    /// <summary>
    /// Attempts to apply valid filter and sort query components to a queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="query">The filtered and sorted queryable when translation succeeds; otherwise the original source.</param>
    /// <param name="errors">Structured binding or translation errors.</param>
    /// <returns><see langword="true"/> when the request was valid and translated successfully; otherwise <see langword="false"/>.</returns>
    public bool TryApplyTo<T>(
        IQueryable<T> source,
        RsqlLinqProfile<T> profile,
        out IQueryable<T> query,
        out IReadOnlyList<RsqlQueryError> errors)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(profile);

        query = source;
        var existingErrors = GetErrors();
        if (existingErrors.Count > 0)
        {
            errors = existingErrors;
            return false;
        }

        try
        {
            query = ApplyTo(source, profile);
            errors = [];
            return true;
        }
        catch (RsqlLinqException exception)
        {
            errors =
            [
                new RsqlQueryError(
                    Filter.ParameterName,
                    exception.Message,
                    RsqlQueryErrorSource.Filter,
                    RsqlQueryErrorCodes.AdapterTranslationError)
            ];
            return false;
        }
    }

    /// <summary>
    /// Gets structured query binding errors for custom API error handlers.
    /// </summary>
    /// <returns>The binding errors in filter, sort, and page order.</returns>
    public IReadOnlyList<RsqlQueryError> GetErrors()
    {
        var errors = new List<RsqlQueryError>();

        foreach (var diagnostic in Filter.Diagnostics)
        {
            errors.Add(new RsqlQueryError(
                Filter.ParameterName,
                diagnostic.Message,
                RsqlQueryErrorSource.Filter,
                diagnostic.Code,
                diagnostic.Span,
                diagnostic.Start,
                diagnostic.End));
        }

        AddErrors(errors, Sort.Errors, RsqlQueryErrorSource.Sort);
        AddErrors(errors, Page.Errors, RsqlQueryErrorSource.Page);

        return errors;
    }

    /// <summary>
    /// Converts all binding errors into the dictionary shape expected by ASP.NET Core validation problem results.
    /// </summary>
    /// <returns>A dictionary keyed by validation field.</returns>
    public Dictionary<string, string[]> ToValidationErrors()
    {
        return ToValidationErrors(GetErrors());
    }

    /// <summary>
    /// Converts binding or translation errors into the dictionary shape expected by ASP.NET Core validation problem results.
    /// </summary>
    /// <param name="queryErrors">The query errors to convert.</param>
    /// <returns>A dictionary keyed by validation field.</returns>
    public Dictionary<string, string[]> ToValidationErrors(IReadOnlyList<RsqlQueryError> queryErrors)
    {
        ArgumentNullException.ThrowIfNull(queryErrors);

        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var error in queryErrors)
        {
            if (!errors.TryGetValue(error.ParameterName, out var messages))
            {
                messages = [];
                errors[error.ParameterName] = messages;
            }

            messages.Add(FormatValidationMessage(error));
        }

        return errors.ToDictionary(
            item => item.Key,
            item => item.Value.ToArray(),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// Converts all binding errors into ASP.NET Core validation problem details.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to assign to the problem details.</param>
    /// <param name="title">The problem details title.</param>
    /// <returns>The validation problem details.</returns>
    public HttpValidationProblemDetails ToValidationProblemDetails(
        int statusCode = StatusCodes.Status400BadRequest,
        string title = "One or more RSQL query parameters are invalid.")
    {
        return ToValidationProblemDetails(GetErrors(), statusCode, title);
    }

    /// <summary>
    /// Converts binding or translation errors into ASP.NET Core validation problem details.
    /// </summary>
    /// <param name="queryErrors">The query errors to convert.</param>
    /// <param name="statusCode">The HTTP status code to assign to the problem details.</param>
    /// <param name="title">The problem details title.</param>
    /// <returns>The validation problem details.</returns>
    public HttpValidationProblemDetails ToValidationProblemDetails(
        IReadOnlyList<RsqlQueryError> queryErrors,
        int statusCode = StatusCodes.Status400BadRequest,
        string title = "One or more RSQL query parameters are invalid.")
    {
        ArgumentNullException.ThrowIfNull(queryErrors);

        var problemDetails = new HttpValidationProblemDetails(ToValidationErrors(queryErrors))
        {
            Status = statusCode,
            Title = title
        };
        problemDetails.Extensions["rsqlErrors"] = queryErrors;

        return problemDetails;
    }

    private static string? ReadQueryValue(HttpContext context, string parameterName)
    {
        return context.Request.Query.TryGetValue(parameterName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static void AddErrors(
        List<RsqlQueryError> target,
        IReadOnlyDictionary<string, string[]> source,
        RsqlQueryErrorSource errorSource)
    {
        foreach (var (key, messages) in source)
        {
            foreach (var message in messages)
            {
                target.Add(new RsqlQueryError(key, message, errorSource));
            }
        }
    }

    private static string FormatValidationMessage(RsqlQueryError error)
    {
        return string.IsNullOrWhiteSpace(error.Code)
            ? error.Message
            : $"{error.Code}: {error.Message}";
    }
}
