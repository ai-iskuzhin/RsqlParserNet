using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RsqlParserNet.Linq;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Represents a sort request bound from an ASP.NET Core query string parameter.
/// </summary>
public sealed class RsqlSortQuery
{
    /// <summary>
    /// The default query string parameter name used for sorting.
    /// </summary>
    public const string DefaultSortParameterName = "sort";

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSortQuery"/> class.
    /// </summary>
    /// <param name="parameterName">The query string parameter name that supplied the sort.</param>
    /// <param name="expression">The raw sort expression text.</param>
    /// <param name="request">The parsed sort request when binding succeeded.</param>
    /// <param name="errors">The validation errors produced while binding.</param>
    public RsqlSortQuery(
        string parameterName,
        string? expression,
        RsqlSortRequest? request,
        IReadOnlyDictionary<string, string[]> errors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        ArgumentNullException.ThrowIfNull(errors);

        ParameterName = parameterName;
        Expression = expression;
        Request = request;
        Errors = errors;
    }

    /// <summary>
    /// Gets the query string parameter name that supplied the sort.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the raw sort expression text.
    /// </summary>
    public string? Expression { get; }

    /// <summary>
    /// Gets the parsed sort request when binding succeeded.
    /// </summary>
    public RsqlSortRequest? Request { get; }

    /// <summary>
    /// Gets the validation errors produced while binding.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the request supplied a non-empty sort expression.
    /// </summary>
    public bool IsSpecified => !string.IsNullOrWhiteSpace(Expression);

    /// <summary>
    /// Gets a value indicating whether the sort query has no validation errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether binding produced a sort request.
    /// </summary>
    public bool HasRequest => Request is not null;

    /// <summary>
    /// Parses raw sort text into an <see cref="RsqlSortQuery"/>.
    /// </summary>
    /// <param name="expression">The raw sort expression text.</param>
    /// <param name="parameterName">The query string parameter name that supplied the sort.</param>
    /// <returns>The bound sort query.</returns>
    public static RsqlSortQuery Parse(
        string? expression,
        string parameterName = DefaultSortParameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(expression))
        {
            return new RsqlSortQuery(parameterName, expression, null, new Dictionary<string, string[]>(StringComparer.Ordinal));
        }

        try
        {
            return new RsqlSortQuery(
                parameterName,
                expression,
                RsqlSortRequest.Parse(expression),
                new Dictionary<string, string[]>(StringComparer.Ordinal));
        }
        catch (ArgumentException exception)
        {
            return new RsqlSortQuery(
                parameterName,
                expression,
                null,
                new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    [parameterName] = [exception.Message]
                });
        }
    }

    /// <summary>
    /// Binds a sort request from the current ASP.NET Core request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="parameter">The endpoint parameter being bound.</param>
    /// <returns>The bound sort query.</returns>
    public static ValueTask<RsqlSortQuery> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        var options = context.RequestServices.GetService<IOptions<RsqlSortQueryOptions>>()?.Value
            ?? new RsqlSortQueryOptions();
        var expression = context.Request.Query.TryGetValue(options.SortParameterName, out var values)
            ? values.FirstOrDefault()
            : null;

        return ValueTask.FromResult(Parse(expression, options.SortParameterName));
    }

    /// <summary>
    /// Converts binding errors into the dictionary shape expected by ASP.NET Core validation problem results.
    /// </summary>
    /// <returns>A dictionary keyed by query string parameter name.</returns>
    public Dictionary<string, string[]> ToValidationErrors()
    {
        return Errors.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.Ordinal);
    }
}
