using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RsqlParserNet.Linq;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Represents a page request bound from ASP.NET Core query string parameters.
/// </summary>
public sealed class RsqlPageQuery
{
    /// <summary>
    /// The default query string parameter name used for the one-based page number.
    /// </summary>
    public const string DefaultPageParameterName = "page";

    /// <summary>
    /// The default query string parameter name used for the page size.
    /// </summary>
    public const string DefaultPageSizeParameterName = "pageSize";

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlPageQuery"/> class.
    /// </summary>
    /// <param name="request">The parsed page request when binding succeeded.</param>
    /// <param name="errors">The validation errors produced while binding.</param>
    public RsqlPageQuery(RsqlPageRequest? request, IReadOnlyDictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Request = request;
        Errors = errors;
    }

    /// <summary>
    /// Gets the parsed page request when binding succeeded.
    /// </summary>
    public RsqlPageRequest? Request { get; }

    /// <summary>
    /// Gets the validation errors produced while binding.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the page query has no validation errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Parses raw page and page size values into an <see cref="RsqlPageQuery"/>.
    /// </summary>
    /// <param name="page">The raw one-based page number text.</param>
    /// <param name="pageSize">The raw page size text.</param>
    /// <param name="options">Optional page query binding options.</param>
    /// <returns>The bound page query.</returns>
    public static RsqlPageQuery Parse(string? page, string? pageSize, RsqlPageQueryOptions? options = null)
    {
        var effectiveOptions = options ?? new RsqlPageQueryOptions();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var parsedPage = ParseOptionalInt(page, effectiveOptions.DefaultPage, effectiveOptions.PageParameterName, errors);
        var parsedPageSize = ParseOptionalInt(pageSize, effectiveOptions.DefaultPageSize, effectiveOptions.PageSizeParameterName, errors);

        if (parsedPage < RsqlPageRequest.FirstPage)
        {
            errors[effectiveOptions.PageParameterName] = ["Page must be greater than or equal to 1."];
        }

        if (parsedPageSize < 1)
        {
            errors[effectiveOptions.PageSizeParameterName] = ["Page size must be greater than or equal to 1."];
        }

        if (effectiveOptions.MaxPageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveOptions.MaxPageSize), effectiveOptions.MaxPageSize, "Maximum page size must be greater than or equal to 1.");
        }

        parsedPageSize = Math.Min(parsedPageSize, effectiveOptions.MaxPageSize);

        if (errors.Count > 0)
        {
            return new RsqlPageQuery(null, errors);
        }

        try
        {
            return new RsqlPageQuery(new RsqlPageRequest(parsedPage, parsedPageSize), errors);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            errors[effectiveOptions.PageParameterName] = [exception.Message];
            return new RsqlPageQuery(null, errors);
        }
    }

    /// <summary>
    /// Binds a page request from the current ASP.NET Core request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="parameter">The endpoint parameter being bound.</param>
    /// <returns>The bound page query.</returns>
    public static ValueTask<RsqlPageQuery> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        var options = context.RequestServices.GetService<IOptions<RsqlPageQueryOptions>>()?.Value
            ?? new RsqlPageQueryOptions();
        var page = context.Request.Query.TryGetValue(options.PageParameterName, out var pageValues)
            ? pageValues.FirstOrDefault()
            : null;
        var pageSize = context.Request.Query.TryGetValue(options.PageSizeParameterName, out var pageSizeValues)
            ? pageSizeValues.FirstOrDefault()
            : null;

        return ValueTask.FromResult(Parse(page, pageSize, options));
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

    private static int ParseOptionalInt(
        string? value,
        int defaultValue,
        string parameterName,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (int.TryParse(value, out var parsed))
        {
            return parsed;
        }

        errors[parameterName] = [$"'{value}' is not a valid integer."];
        return defaultValue;
    }
}
