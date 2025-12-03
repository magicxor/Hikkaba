using System;
using Hikkaba.Paging.Enums;
using NUnit.Framework.Constraints;

namespace Hikkaba.Tests.Integration.Utils;

internal static class NUnitUtils
{
    public static CollectionOrderedConstraint IsOrderedBy(string fieldName, OrderByDirection direction)
    {
        return direction switch
        {
            OrderByDirection.Asc => Is.Ordered.Ascending.By(fieldName),
            OrderByDirection.Desc => Is.Ordered.Descending.By(fieldName),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }
}
