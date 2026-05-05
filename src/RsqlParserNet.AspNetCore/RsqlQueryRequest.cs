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

        return ValueTask.FromResult(new RsqlQueryRequest(
            RsqlQueryFilter.Parse(filterExpression, filterOptions.ParseOptions, filterOptions.QueryParameterName),
            RsqlSortQuery.Parse(sortExpression, sortOptions.SortParameterName),
            RsqlPageQuery.Parse(page, pageSize, pageOptions)));
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
    /// Converts all binding errors into the dictionary shape expected by ASP.NET Core validation problem results.
    /// </summary>
    /// <returns>A dictionary keyed by validation field.</returns>
    public Dictionary<string, string[]> ToValidationErrors()
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        AddErrors(errors, Filter.ToValidationErrors());
        AddErrors(errors, Sort.ToValidationErrors());
        AddErrors(errors, Page.ToValidationErrors());

        return errors.ToDictionary(
            item => item.Key,
            item => item.Value.ToArray(),
            StringComparer.Ordinal);
    }

    private static string? ReadQueryValue(HttpContext context, string parameterName)
    {
        return context.Request.Query.TryGetValue(parameterName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static void AddErrors(
        Dictionary<string, List<string>> target,
        IReadOnlyDictionary<string, string[]> source)
    {
        foreach (var (key, messages) in source)
        {
            if (!target.TryGetValue(key, out var existingMessages))
            {
                existingMessages = [];
                target[key] = existingMessages;
            }

            existingMessages.AddRange(messages);
        }
    }
}
