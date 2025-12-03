using System;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using NUnit.Framework.Constraints;

namespace Hikkaba.Tests.Integration.Extensions;

internal static class CollectionOrderedConstraintExtensions
{
    extension(Is nunitIs)
    {
        public static CollectionOrderedConstraint OrderedBy(string fieldName, OrderByDirection direction)
        {
            return direction switch
            {
                OrderByDirection.Asc => Is.Ordered.Ascending.By(fieldName),
                OrderByDirection.Desc => Is.Ordered.Descending.By(fieldName),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid order by direction"),
            };
        }

        public static CollectionOrderedConstraint OrderedBy(params ReadOnlySpan<OrderByItem> orderByItems)
        {
            if (orderByItems.Length == 0)
            {
                throw new ArgumentException("Order by list cannot be empty", nameof(orderByItems));
            }

            CollectionOrderedConstraint? constraint = null;

            foreach (var orderByItem in orderByItems)
            {
                constraint = constraint is null
                    ? Is.OrderedBy(orderByItem.Field, orderByItem.Direction)
                    : constraint.ThenBy(orderByItem.Field, orderByItem.Direction);
            }

            return constraint ?? throw new InvalidOperationException("Failed to build ordered constraint");
        }
    }

    extension(CollectionOrderedConstraint constraint)
    {
        public CollectionOrderedConstraint ThenBy(string fieldName, OrderByDirection direction)
        {
            return direction switch
            {
                OrderByDirection.Asc => constraint.Then.Ascending.By(fieldName),
                OrderByDirection.Desc => constraint.Then.Descending.By(fieldName),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid order by direction"),
            };
        }
    }
}
