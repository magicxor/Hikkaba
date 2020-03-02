using System;

namespace Hikkaba.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        public DateTimeKindAttribute(DateTimeKind kind)
        {
            Kind = kind;
        }

        public DateTimeKind Kind { get; }
    }
}
