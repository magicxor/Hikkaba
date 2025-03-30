using System.Linq;
using System.Reflection;
using Hikkaba.Common.Enums;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Tests.Unit.Tests;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class LogEventIdsTests
{
    [Test]
    public void EnsureIdsInRange()
    {
        // Get all static fields of type EventId from LogEventIds class
        var eventIds = typeof(LogEventIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(EventId))
            .Select(f => f.GetValue(null) as EventId?)
            .Where(eventId => eventId != null)
            .Select(eventId => eventId!.Value)
            .ToList();

        // Check that all EventIds are greater than or equal to the StartId
        var allEventIdsAreInRange = eventIds.TrueForAll(e => e.Id >= LogEventIds.StartId);

        Assert.That(allEventIdsAreInRange, Is.True);
    }

    [Test]
    public void EnsureNoDuplicateEventIds()
    {
        // Get all static fields of type EventId from LogEventIds class
        var eventIds = typeof(LogEventIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(EventId))
            .Select(f => f.GetValue(null) as EventId?)
            .Where(eventId => eventId != null)
            .Select(eventId => eventId!.Value)
            .ToList();

        // Check for duplicates
        var duplicates = eventIds.GroupBy(e => e.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicates, Is.Empty, $"Duplicate EventIds found: {string.Join(", ", duplicates)}");
    }
}
