using Hikkaba.Common.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace Hikkaba.Common.Extensions
{
    public static class DateTimeKindAttributeUtils
    {
        public static void ApplyDateTimeKindAttributeToAllPropertiesOf(object entity)
        {
            if (entity == null)
                return;

            // Find all properties that are of type DateTime or DateTime?;
            var properties = entity.GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(DateTime)
                         || x.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                // Check whether these properties have the DateTimeKindAttribute;
                var attr = property.GetCustomAttribute<DateTimeKindAttribute>();
                if (attr == null)
                    continue;

                var dt = property.PropertyType == typeof(DateTime?)
                    ? (DateTime?)property.GetValue(entity)
                    : (DateTime)property.GetValue(entity);

                if (dt == null)
                    continue;

                // If the value is not null set the appropriate DateTimeKind;
                property.SetValue(entity, DateTime.SpecifyKind(dt.Value, attr.Kind));
            }
        }
    }
}
