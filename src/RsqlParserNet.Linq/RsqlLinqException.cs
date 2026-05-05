namespace RsqlParserNet.Linq;

/// <summary>
/// Represents an error that occurs while translating RSQL AST nodes to LINQ expressions.
/// </summary>
public sealed class RsqlLinqException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlLinqException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public RsqlLinqException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlLinqException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The exception that caused this translation error.</param>
    public RsqlLinqException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
