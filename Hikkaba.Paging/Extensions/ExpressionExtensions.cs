using System.Linq.Expressions;
using Hikkaba.Paging.Utils;

namespace Hikkaba.Paging.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Linq.Expressions.Expression" />.
/// </summary>
/// <seealso href="https://github.com/MapsterMapper/Mapster/blob/master/src/Mapster/Utils/ExpressionEx.cs" />
internal static class ExpressionExtensions
{
    public static bool OrderMethodExists(this Expression expression)
    {
        var visitor = new OrderingMethodFinder();
        visitor.Visit(expression);
        return visitor.OrderingMethodFound;
    }
}
