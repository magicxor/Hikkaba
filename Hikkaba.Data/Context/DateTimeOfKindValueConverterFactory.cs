using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace Hikkaba.Data.Context
{
    public class DateTimeOfKindValueConverterFactory
    {
        private static readonly DateTimeKind[] _dateTimeKinds = (DateTimeKind[])Enum.GetValues(typeof(DateTimeKind));
        private readonly IReadOnlyDictionary<DateTimeKind, ValueConverter<DateTime, DateTime>> _dateTimeValueConverters;
        private readonly IReadOnlyDictionary<DateTimeKind, ValueConverter<DateTime?, DateTime?>> _nullableDateTimeValueConverters;

        public DateTimeOfKindValueConverterFactory()
        {
            var dateTimeValueConverters = new Dictionary<DateTimeKind, ValueConverter<DateTime, DateTime>>(_dateTimeKinds.Length);
            var nullableDateTimeValueConverters = new Dictionary<DateTimeKind, ValueConverter<DateTime?, DateTime?>>(_dateTimeKinds.Length);

            foreach (var dateTimeKind in _dateTimeKinds)
            {
                dateTimeValueConverters.Add(dateTimeKind, new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, dateTimeKind)));
                nullableDateTimeValueConverters.Add(dateTimeKind, new ValueConverter<DateTime?, DateTime?>(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, dateTimeKind) : v));
            }

            _dateTimeValueConverters = dateTimeValueConverters;
            _nullableDateTimeValueConverters = nullableDateTimeValueConverters;
        }

        public ValueConverter<DateTime, DateTime> GetDateTimeValueConverter(DateTimeKind dateTimeKind)
        {
            return _dateTimeValueConverters[dateTimeKind];
        }

        public ValueConverter<DateTime?, DateTime?> GetNullableDateTimeValueConverter(DateTimeKind dateTimeKind)
        {
            return _nullableDateTimeValueConverters[dateTimeKind];
        }
    }
}
