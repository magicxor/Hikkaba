using System.Linq.Expressions;

namespace Hikkaba.Paging.Utils;

// Adapted from internal System.Web.Util.OrderingMethodFinder http://referencesource.microsoft.com/#System.Web/Util/OrderingMethodFinder.cs
internal sealed class OrderingMethodFinder : ExpressionVisitor
{
    public bool OrderingMethodFound { get; private set; }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var name = node.Method.Name;

        if (node.Method.DeclaringType == typeof(Queryable)
            && (name.StartsWith("OrderBy", StringComparison.Ordinal)
                || name.StartsWith("ThenBy", StringComparison.Ordinal)))
        {
            OrderingMethodFound = true;
        }

        return base.VisitMethodCall(node);
    }
}
