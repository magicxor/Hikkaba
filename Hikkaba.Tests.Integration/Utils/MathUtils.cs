using System;
using System.Diagnostics.Contracts;

namespace Hikkaba.Tests.Integration.Utils;

public static class MathUtils
{
    [Pure]
    public static bool AreEqual(float a, float b, float tolerance = 0.0001f)
    {
        return MathF.Abs(a - b) < tolerance;
    }

    [Pure]
    public static bool AreEqual(double a, double b, double tolerance = 0.0001d)
    {
        return Math.Abs(a - b) < tolerance;
    }
}
