using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models;

namespace Hikkaba.Tests.Unit.Tests.Entities;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public sealed class EntityPropertiesTests
{
    private IReadOnlyCollection<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(nameSpace, StringComparison.Ordinal) == true
                        && t is { IsClass: true, IsInterface: false } and { IsAbstract: false, IsNested: false }
                        && t.GetMethods().All(m => m.Name != "<Clone>$"))
            .ToList()
            .AsReadOnly();
    }

    [Test]
    public void AllEntityClasses_GettersAndSetters_ShouldWork()
    {
        // Arrange
        var entityNamespace = typeof(ApplicationUser).Namespace;
        var entityAssembly = typeof(ApplicationUser).Assembly;

        var types = GetTypesInNamespace(entityAssembly, entityNamespace!)
            .ToList();

        // Act & Assert
        foreach (var type in types)
        {
            TestContext.Progress.WriteLine($"Testing entity: {type.Name}");

            // Create an instance of the entity
            var entity = Activator.CreateInstance(type);
            Assert.That(entity, Is.Not.Null, $"Failed to create instance of {type.Name}");

            // Get all properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetMethod?.IsVirtual == false) // Ignore virtual properties
                .ToList();

            foreach (var property in properties)
            {
                TestContext.Progress.WriteLine($"  Testing property: {property.Name}");

                // Skip read-only properties (those without a setter)
                if (property.SetMethod == null || !property.SetMethod.IsPublic)
                {
                    TestContext.Progress.WriteLine($"    Skipping read-only property: {property.Name}");
                    continue;
                }

                // Skip properties with required attribute
                var isRequired = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true).Length != 0;
                if (isRequired && !property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    TestContext.Progress.WriteLine($"    Skipping required reference type property: {property.Name}");
                    continue;
                }

                try
                {
                    // Set a value to the property
                    var testValue = GetTestValue(property.PropertyType);
                    property.SetValue(entity, testValue);

                    // Get the value back and verify it matches
                    var retrievedValue = property.GetValue(entity);
                    Assert.That(retrievedValue, Is.EqualTo(testValue), $"Property {type.Name}.{property.Name} getter/setter failed");
                }
                catch (Exception ex) when (ex is not AssertionException)
                {
                    TestContext.Progress.WriteLine($"    Error testing property {property.Name}: {ex.Message}");
                    Assert.Fail($"Exception while testing {type.Name}.{property.Name}: {ex.Message}");
                }
            }
        }
    }

    private static object? GetTestValue(Type type)
    {
        if (type == typeof(string))
        {
            return "Test String";
        }

        if (type == typeof(int) || type == typeof(int?))
        {
            return 42;
        }

        if (type == typeof(short) || type == typeof(short?))
        {
            return (short)42;
        }

        if (type == typeof(byte) || type == typeof(byte?))
        {
            return (byte)42;
        }

        if (type == typeof(long) || type == typeof(long?))
        {
            return 42L;
        }

        if (type == typeof(decimal) || type == typeof(decimal?))
        {
            return 42.0m;
        }

        if (type == typeof(double) || type == typeof(double?))
        {
            return 42.0;
        }

        if (type == typeof(float) || type == typeof(float?))
        {
            return 42.0f;
        }

        if (type == typeof(bool) || type == typeof(bool?))
        {
            return true;
        }

        if (type == typeof(Guid) || type == typeof(Guid?))
        {
            return Guid.Empty;
        }

        if (type == typeof(DateTime) || type == typeof(DateTime?))
        {
            return new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        if (type == typeof(DateOnly) || type == typeof(DateOnly?))
        {
            return new DateOnly(2023, 1, 1);
        }

        if (type == typeof(TimeOnly) || type == typeof(TimeOnly?))
        {
            return new TimeOnly(12, 0, 0);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
        }

        // if struct enum, return first value
        if (type.IsEnum)
        {
            var enumValues = Enum.GetValues(type);
            return enumValues.Length > 0 ? enumValues.GetValue(0) : null;
        }

        // For other types, return null
        return null;
    }
}
