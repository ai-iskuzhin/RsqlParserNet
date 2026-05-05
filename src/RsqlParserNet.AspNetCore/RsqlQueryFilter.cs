using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Represents an RSQL query string filter bound from an ASP.NET Core request.
/// </summary>
public sealed class RsqlQueryFilter
{
    /// <summary>
    /// The default query string parameter name used for RSQL filters.
    /// </summary>
    public const string DefaultQueryParameterName = "filter";

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlQueryFilter"/> class.
    /// </summary>
    /// <param name="parameterName">The query string parameter name that supplied the filter.</param>
    /// <param name="expression">The raw RSQL expression text from the request.</param>
    /// <param name="query">The parsed query when parsing succeeded.</param>
    /// <param name="diagnostics">The parse diagnostics when parsing failed.</param>
    public RsqlQueryFilter(
        string parameterName,
        string? expression,
        RsqlQuery? query,
        IReadOnlyList<RsqlDiagnostic> diagnostics)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        ArgumentNullException.ThrowIfNull(diagnostics);

        ParameterName = parameterName;
        Expression = expression;
        Query = query;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Gets the query string parameter name that supplied the filter.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the raw RSQL expression text from the request.
    /// </summary>
    public string? Expression { get; }

    /// <summary>
    /// Gets the parsed query when parsing succeeded.
    /// </summary>
    public RsqlQuery? Query { get; }

    /// <summary>
    /// Gets the parse diagnostics when parsing failed.
    /// </summary>
    public IReadOnlyList<RsqlDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets a value indicating whether the request supplied a non-empty filter expression.
    /// </summary>
    public bool IsSpecified => !string.IsNullOrWhiteSpace(Expression);

    /// <summary>
    /// Gets a value indicating whether the filter has no parse diagnostics.
    /// </summary>
    public bool IsValid => Diagnostics.Count == 0;

    /// <summary>
    /// Gets a value indicating whether parsing produced a query.
    /// </summary>
    public bool HasQuery => Query is not null;

    /// <summary>
    /// Parses a raw query string value into an <see cref="RsqlQueryFilter"/>.
    /// </summary>
    /// <param name="expression">The raw RSQL expression text.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <param name="parameterName">The query string parameter name that supplied the filter.</param>
    /// <returns>A bound query filter result.</returns>
    public static RsqlQueryFilter Parse(
        string? expression,
        RsqlParseOptions? parseOptions = null,
        string parameterName = DefaultQueryParameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (string.IsNullOrWhiteSpace(expression))
        {
            return new RsqlQueryFilter(parameterName, expression, null, []);
        }

        var result = RsqlParser.TryParse(expression, parseOptions ?? RsqlParseOptions.Default);
        return new RsqlQueryFilter(parameterName, expression, result.Query, result.Diagnostics);
    }

    /// <summary>
    /// Binds an RSQL query filter from the current ASP.NET Core request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="parameter">The endpoint parameter being bound.</param>
    /// <returns>The bound query filter result.</returns>
    public static ValueTask<RsqlQueryFilter> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        var options = context.RequestServices.GetService<IOptions<RsqlQueryFilterOptions>>()?.Value
            ?? new RsqlQueryFilterOptions();
        var parameterName = ResolveParameterName(parameter, options);
        var expression = context.Request.Query.TryGetValue(parameterName, out var values)
            ? values.FirstOrDefault()
            : null;

        return ValueTask.FromResult(Parse(expression, options.ParseOptions, parameterName));
    }

    /// <summary>
    /// Converts diagnostics into the dictionary shape expected by ASP.NET Core validation problem results.
    /// </summary>
    /// <returns>A dictionary keyed by query string parameter name.</returns>
    public Dictionary<string, string[]> ToValidationErrors()
    {
        return Diagnostics.Count == 0
            ? new Dictionary<string, string[]>(StringComparer.Ordinal)
            : new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [ParameterName] = Diagnostics
                    .Select(diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")
                    .ToArray()
            };
    }

    private static string ResolveParameterName(ParameterInfo parameter, RsqlQueryFilterOptions options)
    {
        return string.IsNullOrWhiteSpace(options.QueryParameterName)
            ? parameter.Name ?? DefaultQueryParameterName
            : options.QueryParameterName;
    }
}
