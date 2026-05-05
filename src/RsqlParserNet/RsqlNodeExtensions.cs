namespace RsqlParserNet;

/// <summary>
/// Provides traversal helpers for RSQL abstract syntax tree nodes.
/// </summary>
public static class RsqlNodeExtensions
{
    /// <summary>
    /// Enumerates this node and all descendant nodes in pre-order.
    /// </summary>
    /// <param name="node">The node to enumerate.</param>
    /// <returns>The node and its descendants.</returns>
    public static IEnumerable<RsqlNode> DescendantsAndSelf(this RsqlNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        yield return node;

        if (node is not RsqlLogicalNode logicalNode)
        {
            yield break;
        }

        foreach (var child in logicalNode.Children)
        {
            foreach (var descendant in child.DescendantsAndSelf())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Enumerates all descendant comparison nodes, including this node when it is a comparison.
    /// </summary>
    /// <param name="node">The node to enumerate.</param>
    /// <returns>The comparison nodes in pre-order.</returns>
    public static IEnumerable<RsqlComparisonNode> Comparisons(this RsqlNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        foreach (var descendant in node.DescendantsAndSelf())
        {
            if (descendant is RsqlComparisonNode comparisonNode)
            {
                yield return comparisonNode;
            }
        }
    }
}
